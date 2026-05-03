using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class MainForm : Form
    {
        private readonly UserAccount _user;
        private readonly FlowLayoutPanel _statsPanel = new FlowLayoutPanel();
        private readonly FlowLayoutPanel _tilesPanel = new FlowLayoutPanel();
        public bool ReturnToLogin { get; private set; }

        public MainForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Автострахование - главное окно";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;

            var sidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.Sidebar, Padding = new Padding(18) };
            sidebar.Controls.Add(new Label
            {
                Text = "Автострахование",
                Dock = DockStyle.Top,
                Height = 54,
                AutoSize = false,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            });
            var btnLogout = Theme.CreatePrimaryButton("Выход", 210, true);
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Click += delegate { ReturnToLogin = true; Close(); };
            sidebar.Controls.Add(btnLogout);

            var top = new Panel { Dock = DockStyle.Top, Height = 96, Padding = new Padding(24, 16, 24, 14), BackColor = Theme.Surface };
            var lblSubtitle = new Label
            {
                Text = BuildAccessText(_user.Role),
                Dock = DockStyle.Top,
                Height = 42,
                AutoSize = false,
                ForeColor = Theme.Muted
            };
            var lblTitle = new Label
            {
                Text = BuildRoleTitle(_user.Role),
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Theme.Text
            };
            top.Controls.Add(lblSubtitle);
            top.Controls.Add(lblTitle);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22) };
            _statsPanel.Dock = DockStyle.Top;
            _statsPanel.Height = 150;
            _statsPanel.WrapContents = true;
            _statsPanel.AutoSize = false;
            _statsPanel.BackColor = Color.Transparent;
            _statsPanel.Padding = new Padding(0, 4, 0, 8);
            _tilesPanel.Dock = DockStyle.Fill; _tilesPanel.WrapContents = true; _tilesPanel.AutoScroll = true;
            body.Controls.Add(_tilesPanel); body.Controls.Add(_statsPanel);

            Controls.Add(body); Controls.Add(top); Controls.Add(sidebar);
            Load += delegate { FillStats(); FillTiles(); };
        }

        private string BuildRoleTitle(UserRole role)
        {
            if (role == UserRole.DepartmentHead) return "Главный модуль руководителя отдела";
            if (role == UserRole.Manager) return "Главный модуль менеджера";
            if (role == UserRole.SeniorAgent) return "Главный модуль старшего агента";
            return "Главный модуль страхового агента";
        }

        private string BuildAccessText(UserRole role)
        {
            if (role == UserRole.DepartmentHead)
                return "Централизованный доступ ко всем разделам: клиенты, автомобили, договоры, страховые случаи, выплаты, сотрудники, комиссии и отчеты.";
            if (role == UserRole.Manager)
                return "Доступ к разделам: клиенты, автомобили, договоры, страховые случаи, сотрудники и отчеты. Разделы выплаты и комиссии скрыты.";
            if (role == UserRole.SeniorAgent)
                return "Доступ к разделам: клиенты, автомобили, договоры, сотрудники и отчеты.";
            return "Доступ к разделам: страховые случаи, выплаты, комиссии, сотрудники и отчеты.";
        }

        private void FillStats()
        {
            _statsPanel.Controls.Clear();
            if (_user.Role == UserRole.DepartmentHead || _user.Role == UserRole.Manager || _user.Role == UserRole.SeniorAgent)
                AddStatCard("Клиенты", SafeCount("SELECT COUNT(*) FROM Client").ToString(), Theme.Primary);
            if (_user.Role == UserRole.DepartmentHead || _user.Role == UserRole.Manager || _user.Role == UserRole.SeniorAgent)
                AddStatCard("Автомобили", SafeCount("SELECT COUNT(*) FROM Vehicles").ToString(), Theme.Success);
            if (_user.Role == UserRole.DepartmentHead || _user.Role == UserRole.Manager || _user.Role == UserRole.SeniorAgent)
                AddStatCard("Договоры", SafeCount("SELECT COUNT(*) FROM Contract").ToString(), Theme.Warning);
            if (_user.Role == UserRole.DepartmentHead || _user.Role == UserRole.Manager || _user.Role == UserRole.InsuranceAgent)
                AddStatCard("Страховые случаи", SafeCount("SELECT COUNT(*) FROM Insurance_cases").ToString(), Color.FromArgb(56, 96, 178));
            if (_user.Role == UserRole.DepartmentHead || _user.Role == UserRole.InsuranceAgent)
                AddStatCard("Выплаты", SafeRoundedMoney("SELECT ISNULL(SUM(payout_amount), 0) FROM Insurance_payouts") + " ₽", Color.FromArgb(126, 87, 194));
        }

        private int SafeCount(string sql)
        {
            try { return Db.Count(sql); } catch { return 0; }
        }

        private string SafeRoundedMoney(string sql)
        {
            try
            {
                var raw = Db.Scalar(sql);
                var amount = Convert.ToDecimal(raw);
                return Math.Round(amount, 0, MidpointRounding.AwayFromZero).ToString("0");
            }
            catch
            {
                return "0";
            }
        }

        private void FillTiles()
        {
            _tilesPanel.Controls.Clear();
            var isHead = _user.Role == UserRole.DepartmentHead;
            var isManager = _user.Role == UserRole.Manager;
            var isSeniorAgent = _user.Role == UserRole.SeniorAgent;
            var isInsuranceAgent = _user.Role == UserRole.InsuranceAgent;

            AddTile("Клиенты", "Учет физических лиц и их контактных данных.", delegate { OpenModule("Клиенты", new ClientsForm(_user)); }, isHead || isManager || isSeniorAgent);
            AddTile("Автомобили", "VIN, госномер, модель, категория, мощность.", delegate { OpenModule("Автомобили", new VehiclesForm(_user)); }, isHead || isManager || isSeniorAgent);
            AddTile("Договоры", "Оформление полисов ОСАГО, КАСКО, ДСАГО.", delegate { OpenModule("Договоры", new ContractsForm(_user)); }, isHead || isManager || isSeniorAgent);
            AddTile("Страховые случаи", "Регистрация ДТП, угона, повреждений и ущерба.", delegate { OpenModule("Страховые случаи", new InsuranceCasesForm(_user)); }, isHead || isManager || isInsuranceAgent);
            AddTile("Выплаты", "Учет страховых выплат по случаям.", delegate { OpenModule("Выплаты", new PayoutsForm(_user)); }, isHead || isInsuranceAgent);
            AddTile("Сотрудники", "Учет сотрудников страховой компании.", delegate { OpenModule("Сотрудники", new EmployeesForm(_user)); }, isHead || isSeniorAgent || isInsuranceAgent);
            AddTile("Комиссии", "Просмотр начисленных комиссий.", delegate { OpenModule("Комиссии", new CommissionsForm()); }, isHead || isInsuranceAgent);
            AddTile("Отчеты", "Сводная аналитика по договорам, случаям и выплатам.", delegate { OpenModule("Отчеты", new ReportsForm()); }, true);
        }

        private void AddTile(string title, string description, Action action, bool visible)
        {
            if (!visible) return;
            var card = Theme.CreateCard(); card.Width = 250; card.Height = 172;
            var lblTitle = new Label { Text = title, Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            var lblDescription = new Label { Text = description, Dock = DockStyle.Fill, ForeColor = Theme.Muted, AutoEllipsis = true };
            var btnOpen = Theme.CreatePrimaryButton("Открыть", 110, true); btnOpen.Dock = DockStyle.Bottom; btnOpen.Click += delegate { action(); };
            card.Controls.Add(btnOpen); card.Controls.Add(lblDescription); card.Controls.Add(lblTitle);
            _tilesPanel.Controls.Add(card);
        }

        private void AddStatCard(string title, string value, Color color)
        {
            var valueFontSize = value.IndexOf("₽", StringComparison.Ordinal) >= 0 ? 18F : 22F;
            var card = Theme.CreateCard();
            card.Width = 180;
            card.Height = 110;
            card.BackColor = Theme.Surface;
            card.Margin = new Padding(0, 0, 16, 0);
            card.Padding = new Padding(14, 10, 14, 10);

            var accent = new Panel
            {
                Dock = DockStyle.Top,
                Height = 6,
                BackColor = color,
                Margin = new Padding(0, 0, 0, 8)
            };
            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 26,
                ForeColor = Theme.Muted,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblValue = new Label
            {
                Text = value,
                Dock = DockStyle.Bottom,
                Height = 42,
                Font = new Font("Segoe UI", valueFontSize, FontStyle.Bold),
                ForeColor = Theme.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            card.Controls.Add(accent);
            _statsPanel.Controls.Add(card);
        }

        private void OpenModule(string name, Form form)
        {
            LogService.Log("Открытие модуля", name);
            using (form) form.ShowDialog(this);
            FillStats();
        }
    }
}
