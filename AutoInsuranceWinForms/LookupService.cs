using System.Data;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public static class LookupService
    {
        public static void Fill(ComboBox combo, string sql, string valueMember, string displayMember)
        {
            var table = Db.Query(sql);
            combo.DataSource = table;
            combo.ValueMember = valueMember;
            combo.DisplayMember = displayMember;
        }

        public static DataTable Query(string sql)
        {
            return Db.Query(sql);
        }
    }
}
