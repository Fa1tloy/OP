using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TechStoreApp4.Data;
using TechStoreApp4.Models;

namespace TechStoreApp4.Forms
{
    public partial class AppointmentsForm : Form
    {
        private DbHelper dbHelper;
        private int serviceId;
        private string serviceName;
        private decimal servicePrice;
        private List<Appointment> appointments;
        private User currentUser;

        public AppointmentsForm(int serviceId, string serviceName, decimal servicePrice, User user)
        {
            InitializeComponent();
            this.serviceId = serviceId;
            this.serviceName = serviceName;
            this.servicePrice = servicePrice;
            this.currentUser = user;
            dbHelper = new DbHelper();
            LoadAppointments();

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

        private void LoadAppointments()
        {
            appointments = dbHelper.GetAppointmentsByService(serviceId);
            dgvAppointments.Rows.Clear();

            decimal total = 0;
            foreach (var app in appointments)
            {
                decimal cost = servicePrice * app.SessionsCount;
                total += cost;
                dgvAppointments.Rows.Add(app.Id, app.ClientName, app.AppointmentDate.ToShortDateString(), app.SessionsCount, cost);
            }

            lblTotal.Text = $"Общая сумма записей: {total:C}";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new AddEditAppointmentForm(serviceId, serviceName);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                var newAppointment = addForm.Tag as Appointment;
                if (newAppointment != null)
                {
                    dbHelper.AddAppointment(newAppointment);
                    LoadAppointments();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int appointmentId = Convert.ToInt32(dgvAppointments.SelectedRows[0].Cells[0].Value);
            var appointment = dbHelper.GetAppointmentById(appointmentId);
            if (appointment == null)
            {
                MessageBox.Show("Запись не найдена", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var editForm = new AddEditAppointmentForm(appointment);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                var updatedAppointment = editForm.Tag as Appointment;
                if (updatedAppointment != null)
                {
                    dbHelper.UpdateAppointment(updatedAppointment);
                    LoadAppointments();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int appointmentId = Convert.ToInt32(dgvAppointments.SelectedRows[0].Cells[0].Value);
            string clientName = dgvAppointments.SelectedRows[0].Cells[1].Value.ToString();

            if (MessageBox.Show($"Удалить запись клиента {clientName}?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dbHelper.DeleteAppointment(appointmentId);
                LoadAppointments();
            }
        }
    }
}