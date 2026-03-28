using System;
using System.Windows.Forms;
using TechStoreApp4.Data;
using TechStoreApp4.Models;

namespace TechStoreApp4.Forms
{
    public partial class AddEditAppointmentForm : Form
    {
        private DbHelper dbHelper;
        private Appointment editingAppointment;
        private int serviceId;
        private string serviceName;
        private bool isEditing;

        public AddEditAppointmentForm(int serviceId, string serviceName)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            this.serviceId = serviceId;
            this.serviceName = serviceName;
            isEditing = false;
            this.Text = $"Добавить запись для услуги: {serviceName}";
            dtpAppointmentDate.Value = DateTime.Now;
            nudSessionsCount.Minimum = 1;
            nudSessionsCount.Maximum = 999;
            nudSessionsCount.Value = 1;

            LoadClients();
        }

        public AddEditAppointmentForm(Appointment appointment)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingAppointment = appointment;
            this.serviceId = appointment.ServiceId;
            this.serviceName = appointment.Service?.Name ?? "Услуга";
            isEditing = true;
            this.Text = $"Редактировать запись (услуга: {serviceName})";

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
            if (cmbClientName.Items.Contains(editingAppointment.ClientName))
                cmbClientName.SelectedItem = editingAppointment.ClientName;
            else if (cmbClientName.Items.Count > 0)
                cmbClientName.SelectedIndex = 0;

            dtpAppointmentDate.Value = editingAppointment.AppointmentDate;
            nudSessionsCount.Value = editingAppointment.SessionsCount;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbClientName.SelectedItem == null || string.IsNullOrWhiteSpace(cmbClientName.SelectedItem.ToString()))
            {
                MessageBox.Show("Выберите клиента", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nudSessionsCount.Value <= 0)
            {
                MessageBox.Show("Количество сессий должно быть положительным", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Appointment appointment = new Appointment
            {
                ServiceId = serviceId,
                ClientName = cmbClientName.SelectedItem.ToString(),
                AppointmentDate = dtpAppointmentDate.Value.Date,
                SessionsCount = (int)nudSessionsCount.Value
            };

            if (editingAppointment != null)
            {
                appointment.Id = editingAppointment.Id;
            }

            this.Tag = appointment;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}