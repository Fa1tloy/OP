using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TechStoreApp.Data;

using TechStoreApp2.Forms;
using TechStoreApp2.Models;

namespace TechStoreApp2.Forms
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private DbHelper dbHelper;
        private List<Computer> currentComputers;
        private Computer selectedComputer;
        private int? lastSelectedComputerId = null;

        public MainForm(User user)
        {
            InitializeComponent();
            currentUser = user;
            dbHelper = new DbHelper();
            currentComputers = new List<Computer>();

            if (currentUser == null) // Гость
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

            LoadCategoriesAndComputers();
        }

        private void LoadCategoriesAndComputers()
        {
            treeViewComputers.Nodes.Clear();
            var categories = dbHelper.GetCategories();
            currentComputers = dbHelper.GetAllComputers();

            TreeNode selectedNode = null;
            foreach (var cat in categories)
            {
                TreeNode catNode = new TreeNode(cat.Name) { Tag = cat };
                foreach (var comp in currentComputers.FindAll(c => c.CategoryId == cat.Id))
                {
                    TreeNode compNode = new TreeNode(comp.Name) { Tag = comp };
                    catNode.Nodes.Add(compNode);
                    if (lastSelectedComputerId.HasValue && comp.Id == lastSelectedComputerId.Value)
                        selectedNode = compNode;
                }
                treeViewComputers.Nodes.Add(catNode);
                if (selectedNode != null && catNode.Nodes.Contains(selectedNode))
                    treeViewComputers.SelectedNode = selectedNode;
            }
            if (selectedNode != null)
                treeViewComputers.SelectedNode = selectedNode;
            else
                panelCard.Controls.Clear();
        }

        private void treeViewComputers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Computer comp)
            {
                selectedComputer = comp;
                ShowComputerCard(comp);
            }
        }

        private void ShowComputerCard(Computer computer)
        {
            panelCard.Controls.Clear();

            // Фото
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrEmpty(computer.Photo) && File.Exists(computer.Photo))
                pictureBox.Image = Image.FromFile(computer.Photo);
            else
                pictureBox.Image = Properties.Resources.no_image;
            panelCard.Controls.Add(pictureBox);

            // Название
            Label lblName = new Label
            {
                Text = computer.Name,
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
            lstProps.Items.Add($"Характеристики: {computer.Specs ?? "не указаны"}");
            lstProps.Items.Add($"Цена за час: {computer.PricePerHour:C}");
            lstProps.Items.Add($"Вес: {computer.Weight ?? "не указан"}");
            lstProps.Items.Add($"Категория: {computer.Category.Name}");
            panelCard.Controls.Add(lstProps);

            // Кнопка "Бронирования"
            Button btnSessions = new Button
            {
                Text = "Просмотр бронирований",
                Location = new Point(10, 390),
                Size = new Size(200, 30)
            };
            btnSessions.Click += (s, e) => ShowSessionsForm(computer);
            panelCard.Controls.Add(btnSessions);
        }

        private void ShowSessionsForm(Computer computer)
        {
            SessionsForm sessionsForm = new SessionsForm(computer.Id, computer.Name, computer.PricePerHour, currentUser);
            sessionsForm.ShowDialog();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadCategoriesAndComputers();
                return;
            }

            var found = dbHelper.SearchComputers(searchText);
            treeViewComputers.Nodes.Clear();
            TreeNode searchNode = new TreeNode($"Результаты поиска: {searchText}");
            foreach (var comp in found)
            {
                TreeNode compNode = new TreeNode(comp.Name) { Tag = comp };
                searchNode.Nodes.Add(compNode);
            }
            treeViewComputers.Nodes.Add(searchNode);
            searchNode.Expand();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddEditComputerForm addForm = new AddEditComputerForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadCategoriesAndComputers();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedComputer == null)
            {
                MessageBox.Show("Выберите компьютер для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AddEditComputerForm editForm = new AddEditComputerForm(selectedComputer);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                lastSelectedComputerId = selectedComputer.Id;
                LoadCategoriesAndComputers();
                lastSelectedComputerId = null;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedComputer == null)
            {
                MessageBox.Show("Выберите компьютер для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dbHelper.HasSessions(selectedComputer.Id))
            {
                MessageBox.Show("Невозможно удалить компьютер, так как на него есть бронирования.\n" +
                                "Сначала удалите все бронирования.", "Ошибка удаления",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Удалить компьютер \"{selectedComputer.Name}\"?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    dbHelper.DeleteComputer(selectedComputer.Id);
                    LoadCategoriesAndComputers();
                    selectedComputer = null;
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