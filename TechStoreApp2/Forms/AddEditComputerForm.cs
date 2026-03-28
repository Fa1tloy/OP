using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using TechStoreApp.Data;

using TechStoreApp2.Models;

namespace TechStoreApp2.Forms
{
    public partial class AddEditComputerForm : Form
    {
        private DbHelper dbHelper;
        private Computer editingComputer;
        private List<Category> categories;
        private string selectedPhotoPath;

        public AddEditComputerForm(Computer computer = null)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingComputer = computer;
            LoadCategories();

            if (editingComputer != null)
            {
                Text = "Редактирование компьютера";
                FillForm();
            }
            else
            {
                Text = "Добавление компьютера";
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
            txtName.Text = editingComputer.Name;
            txtSpecs.Text = editingComputer.Specs;
            txtPricePerHour.Text = editingComputer.PricePerHour.ToString();
            txtWeight.Text = editingComputer.Weight;
            comboCategory.SelectedValue = editingComputer.CategoryId;

            if (!string.IsNullOrEmpty(editingComputer.Photo) && File.Exists(editingComputer.Photo))
            {
                try
                {
                    using (var fs = new FileStream(editingComputer.Photo, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox.Image = Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                selectedPhotoPath = editingComputer.Photo;
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
                MessageBox.Show("Введите название компьютера", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPricePerHour.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену за час", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Computer computer = new Computer
            {
                Name = txtName.Text.Trim(),
                Specs = txtSpecs.Text.Trim(),
                PricePerHour = price,
                Weight = txtWeight.Text.Trim(),
                CategoryId = (int)comboCategory.SelectedValue,
                Photo = selectedPhotoPath
            };

            if (editingComputer != null)
            {
                computer.Id = editingComputer.Id;
                dbHelper.UpdateComputer(computer);
            }
            else
            {
                dbHelper.AddComputer(computer);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}