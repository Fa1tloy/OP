using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TechStoreApp3.Data;
using TechStoreApp3.Models;


namespace TechStoreApp3.Forms
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private DbHelper dbHelper;
        private List<Dish> currentDishes;
        private Dish selectedDish;
        private int? lastSelectedDishId = null;

        public MainForm(User user)
        {
            InitializeComponent();
            currentUser = user;
            dbHelper = new DbHelper();
            currentDishes = new List<Dish>();

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

            LoadCategoriesAndDishes();
        }

        private void LoadCategoriesAndDishes()
        {
            treeViewDishes.Nodes.Clear();
            var categories = dbHelper.GetCategories();
            currentDishes = dbHelper.GetAllDishes();

            TreeNode selectedNode = null;
            foreach (var cat in categories)
            {
                TreeNode catNode = new TreeNode(cat.Name) { Tag = cat };
                foreach (var dish in currentDishes.FindAll(d => d.CategoryId == cat.Id))
                {
                    TreeNode dishNode = new TreeNode(dish.Name) { Tag = dish };
                    catNode.Nodes.Add(dishNode);
                    if (lastSelectedDishId.HasValue && dish.Id == lastSelectedDishId.Value)
                        selectedNode = dishNode;
                }
                treeViewDishes.Nodes.Add(catNode);
                if (selectedNode != null && catNode.Nodes.Contains(selectedNode))
                    treeViewDishes.SelectedNode = selectedNode;
            }
            if (selectedNode != null)
                treeViewDishes.SelectedNode = selectedNode;
            else
                panelCard.Controls.Clear();
        }

        private void treeViewDishes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Dish dish)
            {
                selectedDish = dish;
                ShowDishCard(dish);
            }
        }

        private void ShowDishCard(Dish dish)
        {
            panelCard.Controls.Clear();

            // Фото
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrEmpty(dish.Photo) && File.Exists(dish.Photo))
                pictureBox.Image = Image.FromFile(dish.Photo);
            else
                pictureBox.Image = Properties.Resources.no_image; // заглушка
            panelCard.Controls.Add(pictureBox);

            // Название
            Label lblName = new Label
            {
                Text = dish.Name,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(10, 220),
                AutoSize = true
            };
            panelCard.Controls.Add(lblName);

            // Характеристики списком
            ListBox lstProps = new ListBox
            {
                Location = new Point(10, 260),
                Width = 350,
                Height = 120
            };
            lstProps.Items.Add($"Описание: {dish.Description ?? "не указано"}");
            lstProps.Items.Add($"Цена: {dish.Price:C}");
            lstProps.Items.Add($"Вес: {dish.Weight ?? "не указан"}");
            lstProps.Items.Add($"Категория: {dish.Category.Name}");
            panelCard.Controls.Add(lstProps);

            // Кнопка "Заказы"
            Button btnOrders = new Button
            {
                Text = "Просмотр заказов",
                Location = new Point(10, 390),
                Size = new Size(150, 30)
            };
            btnOrders.Click += (s, e) => ShowOrdersForm(dish);
            panelCard.Controls.Add(btnOrders);
        }

        private void ShowOrdersForm(Dish dish)
        {
            OrdersForm ordersForm = new OrdersForm(dish.Id, dish.Name, dish.Price, currentUser);
            ordersForm.ShowDialog();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadCategoriesAndDishes();
                return;
            }

            var found = dbHelper.SearchDishes(searchText);
            treeViewDishes.Nodes.Clear();
            TreeNode searchNode = new TreeNode($"Результаты поиска: {searchText}");
            foreach (var dish in found)
            {
                TreeNode dishNode = new TreeNode(dish.Name) { Tag = dish };
                searchNode.Nodes.Add(dishNode);
            }
            treeViewDishes.Nodes.Add(searchNode);
            searchNode.Expand();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddEditDishForm addForm = new AddEditDishForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadCategoriesAndDishes();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedDish == null)
            {
                MessageBox.Show("Выберите блюдо для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AddEditDishForm editForm = new AddEditDishForm(selectedDish);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                lastSelectedDishId = selectedDish.Id;
                LoadCategoriesAndDishes();
                lastSelectedDishId = null;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedDish == null)
            {
                MessageBox.Show("Выберите блюдо для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (dbHelper.HasOrders(selectedDish.Id))
                {
                    MessageBox.Show("Невозможно удалить блюдо, так как на него есть заказы.\n" +
                                    "Сначала удалите все заказы на это блюдо.", "Ошибка удаления",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке заказов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Удалить блюдо \"{selectedDish.Name}\"?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    dbHelper.DeleteDish(selectedDish.Id);
                    LoadCategoriesAndDishes();
                    selectedDish = null;
                    panelCard.Controls.Clear();
                }
                catch (MySqlException ex) when (ex.Message.Contains("foreign key constraint fails"))
                {
                    MessageBox.Show("Невозможно удалить блюдо, так как на него есть заказы.\n" +
                                    "Сначала удалите все заказы на это блюдо.", "Ошибка удаления",
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