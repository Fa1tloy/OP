using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TechStoreApp.Data;
using TechStoreApp.Models;

namespace TechStoreApp.Forms
{
    public partial class AddEditProductForm : Form
    {
        private DbHelper dbHelper;
        private Product editingProduct;
        private List<Category> categories;
        private string selectedPhotoPath; // путь к файлу изображения в папке Images

        public AddEditProductForm(Product product = null)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingProduct = product;
            LoadCategories();

            if (editingProduct != null)
            {
                Text = "Редактирование товара";
                FillForm();
            }
            else
            {
                Text = "Добавление товара";
            }
        }

        private void LoadCategories()
        {
            categories = dbHelper.GetCategories();
            comboCategory.DataSource = categories;
            comboCategory.DisplayMember = "Name";
            comboCategory.ValueMember = "Id";
        }

        private void FillForm()
        {
            txtName.Text = editingProduct.Name;
            txtBrand.Text = editingProduct.Brand;
            txtPrice.Text = editingProduct.Price.ToString();
            txtPower.Text = editingProduct.Power;
            comboCategory.SelectedValue = editingProduct.CategoryId;

            // Загружаем фото через поток, чтобы не блокировать файл
            if (!string.IsNullOrEmpty(editingProduct.Photo) && File.Exists(editingProduct.Photo))
            {
                try
                {
                    using (var fs = new FileStream(editingProduct.Photo, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox.Image = Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                selectedPhotoPath = editingProduct.Photo;
            }
        }

        private void btnSelectPhoto_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Копируем выбранный файл в папку Images приложения
                    string destPhotoPath = CopyImageToAppFolder(ofd.FileName);
                    if (destPhotoPath != null)
                    {
                        // Загружаем из нового файла через поток
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
            // Если файл уже существует, создаём уникальное имя
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
                MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Если фото было выбрано, selectedPhotoPath уже указывает на файл в папке Images
            // Если фото не выбрано (пусто), оставляем пустым
            // При редактировании, если фото не менялось, selectedPhotoPath уже содержит путь к существующему файлу в Images

            Product product = new Product
            {
                Name = txtName.Text.Trim(),
                Brand = txtBrand.Text.Trim(),
                Price = price,
                Power = txtPower.Text.Trim(),
                CategoryId = (int)comboCategory.SelectedValue,
                Photo = selectedPhotoPath // уже полный путь к файлу в папке Images
            };

            if (editingProduct != null)
            {
                product.Id = editingProduct.Id;
                dbHelper.UpdateProduct(product);
            }
            else
            {
                dbHelper.AddProduct(product);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}