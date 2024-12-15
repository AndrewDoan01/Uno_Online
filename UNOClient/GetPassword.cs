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

namespace UnoOnline
{
    public partial class GetPassword : Form
    {
        public GetPassword()
        {
            InitializeComponent();
        }

        private void retrievePasswordButton_Click(object sender, EventArgs e)
        {
            // Lấy thông tin từ TextBox
            string username = usernameTextBox.Text.Trim();
            string email = emailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên tài khoản và email.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Chuỗi kết nối đến database
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\PC\Source\Repos\Uno_Online\UnoClient\Database1.mdf;Integrated Security=True;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Truy vấn kiểm tra email dựa trên tên tài khoản
                    string query = "SELECT Email FROM TaiKhoan WHERE TenTaiKhoan = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        string storedEmail = cmd.ExecuteScalar()?.ToString();

                        if (storedEmail != null && storedEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                        {
                            // Tạo mật khẩu mới
                            string newPassword = GenerateRandomPassword();

                            // Băm mật khẩu mới
                            string hashedPassword = PasswordHelper.HashPassword(newPassword);

                            // Cập nhật mật khẩu vào database
                            UpdatePasswordInDatabase(username, hashedPassword);

                            // Hiển thị mật khẩu mới
                            passwordTextBox.Text = newPassword;
                        }
                        else
                        {
                            MessageBox.Show("Tên tài khoản hoặc email không chính xác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy lại mật khẩu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Hàm tạo mật khẩu ngẫu nhiên
        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8); // Mật khẩu ngẫu nhiên 8 ký tự
        }

        // Hàm cập nhật mật khẩu vào database
        private void UpdatePasswordInDatabase(string username, string hashedPassword)
        {
            try
            {
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\PC\Source\Repos\Uno_Online\UnoClient\Database1.mdf;Integrated Security=True;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE TaiKhoan SET MatKhau = @PasswordHash WHERE TenTaiKhoan = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật mật khẩu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
