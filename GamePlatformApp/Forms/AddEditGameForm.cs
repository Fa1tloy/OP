using GamePlatformApp.Data;
using GamePlatformApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GamePlatformApp.Forms
{
    public partial class AddEditGameForm : Form
    {
        private DbHelper dbHelper;
        private Game editingGame;
        private List<Category> categories;
        private string selectedPhotoPath;

        public AddEditGameForm(Game game = null)
        {
            InitializeComponent();
            dbHelper = new DbHelper();
            editingGame = game;
            LoadCategories();

            if (editingGame != null)
            {
                Text = "Редактирование игры";
                FillForm();
            }
            else
            {
                Text = "Добавление игры";
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
            txtName.Text = editingGame.Name;
            txtDeveloper.Text = editingGame.Developer;
            if (editingGame.ReleaseYear.HasValue)
                numReleaseYear.Value = editingGame.ReleaseYear.Value;
            txtPrice.Text = editingGame.Price.ToString();
            txtPower.Text = editingGame.Power;
            comboCategory.SelectedValue = editingGame.CategoryId;

            if (!string.IsNullOrEmpty(editingGame.Photo) && File.Exists(editingGame.Photo))
            {
                try
                {
                    using (var fs = new FileStream(editingGame.Photo, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox.Image = Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                selectedPhotoPath = editingGame.Photo;
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
                MessageBox.Show("Введите название игры", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? releaseYear = null;
            if (numReleaseYear.Value > 0)
                releaseYear = (int)numReleaseYear.Value;

            Game game = new Game
            {
                Name = txtName.Text.Trim(),
                Developer = txtDeveloper.Text.Trim(),
                ReleaseYear = releaseYear,
                Price = price,
                Power = txtPower.Text.Trim(),
                CategoryId = (int)comboCategory.SelectedValue,
                Photo = selectedPhotoPath
            };

            if (editingGame != null)
            {
                game.Id = editingGame.Id;
                dbHelper.UpdateGame(game);
            }
            else
            {
                dbHelper.AddGame(game);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}