using System;
using System.Windows.Forms;
using GamePlatformApp.Data;
using GamePlatformApp.Models;

namespace GamePlatformApp.Forms
{
    public partial class AddEditOrderForm : Form
    {
        private DbHelper dbHelper;
        private Order editingOrder;
        private int gameId;
        private string gameName;
        private bool isEditing;

        public AddEditOrderForm(int gameId, string gameName)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            this.gameId = gameId;
            this.gameName = gameName;
            isEditing = false;
            this.Text = $"Добавить заказ для игры: {gameName}";
            dtpOrderDate.Value = DateTime.Now;
            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = 999;
            nudQuantity.Value = 1;

            LoadClients();
        }

        public AddEditOrderForm(Order order)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingOrder = order;
            this.gameId = order.GameId;
            this.gameName = order.Game?.Name ?? "Игра";
            isEditing = true;
            this.Text = $"Редактировать заказ (игра: {gameName})";

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
            if (cmbClientName.Items.Contains(editingOrder.ClientName))
                cmbClientName.SelectedItem = editingOrder.ClientName;
            else if (cmbClientName.Items.Count > 0)
                cmbClientName.SelectedIndex = 0;

            dtpOrderDate.Value = editingOrder.OrderDate;
            nudQuantity.Value = editingOrder.Quantity;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbClientName.SelectedItem == null || string.IsNullOrWhiteSpace(cmbClientName.SelectedItem.ToString()))
            {
                MessageBox.Show("Выберите клиента", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nudQuantity.Value <= 0)
            {
                MessageBox.Show("Количество должно быть положительным", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Order order = new Order
            {
                GameId = gameId,
                ClientName = cmbClientName.SelectedItem.ToString(),
                OrderDate = dtpOrderDate.Value.Date,
                Quantity = (int)nudQuantity.Value
            };

            if (editingOrder != null)
            {
                order.Id = editingOrder.Id;
            }

            this.Tag = order;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}