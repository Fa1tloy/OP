using System;
using System.Windows.Forms;
using TechStoreApp3.Data;
using TechStoreApp3.Models;


namespace TechStoreApp3.Forms
{
    public partial class LoginForm : Form
    {
        private DbHelper dbHelper;

        public LoginForm()
        {
            InitializeComponent();
            dbHelper = new DbHelper();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            User user = dbHelper.GetUser(login, password);
            if (user != null)
            {
                MainForm mainForm = new MainForm(user);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuest_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm(null); // null – гость
            mainForm.Show();
            this.Hide();
        }
    }
}