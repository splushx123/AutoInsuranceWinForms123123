using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class EmployeesForm : FormBase
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly TextBox _txtFioSearch = Theme.CreateTextBox(280);

        public EmployeesForm(UserAccount user)
        {
            Theme.StyleForm(this); Text = "Сотрудники"; Width = 1100; Height = 660; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            var top = CreateTopPanel();
            top.WrapContents = false;
            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110); var btnEdit = Theme.CreateSecondaryButton("Изменить", 110); var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            var lblSearch = new Label { Text = "Поиск по ФИО:", AutoSize = true, Margin = new Padding(0, 11, 8, 0) };
            _txtFioSearch.Margin = new Padding(0, 8, 18, 0);
            _txtFioSearch.TextChanged += delegate { LoadData(); };
            btnAdd.Click += delegate { OpenEditor(null); }; btnEdit.Click += delegate { var id = SelectedId(_grid); if (id.HasValue) OpenEditor(id.Value); }; btnDelete.Click += delegate { DeleteSelected(); };
            top.Controls.Add(lblSearch); top.Controls.Add(_txtFioSearch);
            top.Controls.Add(btnAdd); top.Controls.Add(btnEdit); top.Controls.Add(btnDelete);
            Controls.Add(_grid); Controls.Add(top); Load += delegate { LoadData(); };
        }
        private void LoadData()
        {
            var searchText = _txtFioSearch.Text.Trim();
            if (searchText.Length == 0)
            {
                _grid.DataSource = Db.Query("SELECT employee_id AS [Код], last_name AS [Фамилия], first_name AS [Имя], middle_name AS [Отчество], position AS [Должность], phone AS [Телефон], email AS [Почта] FROM Employees ORDER BY last_name, first_name");
            }
            else
            {
                _grid.DataSource = Db.Query(@"SELECT employee_id AS [Код], last_name AS [Фамилия], first_name AS [Имя], middle_name AS [Отчество], position AS [Должность], phone AS [Телефон], email AS [Почта]
FROM Employees
WHERE (last_name + ' ' + first_name + ' ' + ISNULL(middle_name, '')) LIKE @fio
ORDER BY last_name, first_name", new SqlParameter("@fio", "%" + searchText + "%"));
            }
            if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
        }
        private void OpenEditor(int? id) { using (var f = new EmployeeEditForm(id)) if (f.ShowDialog(this) == DialogResult.OK) LoadData(); }
        private void DeleteSelected() { var id = SelectedId(_grid); if (!id.HasValue) return; if (MessageBox.Show("Удалить сотрудника?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes) return; try { Db.Execute("DELETE FROM Employees WHERE employee_id=@id", new SqlParameter("@id", id.Value)); LoadData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
    }
}
