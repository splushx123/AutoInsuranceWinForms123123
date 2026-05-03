using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class PayoutEditForm : Form
    {
        private readonly int? _id;
        private readonly ComboBox _case = Theme.CreateComboBox(220);
        private readonly NumericUpDown _amount = Theme.CreateNumeric(220, 100000000);
        private readonly DateTimePicker _date = Theme.CreateDatePicker(220);
        public PayoutEditForm(int? id)
        {
            _id = id; Theme.StyleForm(this); Text = id.HasValue ? "Изменение выплаты" : "Добавление выплаты"; Width = 580; Height = 260; StartPosition = FormStartPosition.CenterParent;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddField(table, "Страховой случай", _case); AddField(table, "Сумма", _amount); AddField(table, "Дата выплаты", _date);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var save = Theme.CreatePrimaryButton("Сохранить", 120); save.Click += delegate { SaveData(); }; var cancel = Theme.CreateSecondaryButton("Отмена", 120); cancel.Click += delegate { Close(); };
            buttons.Controls.Add(save); buttons.Controls.Add(cancel); Controls.Add(table); Controls.Add(buttons);
            LookupService.Fill(_case, "SELECT case_id, CAST(case_id AS varchar(20)) AS title FROM Insurance_cases ORDER BY case_id DESC", "case_id", "title"); if (id.HasValue) LoadData();
        }
        private void AddField(TableLayoutPanel t, string n, Control c) { int r = t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); t.Controls.Add(new Label { Text = n, AutoSize = true, Padding = new Padding(0, 9, 0, 0) }, 0, r); t.Controls.Add(c, 1, r); }
        private void LoadData() { var dt = Db.Query("SELECT * FROM Insurance_payouts WHERE payout_id=@id", new SqlParameter("@id", _id.Value)); if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0]; _case.SelectedValue = Convert.ToInt32(r["case_id"]); _amount.Value = Convert.ToDecimal(r["payout_amount"]); _date.Value = Convert.ToDateTime(r["payout_date"]); }
        private void SaveData()
        {
            try
            {
                if (!ValidateFields()) return;

                if (_id.HasValue)
                    Db.Execute("UPDATE Insurance_payouts SET case_id=@case, payout_amount=@amount, payout_date=@date WHERE payout_id=@id", new SqlParameter("@case", _case.SelectedValue), new SqlParameter("@amount", _amount.Value), new SqlParameter("@date", _date.Value.Date), new SqlParameter("@id", _id.Value));
                else
                    Db.Execute("INSERT INTO Insurance_payouts(payout_id,case_id,payout_amount,payout_date) VALUES(@id,@case,@amount,@date)", new SqlParameter("@id", Db.NextId("Insurance_payouts", "payout_id")), new SqlParameter("@case", _case.SelectedValue), new SqlParameter("@amount", _amount.Value), new SqlParameter("@date", _date.Value.Date));

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения выплаты.\n" + ex.Message); }
        }

        private bool ValidateFields()
        {
            if (_case.SelectedValue == null)
            {
                MessageBox.Show("Выберите страховой случай.");
                return false;
            }
            if (_amount.Value <= 0)
            {
                MessageBox.Show("Сумма выплаты должна быть больше 0.");
                return false;
            }
            if (_date.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Дата выплаты не может быть в будущем.");
                return false;
            }
            return true;
        }
    }
}
