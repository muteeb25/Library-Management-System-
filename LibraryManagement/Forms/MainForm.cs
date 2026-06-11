using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagement.DAL;

namespace LibraryManagement.Forms
{
    public class MainForm : Form
    {
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Panel sidePanel;
        private Panel contentPanel;
        private Label lblTitle;

        public MainForm()
        {
            InitializeComponent();
            CheckConnection();
        }

        private void InitializeComponent()
        {
            this.Text = "Library Management System";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);
            this.IsMdiContainer = true;

            // ── TOP HEADER ──────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(26, 58, 108)
            };
            var lblHeader = new Label
            {
                Text = "📚  Library Management System",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            header.Controls.Add(lblHeader);

            // ── MENU STRIP ───────────────────────────────────────────
            menuStrip = new MenuStrip { BackColor = Color.FromArgb(36, 78, 138), ForeColor = Color.White, Renderer = new DarkMenuRenderer() };

            var mnuBooks     = CreateMenu("📖  Books",     new[] { ("Manage Books",     (EventHandler)OpenBooks) });
            var mnuBorrowers = CreateMenu("👤  Borrowers", new[] { ("Manage Borrowers", (EventHandler)OpenBorrowers) });
            var mnuBranches  = CreateMenu("🏢  Branches",  new[] { ("Manage Branches",  (EventHandler)OpenBranches) });
            var mnuLoans     = CreateMenu("📋  Loans",     new[] { ("Manage Loans",     (EventHandler)OpenLoans) });
            var mnuReports   = CreateMenu("📊  Reports",   new[]
            {
                ("Loans Per Branch",           (EventHandler)OpenLoansPerBranch),
                ("Books by Author & Branch",   (EventHandler)OpenBooksByAuthor),
                ("Active Loans View",          (EventHandler)OpenActiveLoans),
                ("Borrowers with 2+ Books",    (EventHandler)OpenHeavyBorrowers),
                ("Audit Log",                  (EventHandler)OpenAuditLog),
            });
            var mnuSettings  = CreateMenu("⚙️  Settings",  new[] { ("Connection Settings", (EventHandler)OpenSettings) });

            menuStrip.Items.AddRange(new ToolStripItem[] { mnuBooks, mnuBorrowers, mnuBranches, mnuLoans, mnuReports, mnuSettings });

            // ── STATUS BAR ───────────────────────────────────────────
            statusStrip = new StatusStrip { BackColor = Color.FromArgb(26, 58, 108) };
            statusLabel = new ToolStripStatusLabel("Ready") { ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f) };
            statusStrip.Items.Add(statusLabel);

            this.Controls.Add(header);
            this.Controls.Add(menuStrip);
            this.Controls.Add(statusStrip);
            this.MainMenuStrip = menuStrip;
        }

        private ToolStripMenuItem CreateMenu(string text, (string label, EventHandler handler)[] items)
        {
            var menu = new ToolStripMenuItem(text) { ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f) };
            foreach (var (label, handler) in items)
            {
                var item = new ToolStripMenuItem(label);
                item.Click += handler;
                menu.DropDownItems.Add(item);
            }
            return menu;
        }

        private void CheckConnection()
        {
            bool ok = DatabaseHelper.TestConnection();
            statusLabel.Text = ok
                ? "✅  Connected to: " + DatabaseHelper.ConnectionString
                : "❌  Not connected — go to Settings to configure";
            statusLabel.ForeColor = ok ? Color.LightGreen : Color.OrangeRed;
        }

        // ── OPEN FORMS ───────────────────────────────────────────────
        private void OpenMdi(Form f) { f.MdiParent = this; f.Show(); }
        private void OpenBooks(object s, EventArgs e)            => OpenMdi(new BooksForm());
        private void OpenBorrowers(object s, EventArgs e)        => OpenMdi(new BorrowersForm());
        private void OpenBranches(object s, EventArgs e)         => OpenMdi(new BranchesForm());
        private void OpenLoans(object s, EventArgs e)            => OpenMdi(new LoansForm());
        private void OpenLoansPerBranch(object s, EventArgs e)   => OpenMdi(new ReportLoansPerBranchForm());
        private void OpenBooksByAuthor(object s, EventArgs e)    => OpenMdi(new ReportBooksByAuthorForm());
        private void OpenActiveLoans(object s, EventArgs e)      => OpenMdi(new ReportActiveLoansForm());
        private void OpenHeavyBorrowers(object s, EventArgs e)   => OpenMdi(new ReportHeavyBorrowersForm());
        private void OpenAuditLog(object s, EventArgs e)         => OpenMdi(new AuditLogForm());
        private void OpenSettings(object s, EventArgs e)
        {
            var dlg = new ConnectionSettingsForm();
            if (dlg.ShowDialog() == DialogResult.OK) CheckConnection();
        }
    }

    // Dark renderer so menu text is white
    class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable()) { }
    }
    class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected         => Color.FromArgb(60, 120, 200);
        public override Color MenuItemBorder           => Color.FromArgb(80, 140, 220);
        public override Color ToolStripDropDownBackground => Color.FromArgb(36, 78, 138);
        public override Color MenuBorder               => Color.FromArgb(26, 58, 108);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 120, 200);
        public override Color MenuItemSelectedGradientEnd   => Color.FromArgb(60, 120, 200);
    }
}
