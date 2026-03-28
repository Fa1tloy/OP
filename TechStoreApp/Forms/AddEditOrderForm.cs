using System;
using System.Windows.Forms;
using TechStoreApp.Data;
using TechStoreApp.Models;

namespace TechStoreApp.Forms
{
    public partial class AddEditOrderForm : Form
    {
        private DbHelper dbHelper;
        private Order editingOrder;
        private int productId;
        private string productName;
        private bool isEditing;

        // Конструктор для добавления нового заказа
        public AddEditOrderForm(int productId, string productName)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            this.productId = productId;
            this.productName = productName;
            isEditing = false;
            this.Text = $"Добавить заказ для товара: {productName}";
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
            this.productId = order.ProductId;
            this.productName = order.Product?.Name ?? "Товар";
            isEditing = true;
            this.Text = $"Редактировать заказ (товар: {productName})";

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
            // Устанавливаем выбранного клиента, если он есть в списке
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
                ProductId = productId,
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