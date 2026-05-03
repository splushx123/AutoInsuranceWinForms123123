using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class LoginForm : Form
    {
        private readonly TextBox _txtEmail = Theme.CreateTextBox(280);
        private readonly TextBox _txtPassword = Theme.CreateTextBox(280);
        private readonly CheckBox _chkRemember = new CheckBox();
        private readonly Button _btnTogglePassword = new Button();
        private readonly FlowLayoutPanel _passwordWrap = new FlowLayoutPanel();
        private readonly Label _lblError = new Label();
        private readonly string _rememberFile = Path.Combine(Application.StartupPath, "last_email.txt");

        public UserAccount CurrentUser { get; private set; }

        public LoginForm()
        {
            Theme.StyleForm(this);
            Text = "Авторизация";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(900, 500);

            var left = new Panel { Dock = DockStyle.Left, Width = 330, BackColor = Theme.Sidebar, Padding = new Padding(30) };
            left.Controls.Add(new Label
            {
                Text = "Auto Insurance\nWindows Forms",
                Dock = DockStyle.Top,
                Height = 110,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold)
            });

            var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };
            var card = Theme.CreateCard(28); card.Dock = DockStyle.Fill;
            var title = new Label { Text = "Вход в систему", Dock = DockStyle.Top, Height = 42, Font = new Font("Segoe UI", 18F, FontStyle.Bold) };
            var sub = new Label { Text = "Введите e-mail и пароль. Роль определяется по App.config.", Dock = DockStyle.Top, Height = 40, ForeColor = Theme.Muted };

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 220, ColumnCount = 2, Padding = new Padding(0, 12, 0, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "E-mail", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
            layout.Controls.Add(_txtEmail, 1, 0);
            layout.Controls.Add(new Label { Text = "Пароль", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 1);
            _txtPassword.UseSystemPasswordChar = true;
            _txtPassword.AutoSize = false;
            _txtPassword.BorderStyle = BorderStyle.None;
            _txtPassword.Dock = DockStyle.None;
            _txtPassword.Margin = new Padding(0);

            _passwordWrap.Width = _txtEmail.Width + 2;
            _passwordWrap.Height = _txtEmail.Height;
            _passwordWrap.AutoSize = false;
            _passwordWrap.Anchor = AnchorStyles.Left;
            _passwordWrap.Margin = new Padding(0);
            _passwordWrap.Padding = new Padding(0);
            _passwordWrap.FlowDirection = FlowDirection.LeftToRight;
            _passwordWrap.WrapContents = false;
            _passwordWrap.BackColor = Color.White;
            _passwordWrap.BorderStyle = BorderStyle.FixedSingle;
            _txtPassword.Location = new Point(0, 0);
            _txtPassword.Width = _passwordWrap.Width - 24;
            _txtPassword.Height = _txtEmail.Height - 2;
            _txtPassword.Margin = new Padding(8, 4, 0, 0);

            _btnTogglePassword.Dock = DockStyle.None;
            _btnTogglePassword.Width = 18;
            _btnTogglePassword.Height = _txtPassword.Height;
            _btnTogglePassword.FlatStyle = FlatStyle.Flat;
            _btnTogglePassword.FlatAppearance.BorderSize = 0;
            _btnTogglePassword.BackColor = Color.White;
            _btnTogglePassword.ForeColor = Theme.Muted;
            _btnTogglePassword.TextAlign = ContentAlignment.MiddleCenter;
            _btnTogglePassword.Cursor = Cursors.Hand;
            _btnTogglePassword.Margin = new Padding(0);
            _btnTogglePassword.Font = new Font("Segoe UI Symbol", 7.5F, FontStyle.Regular);
            _btnTogglePassword.Text = "👁";
            _btnTogglePassword.Click += delegate
            {
                _txtPassword.UseSystemPasswordChar = !_txtPassword.UseSystemPasswordChar;
                _btnTogglePassword.Text = "👁";
            };

            _btnTogglePassword.Margin = new Padding(0, 5, 2, 0);
            _passwordWrap.Controls.Add(_txtPassword);
            _passwordWrap.Controls.Add(_btnTogglePassword);
            layout.Controls.Add(_passwordWrap, 1, 1);
            _chkRemember.Text = "Запомнить e-mail"; _chkRemember.AutoSize = true;
            layout.Controls.Add(_chkRemember, 1, 2);
            var lnk = new LinkLabel { Text = "Проверить подключение к SQL Server", AutoSize = true, LinkColor = Theme.Primary };
            lnk.Click += delegate { TestConnection(); };
            layout.Controls.Add(lnk, 1, 3);
            _lblError.AutoSize = true; _lblError.ForeColor = Color.Firebrick;
            layout.Controls.Add(_lblError, 1, 4);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, FlowDirection = FlowDirection.RightToLeft };
            var btnLogin = Theme.CreatePrimaryButton("Войти", 120, true);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 120);
            btnLogin.Click += delegate { DoLogin(); };
            btnClose.Click += delegate { Close(); };
            buttons.Controls.Add(btnLogin); buttons.Controls.Add(btnClose);

            card.Controls.Add(buttons); card.Controls.Add(layout); card.Controls.Add(sub); card.Controls.Add(title);
            wrap.Controls.Add(card);
            Controls.Add(wrap); Controls.Add(left);

            if (File.Exists(_rememberFile))
            {
                _txtEmail.Text = File.ReadAllText(_rememberFile).Trim();
                _chkRemember.Checked = _txtEmail.Text.Length > 0;
            }
            else
            {
                _txtEmail.Text = "head@autoin.local";
            }

            AcceptButton = btnLogin;
            Shown += delegate { ApplyPasswordWidthFromEmail(); };
            Resize += delegate { ApplyPasswordWidthFromEmail(); };
        }

        private void ApplyPasswordWidthFromEmail()
        {
            _passwordWrap.Width = _txtEmail.Width + 2;
            _passwordWrap.Height = _txtEmail.Height;
            _txtPassword.Width = _passwordWrap.Width - _btnTogglePassword.Width - 12;
            _btnTogglePassword.Height = _txtPassword.Height;
        }

        private void TestConnection()
        {
            string error;
            MessageBox.Show(Db.CanConnect(out error)
                ? "Подключение к SQL Server выполнено успешно."
                : "Не удалось подключиться к базе данных.\n\n" + error,
                "Проверка подключения", MessageBoxButtons.OK,
                string.IsNullOrEmpty(error) ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void DoLogin()
        {
            if (string.IsNullOrWhiteSpace(_txtEmail.Text))
            {
                _lblError.Text = "Введите e-mail.";
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                _lblError.Text = "Введите пароль.";
                return;
            }

            CurrentUser = AuthService.Authenticate(_txtEmail.Text, _txtPassword.Text);
            if (CurrentUser == null)
            {
                _lblError.Text = "Неверный e-mail или пароль.";
                return;
            }

            string error;
            if (!Db.CanConnect(out error))
            {
                MessageBox.Show("Не удалось подключиться к SQL Server.\n\n" + error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_chkRemember.Checked) File.WriteAllText(_rememberFile, _txtEmail.Text.Trim());
            else if (File.Exists(_rememberFile)) File.Delete(_rememberFile);

            LogService.Log("Авторизация", CurrentUser.Email + " | " + CurrentUser.FullName);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
