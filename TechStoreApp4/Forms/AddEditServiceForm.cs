using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using TechStoreApp4.Data;
using TechStoreApp4.Models;

namespace TechStoreApp4.Forms
{
    public partial class AddEditServiceForm : Form
    {
        private DbHelper dbHelper;
        private Service editingService;
        private string selectedPhotoPath;

        public AddEditServiceForm(Service service = null)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingService = service;
            LoadCategories();

            if (editingService != null)
            {
                Text = "Редактирование услуги";
                FillForm();
            }
            else
            {
                Text = "Добавление услуги";
            }
        }

        private void LoadCategories()
        {
            var categories = dbHelper.GetCategories();
            comboCategory.DataSource = categories;
            comboCategory.DisplayMember = "Name";
            comboCategory.ValueMember = "Id";
        }

        private void FillForm()
        {
            txtName.Text = editingService.Name;
            txtDescription.Text = editingService.Description;
            txtPrice.Text = editingService.Price.ToString();
            txtDuration.Text = editingService.Duration;
            comboCategory.SelectedValue = editingService.CategoryId;

            if (!string.IsNullOrEmpty(editingService.Photo) && File.Exists(editingService.Photo))
            {
                try
                {
                    using (var fs = new FileStream(editingService.Photo, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox.Image = Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                selectedPhotoPath = editingService.Photo;
            }
        }

        private void btnSelectPhoto_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string destPhotoPath = CopyImageToAppFolder(ofd.FileName);
                    if (destPhotoPath != null)
                    {
                        try
                        {
                            using (var fs = new FileStream(destPhotoPath, FileMode.Open, FileAccess.Read))
                            {
                                pictureBox.Image?.Dispose();
                                pictureBox.Image = Image.FromStream(fs);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        selectedPhotoPath = destPhotoPath;
                    }
                }
            }
        }

        private string CopyImageToAppFolder(string sourcePath)
        {
            string appDir = Application.StartupPath;
            string imagesDir = Path.Combine(appDir, "Images");
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);

            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(imagesDir, fileName);
            if (File.Exists(destPath))
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);
                int counter = 1;
                while (File.Exists(destPath))
                {
                    string newName = $"{nameWithoutExt}_{counter}{ext}";
                    destPath = Path.Combine(imagesDir, newName);
                    counter++;
                }
            }
            try
            {
                File.Copy(sourcePath, destPath, false);
                return destPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка копирования изображения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название услуги", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Service service = new Service
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Price = price,
                Duration = txtDuration.Text.Trim(),
                CategoryId = (int)comboCategory.SelectedValue,
                Photo = selectedPhotoPath
            };

            if (editingService != null)
            {
                service.Id = editingService.Id;
                dbHelper.UpdateService(service);
            }
            else
            {
                dbHelper.AddService(service);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}