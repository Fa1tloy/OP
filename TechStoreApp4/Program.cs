using System;
using System.Windows.Forms;
using TechStoreApp4.Forms;

namespace TechStoreApp4
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}