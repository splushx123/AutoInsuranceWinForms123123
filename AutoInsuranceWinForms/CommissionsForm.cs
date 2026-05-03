using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class CommissionsForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly DateTimePicker _dtpPaymentDate = Theme.CreateDatePicker(130);

        public CommissionsForm()
        {
            Theme.StyleForm(this); Text = "Комиссии"; Width = 900; Height = 560; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10), WrapContents = false, BackColor = Theme.Surface };
            var lblDateSearch = new Label { Text = "Поиск по дате выплаты:", AutoSize = true, Margin = new Padding(0, 11, 8, 0) };
            _dtpPaymentDate.ShowCheckBox = true;
            _dtpPaymentDate.Checked = false;
            _dtpPaymentDate.Margin = new Padding(0, 8, 0, 0);
            _dtpPaymentDate.ValueChanged += delegate { LoadData(); };
            top.Controls.Add(lblDateSearch);
            top.Controls.Add(_dtpPaymentDate);

            Controls.Add(_grid);
            Controls.Add(top);
            Load += delegate { LoadData(); };
        }

        private void LoadData()
        {
            if (_dtpPaymentDate.Checked)
            {
                _grid.DataSource = Db.Query("SELECT commission_id AS [Код комиссии], payment_date AS [Дата выплаты] FROM commissions WHERE CAST(payment_date AS date)=@date ORDER BY commission_id DESC", new SqlParameter("@date", _dtpPaymentDate.Value.Date));
            }
            else
            {
                _grid.DataSource = Db.Query("SELECT commission_id AS [Код комиссии], payment_date AS [Дата выплаты] FROM commissions ORDER BY commission_id DESC");
            }
        }
    }
}
