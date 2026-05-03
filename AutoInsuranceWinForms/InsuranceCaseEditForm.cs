using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class InsuranceCaseEditForm : Form
    {
        private readonly int? _id;
        private readonly ComboBox _contract = Theme.CreateComboBox(220);
        private readonly TextBox _description = Theme.CreateTextBox(220);
        private readonly NumericUpDown _damage = Theme.CreateNumeric(220, 100000000);
        private readonly NumericUpDown _guiltyCount = Theme.CreateNumeric(220, 1000000, 0);
        public InsuranceCaseEditForm(int? id)
        {
            _id = id; Theme.StyleForm(this); Text = id.HasValue ? "Изменение страхового случая" : "Добавление страхового случая"; Width = 600; Height = 320; StartPosition = FormStartPosition.CenterParent;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddField(table, "Договор", _contract); AddField(table, "Описание", _description); AddField(table, "Ущерб", _damage); AddField(table, "Кол-во виновных лиц", _guiltyCount);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var save = Theme.CreatePrimaryButton("Сохранить", 120); save.Click += delegate { SaveData(); }; var cancel = Theme.CreateSecondaryButton("Отмена", 120); cancel.Click += delegate { Close(); };
            buttons.Controls.Add(save); buttons.Controls.Add(cancel); Controls.Add(table); Controls.Add(buttons);
            LookupService.Fill(_contract, "SELECT id_contract, CAST(id_contract AS varchar(20)) AS title FROM Contract ORDER BY id_contract DESC", "id_contract", "title"); if (id.HasValue) LoadData();
        }
        private void AddField(TableLayoutPanel t, string n, Control c) { int r = t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); t.Controls.Add(new Label { Text = n, AutoSize = true, Padding = new Padding(0, 9, 0, 0) }, 0, r); t.Controls.Add(c, 1, r); }
        private void LoadData() { var dt = Db.Query("SELECT * FROM Insurance_cases WHERE case_id=@id", new SqlParameter("@id", _id.Value)); if (dt.Rows.Count == 0) return; var r = dt.Rows[0]; _contract.SelectedValue = Convert.ToInt32(r["id_contract"]); _description.Text = r["brief_description"].ToString(); _damage.Value = Convert.ToDecimal(r["final_damage"]); _guiltyCount.Value = Convert.ToDecimal(r["guilty_person"]); }
        private void SaveData()
        {
            try
            {
                if (_id.HasValue)
                    Db.Execute("UPDATE Insurance_cases SET id_contract=@contract, brief_description=@description, final_damage=@damage, guilty_person=@guilty WHERE case_id=@id", new SqlParameter("@contract", _contract.SelectedValue), new SqlParameter("@description", _description.Text.Trim()), new SqlParameter("@damage", _damage.Value), new SqlParameter("@guilty", Convert.ToInt32(_guiltyCount.Value)), new SqlParameter("@id", _id.Value));
                else
                    Db.Execute("INSERT INTO Insurance_cases(case_id,id_contract,brief_description,final_damage,guilty_person) VALUES(@id,@contract,@description,@damage,@guilty)", new SqlParameter("@id", Db.NextId("Insurance_cases", "case_id")), new SqlParameter("@contract", _contract.SelectedValue), new SqlParameter("@description", _description.Text.Trim()), new SqlParameter("@damage", _damage.Value), new SqlParameter("@guilty", Convert.ToInt32(_guiltyCount.Value)));

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения страхового случая.\n" + ex.Message); }
        }
    }
}
