using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class ClientsForm : FormBase
    {
        private readonly UserAccount _user;
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly TextBox _txtSearch = Theme.CreateTextBox(240);

        public ClientsForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Клиенты";
            Width = 1150; Height = 680; StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);
            var top = CreateTopPanel();
            top.Controls.Add(new Label { Text = "Поиск:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
            top.Controls.Add(_txtSearch);
            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110);
            var btnEdit = Theme.CreateSecondaryButton("Изменить", 110);
            var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            _txtSearch.TextChanged += delegate { LoadData(); };
            btnAdd.Click += delegate { OpenEditor(null); };
            btnEdit.Click += delegate { var id = SelectedId(_grid); if (id.HasValue) OpenEditor(id.Value); };
            btnDelete.Click += delegate { DeleteSelected(); };
            top.Controls.Add(btnAdd); top.Controls.Add(btnEdit); top.Controls.Add(btnDelete);
            Controls.Add(_grid); Controls.Add(top);
            Load += delegate { LoadData(); };
        }

        private void LoadData()
        {
            var text = _txtSearch.Text.Trim();
            var search = text.Length == 0 ? "%" : "%" + text + "%";
            _grid.DataSource = Db.Query(@"
SELECT id_client AS [Код], last_name AS [Фамилия], first_name AS [Имя], middle_name AS [Отчество],
       phone AS [Телефон], email AS [Почта], inn AS [ИНН], passport_series AS [Серия], passport_number AS [Номер]
FROM Client
WHERE last_name LIKE @search OR first_name LIKE @search OR phone LIKE @search OR ISNULL(email,'') LIKE @search
ORDER BY last_name, first_name", new SqlParameter("@search", search));
            if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
        }

        private void OpenEditor(int? id)
        {
            using (var form = new ClientEditForm(id))
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        private void DeleteSelected()
        {
            if (_user.Role == UserRole.InsuranceAgent) { MessageBox.Show("У этой роли нет прав на удаление клиентов."); return; }
            var id = SelectedId(_grid); if (!id.HasValue) return;
            if (MessageBox.Show("Удалить клиента?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Db.Execute("DELETE FROM Client WHERE id_client=@id", new SqlParameter("@id", id.Value)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Не удалось удалить клиента.\n" + ex.Message); }
        }
    }
}
