using System;
using System.Windows.Forms;
using TechStoreApp3.Data;
using TechStoreApp3.Models;
using TechStoreApp3.Data;

namespace TechStoreApp3.Forms
{
    public partial class AddEditOrderForm : Form
    {
        private DbHelper dbHelper;
        private Order editingOrder;
        private int dishId;
        private string dishName;
        private bool isEditing;

        // Конструктор для добавления нового заказа
        public AddEditOrderForm(int dishId, string dishName)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            this.dishId = dishId;
            this.dishName = dishName;
            isEditing = false;
            this.Text = $"Добавить заказ для блюда: {dishName}";
            dtpOrderDate.Value = DateTime.Now;
            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = 999;
            nudQuantity.Value = 1;

            LoadClients();
        }

        // Конструктор для редактирования существующего заказа
        public AddEditOrderForm(Order order)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingOrder = order;
            this.dishId = order.DishId;
            this.dishName = order.Dish?.Name ?? "Блюдо";
            isEditing = true;
            this.Text = $"Редактировать заказ (блюдо: {dishName})";

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
                DishId = dishId,
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