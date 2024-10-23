using System;
using System.Windows.Forms;
using System.Data.SqlClient; // Подключение библиотеки для работы с SQL

namespace Склад
{
    public partial class Login : Form
    {
        
        private string connectionString = @"Data Source=ADCLG1;Initial Catalog=УЧ_Мирзоев;Integrated Security=True";

        public Login()
        {
            InitializeComponent();
        }

        // Событие для кнопки закрытия (X)
        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход из приложения", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Application.Exit(); // Закрывает всё приложение
            }
        }

        // Событие для кнопки Войти
        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string login = txtLogin.Text;
                string password = txtPassword.Text;
                
                


                if (ValidateCredentials(login, password, out string role)) // Роль получаем из метода
                {
                    MessageBox.Show("Успешный вход!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Открываем Form2 и передаем туда роль пользователя
                    Form2 mainForm = new Form2(role);
                    this.Hide(); // Прячем форму авторизации
                    mainForm.Show();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Измененный метод для проверки учетных данных с возвратом роли
        private bool ValidateCredentials(string login, string password, out string role)
        {
            role = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Измените названия таблицы и столбцов на русские
                string query = "SELECT Роль FROM Пользователи WHERE Логин = @login AND Пароль = @password";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", password);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        role = result.ToString(); // Получаем роль
                        return true;
                    }
                    return false;
                }
            }
        }



    }
}
