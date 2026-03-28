using System;
using System.Collections.Generic;
using System.Windows.Forms;

using TechStoreApp3.Data;
using TechStoreApp3.Forms;
using TechStoreApp3.Models;

namespace TechStoreApp3.Forms
{
    public partial class OrdersForm : Form
    {
        private DbHelper dbHelper;
        private int dishId;
        private string dishName;
        private decimal dishPrice;
        private List<Order> orders;
        private User currentUser;

        public OrdersForm(int dishId, string dishName, decimal dishPrice, User user)
        {
            InitializeComponent();
            this.dishId = dishId;
            this.dishName = dishName;
            this.dishPrice = dishPrice;
            this.currentUser = user;
            dbHelper = new DbHelper();

            this.Load += OrdersForm_Load; // подписываемся на событие загрузки
        }

        private void OrdersForm_Load(object sender, EventArgs e)
        {
            LoadOrders();

            // Настройка видимости кнопок в зависимости от роли
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
            orders = dbHelper.GetOrdersByDish(dishId); // ← получаем список
            if (orders == null) return;

            dgvOrders.Rows.Clear();

            decimal total = 0;
            foreach (var order in orders) // теперь orders точно не null
            {
                decimal cost = dishPrice * order.Quantity;
                total += cost;
                dgvOrders.Rows.Add(order.Id, order.ClientName, order.OrderDate.ToShortDateString(), order.Quantity, cost);
            }

            lblTotal.Text = $"Общая сумма заказов: {total:C}";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new AddEditOrderForm(dishId, dishName);
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