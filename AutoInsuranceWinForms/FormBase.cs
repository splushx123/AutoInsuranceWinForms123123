using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class FormBase : Form
    {
        protected FlowLayoutPanel CreateTopPanel()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10),
                WrapContents = true,
                BackColor = Theme.Surface
            };
        }

        protected int? SelectedId(DataGridView grid)
        {
            if (grid.CurrentRow == null) return null;
            return System.Convert.ToInt32(grid.CurrentRow.Cells[0].Value);
        }
    }
}
