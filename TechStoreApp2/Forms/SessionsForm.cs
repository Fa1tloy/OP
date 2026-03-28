using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TechStoreApp.Data;

using TechStoreApp2.Models;

namespace TechStoreApp2.Forms
{
    public partial class SessionsForm : Form
    {
        private DbHelper dbHelper;
        private int computerId;
        private string computerName;
        private decimal computerPrice;
        private List<Session> sessions;
        private User currentUser;

        public SessionsForm(int computerId, string computerName, decimal computerPrice, User user)
        {
            InitializeComponent();
            this.computerId = computerId;
            this.computerName = computerName;
            this.computerPrice = computerPrice;
            this.currentUser = user;
            dbHelper = new DbHelper();
            LoadSessions();

            if (currentUser == null || currentUser.Role.Name == "client")
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
            }
            else if (currentUser.Role.Name == "admin")
            {
                btnAdd.Visible = true;
                btnEdit.Visible = true;
                btnDelete.Visible = true;
            }
        }

        private void LoadSessions()
        {
            sessions = dbHelper.GetSessionsByComputer(computerId);
            dgvSessions.Rows.Clear();

            decimal total = 0;
            foreach (var session in sessions)
            {
                decimal cost = computerPrice * session.Hours;
                total += cost;
                dgvSessions.Rows.Add(session.Id, session.ClientName, session.SessionDate.ToShortDateString(), session.Hours, cost);
            }

            lblTotal.Text = $"Общая сумма заказов: {total:C}";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new AddEditSessionForm(computerId, computerName);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                var newSession = addForm.Tag as Session;
                if (newSession != null)
                {
                    dbHelper.AddSession(newSession);
                    LoadSessions();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сеанс для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int sessionId = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells[0].Value);
            var session = dbHelper.GetSessionById(sessionId);
            if (session == null)
            {
                MessageBox.Show("Сеанс не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var editForm = new AddEditSessionForm(session);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                var updatedSession = editForm.Tag as Session;
                if (updatedSession != null)
                {
                    dbHelper.UpdateSession(updatedSession);
                    LoadSessions();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сеанс для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int sessionId = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells[0].Value);
            string clientName = dgvSessions.SelectedRows[0].Cells[1].Value.ToString();

            if (MessageBox.Show($"Удалить сеанс клиента {clientName}?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dbHelper.DeleteSession(sessionId);
                LoadSessions();
            }
        }
    }
}