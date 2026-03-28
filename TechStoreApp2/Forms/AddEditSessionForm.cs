using System;
using System.Windows.Forms;
using TechStoreApp.Data;
using TechStoreApp2.Models;

namespace TechStoreApp2.Forms
{
    public partial class AddEditSessionForm : Form
    {
        private DbHelper dbHelper;
        private Session editingSession;
        private int computerId;
        private string computerName;
        private bool isEditing;

        public AddEditSessionForm(int computerId, string computerName)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            this.computerId = computerId;
            this.computerName = computerName;
            isEditing = false;
            this.Text = $"Добавить бронирование для компьютера: {computerName}";
            dtpSessionDate.Value = DateTime.Now;
            nudHours.Minimum = 1;
            nudHours.Maximum = 24;
            nudHours.Value = 1;

            LoadClients();
        }

        public AddEditSessionForm(Session session)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingSession = session;
            this.computerId = session.ComputerId;
            this.computerName = session.Computer?.Name ?? "Компьютер";
            isEditing = true;
            this.Text = $"Редактировать бронирование (компьютер: {computerName})";

            LoadClients();
            FillForm();
        }

        private void LoadClients()
        {
            var clients = dbHelper.GetClients();
            cmbClientName.Items.Clear();
            foreach (var client in clients)
            {
                cmbClientName.Items.Add(client);
            }
            if (cmbClientName.Items.Count > 0)
                cmbClientName.SelectedIndex = 0;
        }

        private void FillForm()
        {
            if (cmbClientName.Items.Contains(editingSession.ClientName))
                cmbClientName.SelectedItem = editingSession.ClientName;
            else if (cmbClientName.Items.Count > 0)
                cmbClientName.SelectedIndex = 0;

            dtpSessionDate.Value = editingSession.SessionDate;
            nudHours.Value = editingSession.Hours;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbClientName.SelectedItem == null || string.IsNullOrWhiteSpace(cmbClientName.SelectedItem.ToString()))
            {
                MessageBox.Show("Выберите клиента", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nudHours.Value <= 0)
            {
                MessageBox.Show("Количество часов должно быть положительным", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Session session = new Session
            {
                ComputerId = computerId,
                ClientName = cmbClientName.SelectedItem.ToString(),
                SessionDate = dtpSessionDate.Value.Date,
                Hours = (int)nudHours.Value
            };

            if (editingSession != null)
            {
                session.Id = editingSession.Id;
            }

            this.Tag = session;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}