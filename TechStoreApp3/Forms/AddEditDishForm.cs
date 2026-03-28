using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using TechStoreApp3.Data;
using TechStoreApp3.Models;


namespace TechStoreApp3.Forms
{
    public partial class AddEditDishForm : Form
    {
        private DbHelper dbHelper;
        private Dish editingDish;
        private List<Category> categories;
        private string selectedPhotoPath;

        public AddEditDishForm(Dish dish = null)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingDish = dish;
            LoadCategories();

            if (editingDish != null)
            {
                Text = "Редактирование блюда";
                FillForm();
            }
            else
            {
                Text = "Добавление блюда";
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
            txtName.Text = editingDish.Name;
            txtDescription.Text = editingDish.Description;
            txtPrice.Text = editingDish.Price.ToString();
            txtWeight.Text = editingDish.Weight;
            comboCategory.SelectedValue = editingDish.CategoryId;

            if (!string.IsNullOrEmpty(editingDish.Photo) && File.Exists(editingDish.Photo))
            {
                try
                {
                    using (var fs = new FileStream(editingDish.Photo, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox.Image = Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                selectedPhotoPath = editingDish.Photo;
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
                MessageBox.Show("Введите название блюда", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Dish dish = new Dish
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Price = price,
                Weight = txtWeight.Text.Trim(),
                CategoryId = (int)comboCategory.SelectedValue,
                Photo = selectedPhotoPath
            };

            if (editingDish != null)
            {
                dish.Id = editingDish.Id;
                dbHelper.UpdateDish(dish);
            }
            else
            {
                dbHelper.AddDish(dish);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}