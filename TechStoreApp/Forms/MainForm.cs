using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TechStoreApp.Data;
using TechStoreApp.Models;

namespace TechStoreApp.Forms
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private DbHelper dbHelper;
        private List<Product> currentProducts;   // текущий список товаров (для поиска и обновления)
        private Product selectedProduct;          // выбранный товар
        private int? lastSelectedProductId = null; // новое поле
        public MainForm(User user)
        {
            InitializeComponent();
            currentUser = user;
            dbHelper = new DbHelper();
            currentProducts = new List<Product>();

            // Настройка интерфейса в зависимости от роли
            if (currentUser == null)
            {
                // Гость
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                txtSearch.Enabled = false;
                btnSearch.Enabled = false;
            }
            else if (currentUser.Role.Name == "client")
            {
                // Клиент
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                txtSearch.Enabled = true;
                btnSearch.Enabled = true;
            }
            else if (currentUser.Role.Name == "admin")
            {
                // Администратор
                btnAdd.Visible = true;
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                txtSearch.Enabled = true;
                btnSearch.Enabled = true;
            }

            LoadCategoriesAndProducts();
        }

        private void LoadCategoriesAndProducts()
        {
            treeViewProducts.Nodes.Clear();
            var categories = dbHelper.GetCategories();
            currentProducts = dbHelper.GetAllProducts();

            TreeNode selectedNode = null;
            foreach (var cat in categories)
            {
                TreeNode catNode = new TreeNode(cat.Name) { Tag = cat };
                foreach (var prod in currentProducts.FindAll(p => p.CategoryId == cat.Id))
                {
                    TreeNode prodNode = new TreeNode(prod.Name) { Tag = prod };
                    catNode.Nodes.Add(prodNode);
                    if (lastSelectedProductId.HasValue && prod.Id == lastSelectedProductId.Value)
                        selectedNode = prodNode;
                }
                treeViewProducts.Nodes.Add(catNode);
                if (selectedNode != null && catNode.Nodes.Contains(selectedNode))
                    treeViewProducts.SelectedNode = selectedNode;
            }
            if (selectedNode != null)
                treeViewProducts.SelectedNode = selectedNode;
            else
                panelCard.Controls.Clear();
        }

        private void treeViewProducts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Product prod)
            {
                selectedProduct = prod;
                ShowProductCard(prod);
            }
        }

        private void ShowProductCard(Product product)
        {
            // Очистка панели карточки
            panelCard.Controls.Clear();

            // Фото
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrEmpty(product.Photo) && File.Exists(product.Photo))
                pictureBox.Image = Image.FromFile(product.Photo);
            else
                pictureBox.Image = Properties.Resources.no_image; // заглушка (добавить в ресурсы)
            panelCard.Controls.Add(pictureBox);

            // Название
            Label lblName = new Label
            {
                Text = product.Name,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(10, 220),
                AutoSize = true
            };
            panelCard.Controls.Add(lblName);

            // Характеристики
            ListBox lstProps = new ListBox
            {
                Location = new Point(10, 260),
                Width = 350,
                Height = 120
            };
            lstProps.Items.Add($"Бренд: {product.Brand ?? "не указан"}");
            lstProps.Items.Add($"Цена: {product.Price:C}");
            lstProps.Items.Add($"Мощность: {product.Power ?? "не указана"}");
            lstProps.Items.Add($"Категория: {product.Category.Name}");
            panelCard.Controls.Add(lstProps);

            // Кнопка "Заказы"
            Button btnOrders = new Button
            {
                Text = "Просмотр заказов",
                Location = new Point(10, 390),
                Size = new Size(150, 30)
            };
            btnOrders.Click += (s, e) => ShowOrdersForm(product);
            panelCard.Controls.Add(btnOrders);
        }

        private void ShowOrdersForm(Product product)
        {
            OrdersForm ordersForm = new OrdersForm(product.Id, product.Name, product.Price, currentUser);
            ordersForm.ShowDialog();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadCategoriesAndProducts();
                return;
            }

            var found = dbHelper.SearchProducts(searchText);
            treeViewProducts.Nodes.Clear();
            TreeNode searchNode = new TreeNode($"Результаты поиска: {searchText}");
            foreach (var prod in found)
            {
                TreeNode prodNode = new TreeNode(prod.Name) { Tag = prod };
                searchNode.Nodes.Add(prodNode);
            }
            treeViewProducts.Nodes.Add(searchNode);
            searchNode.Expand();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddEditProductForm addForm = new AddEditProductForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadCategoriesAndProducts();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AddEditProductForm editForm = new AddEditProductForm(selectedProduct);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                lastSelectedProductId = selectedProduct.Id; // запоминаем ID
                LoadCategoriesAndProducts();                // перезагружаем дерево
                lastSelectedProductId = null;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Проверка наличия заказов
                if (dbHelper.HasOrders(selectedProduct.Id))
                {
                    MessageBox.Show("Невозможно удалить товар, так как на него есть заказы.\n" +
                                    "Сначала удалите все заказы на этот товар.", "Ошибка удаления",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке заказов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Удалить товар \"{selectedProduct.Name}\"?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    dbHelper.DeleteProduct(selectedProduct.Id);
                    LoadCategoriesAndProducts();    // перезагружаем список товаров
                    selectedProduct = null;
                    panelCard.Controls.Clear();
                }
                catch (MySqlException ex) when (ex.Message.Contains("foreign key constraint fails"))
                {
                    // Если по какой-то причине проверка пропустила заказы, показываем сообщение
                    MessageBox.Show("Невозможно удалить товар, так как на него есть заказы.\n" +
                                    "Сначала удалите все заказы на этот товар.", "Ошибка удаления",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}