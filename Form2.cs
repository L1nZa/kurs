using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Склад
{
    public partial class Form2 : Form
    {
        private string role;  // Роль пользователя
        private string[] availableTables;  // Доступные таблицы
        private string connectionString = @"Data Source=ADCLG1;Initial Catalog=УЧ_Мирзоев;Integrated Security=True";

        

        // Метод для установки цвета текста для роли пользователя
        private void SetUserLabelColor(string role)
        {
            switch (role)
            {
                case "Админ":
                    User.ForeColor = System.Drawing.Color.Red; // Красный для Админа
                    break;
                case "Менеджер":
                    User.ForeColor = System.Drawing.Color.Orange; // Оранжевый для Менеджера
                    break;
                case "Кладовщик":
                    User.ForeColor = System.Drawing.Color.Green; // Зеленый для Кладовщика
                    break;
                default:
                    User.ForeColor = System.Drawing.Color.Black; // Черный по умолчанию
                    break;
            }
        }

        // Метод для загрузки доступных таблиц на основе роли пользователя
        private void LoadAvailableTables()
        {
            switch (role)
            {
                case "Админ":
                    availableTables = new string[] { "Пользователи", "Товары", "Поставщики", "Поставки", "Клиенты", "Отгрузки", "Сборка" };
                    break;
                case "Менеджер":
                    availableTables = new string[] { "Товары", "Отгрузки", "Поставки" };
                    break;
                case "Кладовщик":
                    availableTables = new string[] { "Отгрузки", "Сборка" };
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    break;
            }

            // Устанавливаем текст на кнопки таблиц
            SetTableButtons();
        }

        // Метод для установки текста на кнопки таблиц
        private void SetTableButtons()
        {
            Guna.UI2.WinForms.Guna2Button[] tableButtons = { btntable1, btntable2, btntable3, btntable4, btntable5, btntable6, btntable7 };

            // Очищаем все кнопки
            foreach (var button in tableButtons)
            {
                button.Text = "";
                button.Visible = false;  // Скрываем кнопки, которые не нужны
                button.Click -= btnTable_Click;  // Отписка от события на случай повторного назначения
                button.Click += btnTable_Click;  // Подписка на обработчик события
            }

            // Устанавливаем текст на кнопки для доступных таблиц
            for (int i = 0; i < availableTables.Length; i++)
            {
                tableButtons[i].Text = availableTables[i];
                tableButtons[i].Visible = true;  // Показываем только нужные кнопки
            }
        }

        // Обработка нажатия на кнопку таблицы
        // Обработка нажатия на кнопку таблицы
        private void btnTable_Click(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2Button clickedButton = sender as Guna.UI2.WinForms.Guna2Button;

            if (clickedButton != null && !string.IsNullOrEmpty(clickedButton.Text))
            {
                // Переход на Form3 с передачей текущей таблицы, роли и ссылки на текущую форму
                Form3 form3 = new Form3(availableTables, Array.IndexOf(availableTables, clickedButton.Text), role, this);

                this.Hide(); // Скрываем текущую форму
                form3.Show(); // Открываем Form3
            }
        }


        // Обработка кнопки выхода (logout)
        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login loginForm = new Login();  // Возвращаемся на экран логина
            loginForm.Show();
        }

        // Закрытие приложения при нажатии кнопки X
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
