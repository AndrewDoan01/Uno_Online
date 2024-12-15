using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameApp
{
    public partial class LoginForm : Form
    {
        public string CurrentUsername { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }
        

        private void Button2_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Text;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString))
            {
                conn.Open();
                string query = "SELECT PasswordHash FROM Users WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    string storedHash = (string)cmd.ExecuteScalar();

                    if (storedHash != null && PasswordHelper.VerifyPassword(password, storedHash))
                    {
                        MessageBox.Show("Đăng nhập thành công!");
                        CurrentUsername = username;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Tài khoản hoặc mật khẩu không hợp lệ.");
                    }
                }
            }
        }

        private void ForgotPasswordButton_Click_Click(object sender, EventArgs e)
        {
            ForgotPasswordForm forgotPasswordForm = new ForgotPasswordForm();
            forgotPasswordForm.ShowDialog();
        }
    }
}
