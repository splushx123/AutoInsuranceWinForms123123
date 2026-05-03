using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class EmployeeEditForm : Form
    {
        private readonly int? _id;
        private readonly TextBox _lastName = Theme.CreateTextBox(220);
        private readonly TextBox _firstName = Theme.CreateTextBox(220);
        private readonly TextBox _middleName = Theme.CreateTextBox(220);
        private readonly ComboBox _position = Theme.CreateComboBox(220);
        private readonly TextBox _phone = Theme.CreateTextBox(220);
        private readonly TextBox _email = Theme.CreateTextBox(220);

        public EmployeeEditForm(int? id)
        {
            _id = id; Theme.StyleForm(this); Text = id.HasValue ? "Изменение сотрудника" : "Добавление сотрудника"; Width = 620; Height = 360; StartPosition = FormStartPosition.CenterParent;
            _position.Items.AddRange(new object[] { "Руководитель отдела", "Менеджер", "Старший агент", "Страховой агент" });
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddField(table, "Фамилия", _lastName); AddField(table, "Имя", _firstName); AddField(table, "Отчество", _middleName); AddField(table, "Должность", _position); AddField(table, "Телефон", _phone); AddField(table, "E-mail", _email);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var save = Theme.CreatePrimaryButton("Сохранить", 120); save.Click += delegate { SaveData(); }; var cancel = Theme.CreateSecondaryButton("Отмена", 120); cancel.Click += delegate { Close(); };
            buttons.Controls.Add(save); buttons.Controls.Add(cancel); Controls.Add(table); Controls.Add(buttons);
            ConfigureInputRules();
            if (id.HasValue) LoadData();
        }

        private void ConfigureInputRules()
        {
            _phone.MaxLength = 18;
        }

        private void AddField(TableLayoutPanel t, string n, Control c) { int r = t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); t.Controls.Add(new Label { Text = n, AutoSize = true, Padding = new Padding(0, 9, 0, 0) }, 0, r); t.Controls.Add(c, 1, r); }
        private void LoadData() { var dt = Db.Query("SELECT * FROM Employees WHERE employee_id=@id", new SqlParameter("@id", _id.Value)); if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0]; _lastName.Text = r["last_name"].ToString(); _firstName.Text = r["first_name"].ToString(); _middleName.Text = r["middle_name"].ToString(); _position.Text = r["position"].ToString(); _phone.Text = r["phone"].ToString(); _email.Text = r["email"].ToString(); }

        private void SaveData()
        {
            try
            {
                if (!ValidateFields()) return;
                string normalizedPhone;
                if (!ValidationRules.TryNormalizePhoneRu(_phone.Text, out normalizedPhone))
                {
                    MessageBox.Show("Телефон сотрудника должен быть в формате +7XXXXXXXXXX (10 цифр после +7).");
                    return;
                }

                if (_id.HasValue)
                    Db.Execute("UPDATE Employees SET last_name=@last_name, first_name=@first_name, middle_name=@middle_name, position=@position, phone=@phone, email=@email WHERE employee_id=@id", new SqlParameter("@last_name", _lastName.Text.Trim()), new SqlParameter("@first_name", _firstName.Text.Trim()), new SqlParameter("@middle_name", _middleName.Text.Trim()), new SqlParameter("@position", _position.Text.Trim()), new SqlParameter("@phone", normalizedPhone), new SqlParameter("@email", _email.Text.Trim()), new SqlParameter("@id", _id.Value));
                else
                    Db.Execute("INSERT INTO Employees(employee_id,last_name,first_name,middle_name,position,phone,email) VALUES(@id,@last_name,@first_name,@middle_name,@position,@phone,@email)", new SqlParameter("@id", Db.NextId("Employees", "employee_id")), new SqlParameter("@last_name", _lastName.Text.Trim()), new SqlParameter("@first_name", _firstName.Text.Trim()), new SqlParameter("@middle_name", _middleName.Text.Trim()), new SqlParameter("@position", _position.Text.Trim()), new SqlParameter("@phone", normalizedPhone), new SqlParameter("@email", _email.Text.Trim()));

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения сотрудника.\n" + ex.Message); }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(_lastName.Text) || string.IsNullOrWhiteSpace(_firstName.Text))
            {
                MessageBox.Show("Введите фамилию и имя сотрудника.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_position.Text))
            {
                MessageBox.Show("Выберите должность.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_phone.Text))
            {
                MessageBox.Show("Введите телефон сотрудника.");
                return false;
            }
            if (!ValidationRules.TryNormalizePhoneRu(_phone.Text, out _))
            {
                MessageBox.Show("Телефон сотрудника должен быть в формате +7XXXXXXXXXX (10 цифр после +7).");
                return false;
            }

            var email = _email.Text.Trim();
            if (email.Length == 0)
            {
                MessageBox.Show("Введите e-mail сотрудника.");
                return false;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный e-mail сотрудника.");
                return false;
            }

            return true;
        }
    }
}
