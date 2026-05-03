using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class PayoutsForm : FormBase
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly DateTimePicker _dtpPayoutDate = Theme.CreateDatePicker(130);

        public PayoutsForm(UserAccount user)
        {
            Theme.StyleForm(this); Text = "Выплаты"; Width = 1100; Height = 660; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            var top = CreateTopPanel();
            top.WrapContents = false;
            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110); var btnEdit = Theme.CreateSecondaryButton("Изменить", 110); var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            var lblDateSearch = new Label { Text = "Поиск по дате выплаты:", AutoSize = true, Margin = new Padding(0, 11, 8, 0) };
            _dtpPayoutDate.ShowCheckBox = true;
            _dtpPayoutDate.Checked = false;
            _dtpPayoutDate.Margin = new Padding(0, 8, 18, 0);
            _dtpPayoutDate.ValueChanged += delegate { LoadData(); };
            btnAdd.Click += delegate { OpenEditor(null); }; btnEdit.Click += delegate { var id = SelectedId(_grid); if (id.HasValue) OpenEditor(id.Value); }; btnDelete.Click += delegate { DeleteSelected(); };
            top.Controls.Add(lblDateSearch); top.Controls.Add(_dtpPayoutDate);
            top.Controls.Add(btnAdd); top.Controls.Add(btnEdit); top.Controls.Add(btnDelete);
            Controls.Add(_grid); Controls.Add(top); Load += delegate { LoadData(); };
        }
        private void LoadData()
        {
            if (_dtpPayoutDate.Checked)
            {
                _grid.DataSource = Db.Query("SELECT payout_id AS [Код], case_id AS [Страховой случай], payout_amount AS [Сумма], payout_date AS [Дата выплаты] FROM Insurance_payouts WHERE CAST(payout_date AS date)=@date ORDER BY payout_id DESC", new SqlParameter("@date", _dtpPayoutDate.Value.Date));
            }
            else
            {
                _grid.DataSource = Db.Query("SELECT payout_id AS [Код], case_id AS [Страховой случай], payout_amount AS [Сумма], payout_date AS [Дата выплаты] FROM Insurance_payouts ORDER BY payout_id DESC");
            }
            if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
        }
        private void OpenEditor(int? id) { using (var f = new PayoutEditForm(id)) if (f.ShowDialog(this) == DialogResult.OK) LoadData(); }
        private void DeleteSelected() { var id = SelectedId(_grid); if (!id.HasValue) return; if (MessageBox.Show("Удалить выплату?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes) return; try { Db.Execute("DELETE FROM Insurance_payouts WHERE payout_id=@id", new SqlParameter("@id", id.Value)); LoadData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
    }
}
