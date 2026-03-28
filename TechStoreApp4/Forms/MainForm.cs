using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TechStoreApp4.Data;
using TechStoreApp4.Models;

namespace TechStoreApp4.Forms
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private DbHelper dbHelper;
        private List<Service> currentServices;
        private Service selectedService;
        private int? lastSelectedServiceId = null;

        public MainForm(User user)
        {
            InitializeComponent();
            currentUser = user;
            dbHelper = new DbHelper();
            currentServices = new List<Service>();

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

            LoadCategoriesAndServices();
        }

        private void LoadCategoriesAndServices()
        {
            treeViewServices.Nodes.Clear();
            var categories = dbHelper.GetCategories();
            currentServices = dbHelper.GetAllServices();

            TreeNode selectedNode = null;
            foreach (var cat in categories)
            {
                TreeNode catNode = new TreeNode(cat.Name) { Tag = cat };
                foreach (var serv in currentServices.FindAll(s => s.CategoryId == cat.Id))
                {
                    TreeNode servNode = new TreeNode(serv.Name) { Tag = serv };
                    catNode.Nodes.Add(servNode);
                    if (lastSelectedServiceId.HasValue && serv.Id == lastSelectedServiceId.Value)
                        selectedNode = servNode;
                }
                treeViewServices.Nodes.Add(catNode);
                if (selectedNode != null && catNode.Nodes.Contains(selectedNode))
                    treeViewServices.SelectedNode = selectedNode;
            }
            if (selectedNode != null)
                treeViewServices.SelectedNode = selectedNode;
            else
                panelCard.Controls.Clear();
        }

        private void treeViewServices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Service serv)
            {
                selectedService = serv;
                ShowServiceCard(serv);
            }
        }

        private void ShowServiceCard(Service service)
        {
            panelCard.Controls.Clear();

            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrEmpty(service.Photo) && File.Exists(service.Photo))
                pictureBox.Image = Image.FromFile(service.Photo);
            else
                pictureBox.Image = Properties.Resources.no_image; // заглушка
            panelCard.Controls.Add(pictureBox);

            Label lblName = new Label
            {
                Text = service.Name,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(10, 220),
                AutoSize = true
            };
            panelCard.Controls.Add(lblName);

            ListBox lstProps = new ListBox
            {
                Location = new Point(10, 260),
                Width = 350,
                Height = 120
            };
            lstProps.Items.Add($"Описание: {service.Description ?? "не указано"}");
            lstProps.Items.Add($"Цена: {service.Price:C}");
            lstProps.Items.Add($"Длительность: {service.Duration ?? "не указана"}");
            lstProps.Items.Add($"Категория: {service.Category.Name}");
            panelCard.Controls.Add(lstProps);

            Button btnAppointments = new Button
            {
                Text = "Просмотр записей",
                Location = new Point(10, 390),
                Size = new Size(150, 30)
            };
            btnAppointments.Click += (s, e) => ShowAppointmentsForm(service);
            panelCard.Controls.Add(btnAppointments);
        }

        private void ShowAppointmentsForm(Service service)
        {
            AppointmentsForm appointmentsForm = new AppointmentsForm(service.Id, service.Name, service.Price, currentUser);
            appointmentsForm.ShowDialog();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadCategoriesAndServices();
                return;
            }

            var found = dbHelper.SearchServices(searchText);
            treeViewServices.Nodes.Clear();
            TreeNode searchNode = new TreeNode($"Результаты поиска: {searchText}");
            foreach (var serv in found)
            {
                TreeNode servNode = new TreeNode(serv.Name) { Tag = serv };
                searchNode.Nodes.Add(servNode);
            }
            treeViewServices.Nodes.Add(searchNode);
            searchNode.Expand();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddEditServiceForm addForm = new AddEditServiceForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadCategoriesAndServices();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedService == null)
            {
                MessageBox.Show("Выберите услугу для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AddEditServiceForm editForm = new AddEditServiceForm(selectedService);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                lastSelectedServiceId = selectedService.Id;
                LoadCategoriesAndServices();
                lastSelectedServiceId = null;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedService == null)
            {
                MessageBox.Show("Выберите услугу для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (dbHelper.HasAppointments(selectedService.Id))
                {
                    MessageBox.Show("Невозможно удалить услугу, так как на неё есть записи.\n" +
                                    "Сначала удалите все записи на эту услугу.", "Ошибка удаления",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке записей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Удалить услугу \"{selectedService.Name}\"?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    dbHelper.DeleteService(selectedService.Id);
                    LoadCategoriesAndServices();
                    selectedService = null;
                    panelCard.Controls.Clear();
                }
                catch (MySqlException ex) when (ex.Message.Contains("foreign key constraint fails"))
                {
                    MessageBox.Show("Невозможно удалить услугу, так как на неё есть записи.\n" +
                                    "Сначала удалите все записи на эту услугу.", "Ошибка удаления",
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