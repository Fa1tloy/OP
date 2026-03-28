using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GamePlatformApp.Data;
using GamePlatformApp.Models;

namespace GamePlatformApp.Forms
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private DbHelper dbHelper;
        private List<Game> currentGames;
        private Game selectedGame;
        private int? lastSelectedGameId = null;

        public MainForm(User user)
        {
            InitializeComponent();
            currentUser = user;
            dbHelper = new DbHelper();
            currentGames = new List<Game>();

            if (currentUser == null) // гость
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                txtSearch.Enabled = false;
                btnSearch.Enabled = false;
            }
            else if (currentUser.Role.Name == "client")
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                txtSearch.Enabled = true;
                btnSearch.Enabled = true;
            }
            else if (currentUser.Role.Name == "admin")
            {
                btnAdd.Visible = true;
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                txtSearch.Enabled = true;
                btnSearch.Enabled = true;
            }

            LoadCategoriesAndGames();
        }

        private void LoadCategoriesAndGames()
        {
            treeViewGames.Nodes.Clear();
            var categories = dbHelper.GetCategories();
            currentGames = dbHelper.GetAllGames();

            TreeNode selectedNode = null;
            foreach (var cat in categories)
            {
                TreeNode catNode = new TreeNode(cat.Name) { Tag = cat };
                foreach (var game in currentGames.FindAll(g => g.CategoryId == cat.Id))
                {
                    TreeNode gameNode = new TreeNode(game.Name) { Tag = game };
                    catNode.Nodes.Add(gameNode);
                    if (lastSelectedGameId.HasValue && game.Id == lastSelectedGameId.Value)
                        selectedNode = gameNode;
                }
                treeViewGames.Nodes.Add(catNode);
                if (selectedNode != null && catNode.Nodes.Contains(selectedNode))
                    treeViewGames.SelectedNode = selectedNode;
            }
            if (selectedNode != null)
                treeViewGames.SelectedNode = selectedNode;
            else
                panelCard.Controls.Clear();
        }

        private void treeViewGames_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Game game)
            {
                selectedGame = game;
                ShowGameCard(game);
            }
        }

        private void ShowGameCard(Game game)
        {
            panelCard.Controls.Clear();

            // Фото
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrEmpty(game.Photo) && File.Exists(game.Photo))
                pictureBox.Image = Image.FromFile(game.Photo);
            else
                pictureBox.Image = Properties.Resources.no_image; // добавьте заглушку в ресурсы
            panelCard.Controls.Add(pictureBox);

            // Название
            Label lblName = new Label
            {
                Text = game.Name,
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
            lstProps.Items.Add($"Разработчик: {game.Developer ?? "не указан"}");
            lstProps.Items.Add($"Год выпуска: {(game.ReleaseYear.HasValue ? game.ReleaseYear.ToString() : "не указан")}");
            lstProps.Items.Add($"Цена: {game.Price:C}");
            lstProps.Items.Add($"Мощность: {game.Power ?? "не указана"}");
            lstProps.Items.Add($"Жанр: {game.Category.Name}");
            panelCard.Controls.Add(lstProps);

            // Кнопка "Заказы"
            Button btnOrders = new Button
            {
                Text = "Просмотр заказов",
                Location = new Point(10, 390),
                Size = new Size(150, 30)
            };
            btnOrders.Click += (s, e) => ShowOrdersForm(game);
            panelCard.Controls.Add(btnOrders);
        }

        private void ShowOrdersForm(Game game)
        {
            OrdersForm ordersForm = new OrdersForm(game.Id, game.Name, game.Price, currentUser);
            ordersForm.ShowDialog();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadCategoriesAndGames();
                return;
            }

            var found = dbHelper.SearchGames(searchText);
            treeViewGames.Nodes.Clear();
            TreeNode searchNode = new TreeNode($"Результаты поиска: {searchText}");
            foreach (var game in found)
            {
                TreeNode gameNode = new TreeNode(game.Name) { Tag = game };
                searchNode.Nodes.Add(gameNode);
            }
            treeViewGames.Nodes.Add(searchNode);
            searchNode.Expand();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddEditGameForm addForm = new AddEditGameForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadCategoriesAndGames();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedGame == null)
            {
                MessageBox.Show("Выберите игру для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AddEditGameForm editForm = new AddEditGameForm(selectedGame);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                lastSelectedGameId = selectedGame.Id;
                LoadCategoriesAndGames();
                lastSelectedGameId = null;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedGame == null)
            {
                MessageBox.Show("Выберите игру для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (dbHelper.HasOrders(selectedGame.Id))
                {
                    MessageBox.Show("Невозможно удалить игру, так как на неё есть заказы.\n" +
                                    "Сначала удалите все заказы на эту игру.", "Ошибка удаления",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке заказов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Удалить игру \"{selectedGame.Name}\"?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    dbHelper.DeleteGame(selectedGame.Id);
                    LoadCategoriesAndGames();
                    selectedGame = null;
                    panelCard.Controls.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}