using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GamePlatformApp.Data;
using GamePlatformApp.Models;

namespace GamePlatformApp.Forms
{
    public partial class OrdersForm : Form
    {
        private DbHelper dbHelper;
        private int gameId;
        private string gameName;
        private decimal gamePrice;
        private List<Order> orders;
        private User currentUser;

        public OrdersForm(int gameId, string gameName, decimal gamePrice, User user)
        {
            InitializeComponent();
            this.gameId = gameId;
            this.gameName = gameName;
            this.gamePrice = gamePrice;
            this.currentUser = user;
            dbHelper = new DbHelper();
            LoadOrders();

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

        private void LoadOrders()
        {
            orders = dbHelper.GetOrdersByGame(gameId);
            dgvOrders.Rows.Clear();

            decimal total = 0;
            foreach (var order in orders)
            {
                decimal cost = gamePrice * order.Quantity;
                total += cost;
                dgvOrders.Rows.Add(order.Id, order.ClientName, order.OrderDate.ToShortDateString(), order.Quantity, cost);
            }

            lblTotal.Text = $"Общая сумма заказов: {total:C}";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new AddEditOrderForm(gameId, gameName);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                var newOrder = addForm.Tag as Order;
                if (newOrder != null)
                {
                    dbHelper.AddOrder(newOrder);
                    LoadOrders();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells[0].Value);
            var order = dbHelper.GetOrderById(orderId);
            if (order == null)
            {
                MessageBox.Show("Заказ не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var editForm = new AddEditOrderForm(order);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                var updatedOrder = editForm.Tag as Order;
                if (updatedOrder != null)
                {
                    dbHelper.UpdateOrder(updatedOrder);
                    LoadOrders();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells[0].Value);
            string clientName = dgvOrders.SelectedRows[0].Cells[1].Value.ToString();

            if (MessageBox.Show($"Удалить заказ клиента {clientName}?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dbHelper.DeleteOrder(orderId);
                LoadOrders();
            }
        }
    }
}