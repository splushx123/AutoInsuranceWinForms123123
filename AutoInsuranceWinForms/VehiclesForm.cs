using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class VehiclesForm : FormBase
    {
        private readonly UserAccount _user;
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly TextBox _txtSearch = Theme.CreateTextBox(240);

        public VehiclesForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Автомобили"; Width = 1200; Height = 680; StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);
            var top = CreateTopPanel();
            top.Controls.Add(new Label { Text = "Поиск VIN/госномер:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
            top.Controls.Add(_txtSearch);
            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110);
            var btnEdit = Theme.CreateSecondaryButton("Изменить", 110);
            var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            _txtSearch.TextChanged += delegate { LoadData(); };
            btnAdd.Click += delegate { OpenEditor(null); };
            btnEdit.Click += delegate { var vin = SelectedVin(); if (vin != null) OpenEditor(vin); };
            btnDelete.Click += delegate { DeleteSelected(); };
            top.Controls.Add(btnAdd); top.Controls.Add(btnEdit); top.Controls.Add(btnDelete);
            Controls.Add(_grid); Controls.Add(top); Load += delegate { LoadData(); };
        }

        private string SelectedVin() { return _grid.CurrentRow == null ? null : _grid.CurrentRow.Cells[0].Value.ToString(); }

        private void LoadData()
        {
            var search = _txtSearch.Text.Trim(); if (search.Length == 0) search = "%"; else search = "%" + search + "%";
            _grid.DataSource = Db.Query(@"
SELECT v.VIN AS [VIN], v.license_plate AS [Госномер], b.brand_name AS [Марка], m.model_name AS [Модель],
       vc.category_name AS [Категория], v.engine_power AS [Мощность],
       c.last_name + ' ' + c.first_name AS [Владелец]
FROM Vehicles v
LEFT JOIN car_brands b ON b.id_brand = v.id_brand
LEFT JOIN car_models m ON m.id_model = v.id_model
LEFT JOIN vehicle_categories vc ON vc.id_vehicle_category = v.id_vehicle_category
LEFT JOIN Client c ON c.id_client = v.id_client
WHERE v.VIN LIKE @search OR v.license_plate LIKE @search
ORDER BY v.VIN", new SqlParameter("@search", search));
        }

        private void OpenEditor(string vin)
        {
            using (var form = new VehicleEditForm(vin))
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        private void DeleteSelected()
        {
            if (_user.Role == UserRole.InsuranceAgent) { MessageBox.Show("У этой роли нет прав на удаление автомобилей."); return; }
            var vin = SelectedVin(); if (string.IsNullOrEmpty(vin)) return;
            if (MessageBox.Show("Удалить автомобиль?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Db.Execute("DELETE FROM Vehicles WHERE VIN=@vin", new SqlParameter("@vin", vin)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Не удалось удалить автомобиль.\n" + ex.Message); }
        }
    }
}
