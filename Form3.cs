using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Склад
{
    public partial class Form3 : Form
    {
        private string tableName;  // Название текущей таблицы
        private string role;       // Роль пользователя
        private string[] tables;   // Список всех доступных таблиц
        private int currentIndex;  // Индекс текущей таблицы
        private string connectionString = @"Data Source=ADCLG1;Initial Catalog=УЧ_Мирзоев;Integrated Security=True";
        private Form2 parentForm; // Ссылка на родительскую форму

        public Form3(string[] availableTables, int tableIndex, string userRole, Form2 parentForm)
        {
            InitializeComponent();

            tables = availableTables; // Список всех таблиц
            currentIndex = tableIndex; // Индекс текущей таблицы
            role = userRole; // Роль пользователя
            this.parentForm = parentForm;
            // Устанавливаем начальные значения
            tableName = tables[currentIndex];
            table.Text = $"{tableName}"; // Отображаем название таблицы
            User.Text = $"{role}"; // Отображаем роль пользователя
            SetUserLabelColor(role); // Устанавливаем цвет текста

            LoadTableData(); // Загружаем данные из таблицы

            // Разрешаем редактирование в DataGridView
            DataGridViewTable.ReadOnly = false;
            DataGridViewTable.AllowUserToAddRows = true; // Позволяем добавлять строки
            DataGridViewTable.CellEndEdit += DataGridViewTable_CellEndEdit; // Событие редактирования ячейки
        }



        private void SetUserLabelColor(string role)
        {
            switch (role)
            {
                case "Админ":
                    User.ForeColor = Color.Red; // Красный для Админа
                    break;
                case "Менеджер":
                    User.ForeColor = Color.Orange; // Оранжевый для Менеджера
                    break;
                case "Кладовщик":
                    User.ForeColor = Color.Green; // Зеленый для Кладовщика
                    break;
                default:
                    User.ForeColor = Color.Black; // Черный по умолчанию
                    break;
            }
        }

        private void DataGridViewTable_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что ячейка относится к колонке "Роль"
            if (DataGridViewTable.Columns[e.ColumnIndex].Name == "Роль")
            {
                // Получаем значение роли из ячейки
                var roleValue = e.Value?.ToString();

                // Меняем цвет текста в зависимости от роли
                if (roleValue == "Админ")
                {
                    e.CellStyle.ForeColor = Color.Red; // Красный для Админа
                }
                else if (roleValue == "Менеджер")
                {
                    e.CellStyle.ForeColor = Color.Orange; // Оранжевый для Менеджера
                }
                else if (roleValue == "Кладовщик")
                {
                    e.CellStyle.ForeColor = Color.Green; // Зеленый для Кладовщика
                }
            }
        }

        // Метод для загрузки данных из выбранной таблицы
        private void LoadTableData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName}"; // Загружаем данные из текущей таблицы

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Отображаем данные в DataGridView
                        DataGridViewTable.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных из таблицы {tableName}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Событие редактирования ячейки
        private void DataGridViewTable_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var currentRow = DataGridViewTable.Rows[e.RowIndex];
                var idValue = currentRow.Cells["ID"].Value;

                // Проверяем, заполнены ли обязательные поля
                var login = currentRow.Cells["Логин"].Value?.ToString();
                var password = currentRow.Cells["Пароль"].Value?.ToString();
                var role = currentRow.Cells["Роль"].Value?.ToString();
                var creationDate = currentRow.Cells["ДатаСоздания"].Value?.ToString();
                var status = currentRow.Cells["Статус"].Value?.ToString();

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
                {
                    // Прерываем выполнение, если обязательные поля не заполнены
                    MessageBox.Show("Заполните все обязательные поля: Логин, Пароль, Роль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (idValue == null || idValue == DBNull.Value)
                    {
                        // Выполняем INSERT для новой записи
                        string insertQuery = $"INSERT INTO {tableName} (Логин, Пароль, Роль, ДатаСоздания, Статус) " +
                                             "VALUES (@login, @password, @role, @creationDate, @status); SELECT SCOPE_IDENTITY();";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@role", role);
                            cmd.Parameters.AddWithValue("@creationDate", creationDate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@status", status ?? (object)DBNull.Value);

                            // Получаем сгенерированный ID и вставляем его в ячейку
                            var newId = cmd.ExecuteScalar();
                            currentRow.Cells["ID"].Value = newId;
                        }
                    }
                    else
                    {
                        // Выполняем UPDATE для редактирования
                        string updateQuery = $"UPDATE {tableName} SET Логин = @login, Пароль = @password, Роль = @role, ДатаСоздания = @creationDate, Статус = @status WHERE ID = @id";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@role", role);
                            cmd.Parameters.AddWithValue("@creationDate", creationDate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@status", status ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@id", idValue);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Данные успешно обновлены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void SaveDataForTable(DataGridViewRow row, string tableName, string[] mandatoryFields, SqlConnection connection)
        {
            try
            {
                var idValue = row.Cells["ID"].Value;

                // Проверяем заполненность обязательных полей
                foreach (var field in mandatoryFields)
                {
                    var cellValue = row.Cells[field].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(cellValue))
                    {
                        MessageBox.Show($"Заполните обязательное поле: {field}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (idValue == null || idValue == DBNull.Value)
                {
                    // Формируем запрос на INSERT
                    string insertQuery = $"INSERT INTO {tableName} ({string.Join(", ", mandatoryFields)}, ДатаСоздания) " +
                                         $"VALUES ({string.Join(", ", mandatoryFields.Select(f => $"@{f}"))}, @ДатаСоздания); SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        foreach (var field in mandatoryFields)
                        {
                            cmd.Parameters.AddWithValue($"@{field}", row.Cells[field].Value?.ToString() ?? (object)DBNull.Value);
                        }

                        // Добавляем текущую дату
                        cmd.Parameters.AddWithValue("@ДатаСоздания", DateTime.Now);

                        // Получаем сгенерированный ID и вставляем его в ячейку
                        var newId = cmd.ExecuteScalar();
                        row.Cells["ID"].Value = newId;
                    }
                }
                else
                {
                    // Формируем запрос на UPDATE
                    string updateQuery = $"UPDATE {tableName} SET {string.Join(", ", mandatoryFields.Select(f => $"{f} = @{f}"))}, ДатаСоздания = @ДатаСоздания WHERE ID = @id";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        foreach (var field in mandatoryFields)
                        {
                            cmd.Parameters.AddWithValue($"@{field}", row.Cells[field].Value?.ToString() ?? (object)DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@ДатаСоздания", row.Cells["ДатаСоздания"].Value); // Обновляем только если есть значение
                        cmd.Parameters.AddWithValue("@id", idValue);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Данные успешно обновлены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Обработка нажатия кнопки Next
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentIndex < tables.Length - 1)
            {
                currentIndex++;  // Переходим к следующей таблице
                tableName = tables[currentIndex];
                table.Text = $"{tableName}";
                LoadTableData();  // Перезагружаем данные
            }
            else
            {
                MessageBox.Show("Это последняя таблица.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Обработка нажатия кнопки Back
        private void btnBack_Click(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;  // Переходим к предыдущей таблице
                tableName = tables[currentIndex];
                table.Text = $"{tableName}";
                LoadTableData();  // Перезагружаем данные
            }
            else
            {
                MessageBox.Show("Это первая таблица.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Обработка нажатия кнопки Закрыть (X)
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();  // Закрываем приложение
        }

        // Кнопка назад к списку таблиц на Form2
        private void btnBackToTables_Click(object sender, EventArgs e)
        {
            this.Hide(); // Скрываем текущую форму
            parentForm.Show(); // Показываем родительскую форму
        }

    }
}
