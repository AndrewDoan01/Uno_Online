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
    public partial class ForgotPasswordForm : Form
    {
        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        private void retrievePasswordButton_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text; 
            string email = emailTextBox.Text;       

           
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString))
            {
                try
                {
                    conn.Open();

                    
                    string query = "SELECT Email FROM Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        string storedEmail = (string)cmd.ExecuteScalar(); // Lấy email từ cơ sở dữ liệu

                        if (storedEmail != null && storedEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                        {
                            // Tạo mật khẩu mới
                            string newPassword = GenerateRandomPassword();

                            // Băm mật khẩu mới
                            string hashedPassword = PasswordHelper.HashPassword(newPassword);

                            // Cập nhật mật khẩu mới vào cơ sở dữ liệu
                            UpdatePasswordInDatabase(username, hashedPassword);

                            // Hiển thị mật khẩu mới trên giao diện
                            passwordTextBox.Text = newPassword; // Gán mật khẩu mới vào TextBox
                        }
                        else
                        {
                            passwordTextBox.Text = "Thông tin không hợp lệ."; // Thông báo lỗi nếu không khớp email
                        }
                    }
                }
                catch (Exception ex)
                {
                    passwordTextBox.Text = $"Lỗi: {ex.Message}"; // Thông báo lỗi khi kết nối hoặc truy vấn
                }
            }
        }

        // Hàm tạo mật khẩu ngẫu nhiên
        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8); // Tạo mật khẩu ngẫu nhiên dài 8 ký tự
        }

        // Hàm cập nhật mật khẩu mới vào cơ sở dữ liệu
        private void UpdatePasswordInDatabase(string username, string hashedPassword)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
