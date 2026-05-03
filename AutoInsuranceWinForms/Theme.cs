using System.Drawing;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public static class Theme
    {
        public static readonly Color AppBack = Color.FromArgb(243, 246, 251);
        public static readonly Color Surface = Color.White;
        public static readonly Color Primary = Color.FromArgb(18, 92, 168);
        public static readonly Color PrimaryDark = Color.FromArgb(14, 68, 124);
        public static readonly Color Sidebar = Color.FromArgb(24, 32, 52);
        public static readonly Color Success = Color.FromArgb(0, 153, 102);
        public static readonly Color Warning = Color.FromArgb(230, 145, 56);
        public static readonly Color Muted = Color.FromArgb(107, 114, 128);
        public static readonly Color Text = Color.FromArgb(31, 41, 55);
        public static readonly Color Border = Color.FromArgb(220, 225, 232);

        public static void StyleForm(Form form)
        {
            form.BackColor = AppBack;
            form.Font = new Font("Segoe UI", 10F);
            form.ForeColor = Text;
        }

        public static Panel CreateCard(int padding = 16)
        {
            return new Panel
            {
                BackColor = Surface,
                Padding = new Padding(padding),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static TextBox CreateTextBox(int width)
        {
            return new TextBox { Width = width, BorderStyle = BorderStyle.FixedSingle };
        }

        public static ComboBox CreateComboBox(int width)
        {
            return new ComboBox { Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
        }

        public static DateTimePicker CreateDatePicker(int width)
        {
            return new DateTimePicker { Width = width, Format = DateTimePickerFormat.Short };
        }

        public static NumericUpDown CreateNumeric(int width, decimal max, int decimals = 2)
        {
            return new NumericUpDown { Width = width, Maximum = max, DecimalPlaces = decimals, ThousandsSeparator = true };
        }

        public static Button CreatePrimaryButton(string text, int width, bool blueByDefault = false)
        {
            var b = new Button();
            b.Text = text; b.Width = width; b.Height = 38; b.FlatStyle = FlatStyle.Flat;
            b.Cursor = Cursors.Hand;
            if (blueByDefault)
            {
                b.FlatAppearance.BorderSize = 0;
                b.BackColor = Primary;
                b.ForeColor = Color.White;
                ApplyPrimaryHover(b);
            }
            else
            {
                b.FlatAppearance.BorderColor = Border; b.FlatAppearance.BorderSize = 1;
                b.BackColor = Surface; b.ForeColor = Text;
                ApplyHoverBlue(b);
            }
            return b;
        }

        public static Button CreateSecondaryButton(string text, int width)
        {
            var b = new Button();
            b.Text = text; b.Width = width; b.Height = 38; b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Border; b.FlatAppearance.BorderSize = 1;
            b.BackColor = Surface; b.ForeColor = Text; b.Cursor = Cursors.Hand;
            ApplyHoverBlue(b);
            return b;
        }

        private static void ApplyHoverBlue(Button button)
        {
            button.MouseEnter += delegate
            {
                button.BackColor = Primary;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderColor = Primary;
            };
            button.MouseLeave += delegate
            {
                button.BackColor = Surface;
                button.ForeColor = Text;
                button.FlatAppearance.BorderColor = Border;
            };
        }

        private static void ApplyPrimaryHover(Button button)
        {
            button.MouseEnter += delegate { button.BackColor = PrimaryDark; };
            button.MouseLeave += delegate { button.BackColor = Primary; };
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Sidebar;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }
    }
}
