using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagement.DAL;

namespace LibraryManagement.Forms
{
    // ─────────────────────────────────────────────────────────────
    //  AUDIT LOG FORM  — shows tbl_loan_audit_log (filled by triggers)
    // ─────────────────────────────────────────────────────────────
    public class AuditLogForm : Form
    {
        private DataGridView grid;

        public AuditLogForm()
        {
            this.Text          = "🔍  Loan Audit Log";
            this.Size          = new Size(700, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor     = Color.FromArgb(245, 247, 250);
            this.Font          = new Font("Segoe UI", 9f);

            var titleBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(36, 78, 138) };
            var lblTitle = new Label { Text = "🔍  Loan Audit Log (Trigger Output)", Dock = DockStyle.Fill,
                ForeColor = Color.White, Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) };
            titleBar.Controls.Add(lblTitle);

            var toolBar = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.FromArgb(225, 232, 245), Padding = new Padding(8, 5, 8, 5) };
            var btnRefresh = new Button { Text = "↺ Refresh", Width = 90, Dock = DockStyle.Left, Height = 26,
                BackColor = Color.FromArgb(36, 78, 138), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadLog();

            var btnClear = new Button { Text = "🗑 Clear Log", Width = 100, Dock = DockStyle.Left, Height = 26,
                BackColor = Color.FromArgb(160, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) =>
            {
                if (MessageBox.Show("Clear all audit log entries?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                { DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_loan_audit_log"); LoadLog(); }
            };

            var lblNote = new Label { Text = "Populated automatically by AFTER INSERT / AFTER DELETE triggers", Dock = DockStyle.Left,
                AutoSize = false, Width = 400, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(80, 80, 100), Font = new Font("Segoe UI", 8f, FontStyle.Italic) };

            toolBar.Controls.AddRange(new Control[] { btnRefresh, btnClear, lblNote });

            grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(220, 225, 235),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                { BackColor = Color.FromArgb(36, 78, 138), ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold) },
                RowsDefaultCellStyle = new DataGridViewCellStyle
                { BackColor = Color.White, SelectionBackColor = Color.FromArgb(190, 215, 250) },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 244, 252) }
            };

            this.Controls.Add(grid);
            this.Controls.Add(toolBar);
            this.Controls.Add(titleBar);

            this.Load += (s, e) => LoadLog();
        }

        private void LoadLog()
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT log_ID AS [Log #],
                       log_Action AS [Action],
                       log_BookID AS [Book ID],
                       log_CardNo AS [Card No],
                       log_Timestamp AS [Timestamp]
                FROM tbl_loan_audit_log
                ORDER BY log_Timestamp DESC");
            grid.DataSource = dt;

            // Colour-code INSERT (green) vs DELETE (red)
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["Action"].Value?.ToString() == "INSERT")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 230);
                else if (row.Cells["Action"].Value?.ToString() == "DELETE")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  CONNECTION SETTINGS FORM
    // ─────────────────────────────────────────────────────────────
    public class ConnectionSettingsForm : Form
    {
        private TextBox txtServer, txtDatabase, txtUsername, txtPassword;
        private CheckBox chkWindowsAuth;
        private Label lblStatus;

        public ConnectionSettingsForm()
        {
            this.Text = "⚙️  Connection Settings";
            this.Size = new Size(460, 390);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            var tbl = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new Padding(20, 16, 20, 8) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtServer   = new TextBox { Width = 240, Text = @".\SQLEXPRESS" };
            txtDatabase = new TextBox { Width = 240, Text = "db_LibraryManagement" };
            txtUsername = new TextBox { Width = 240 };
            txtPassword = new TextBox { Width = 240, UseSystemPasswordChar = true };
            chkWindowsAuth = new CheckBox { Text = "Windows Authentication", Width = 240, Checked = true };
            chkWindowsAuth.CheckedChanged += (s, e) =>
            {
                txtUsername.Enabled = !chkWindowsAuth.Checked;
                txtPassword.Enabled = !chkWindowsAuth.Checked;
            };
            txtUsername.Enabled = txtPassword.Enabled = false;

            tbl.Controls.Add(MakeLbl("Server"),    0, 0); tbl.Controls.Add(txtServer,    1, 0);
            tbl.Controls.Add(MakeLbl("Database"),  0, 1); tbl.Controls.Add(txtDatabase,  1, 1);
            tbl.Controls.Add(MakeLbl("Auth"),      0, 2); tbl.Controls.Add(chkWindowsAuth, 1, 2);
            tbl.Controls.Add(MakeLbl("Username"),  0, 3); tbl.Controls.Add(txtUsername,  1, 3);
            tbl.Controls.Add(MakeLbl("Password"),  0, 4); tbl.Controls.Add(txtPassword,  1, 4);

            lblStatus = new Label { Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray };

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(240, 243, 250),
                FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10, 8, 10, 8) };

            var btnOk = new Button { Text = "✔ Apply", Width = 90, Height = 30,
                BackColor = Color.FromArgb(36, 78, 138), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnTest = new Button { Text = "🔌 Test", Width = 90, Height = 30,
                BackColor = Color.FromArgb(14, 130, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            btnTest.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button { Text = "Cancel", Width = 80, Height = 30,
                BackColor = Color.FromArgb(100, 100, 115), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnTest.Click   += BtnTest_Click;
            btnOk.Click     += BtnOk_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnPanel.Controls.AddRange(new Control[] { btnCancel, btnOk, btnTest });
            this.Controls.Add(btnPanel);
            this.Controls.Add(lblStatus);
            this.Controls.Add(tbl);
        }

        private Label MakeLbl(string t) => new Label { Text = t, AutoSize = false, Height = 26, TextAlign = ContentAlignment.MiddleLeft };

        private string BuildConnectionString()
        {
            string auth = chkWindowsAuth.Checked
                ? "Integrated Security=True"
                : $"User Id={txtUsername.Text};Password={txtPassword.Text}";
            return $"Server={txtServer.Text.Trim()};Database={txtDatabase.Text.Trim()};{auth};";
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            DatabaseHelper.ConnectionString = BuildConnectionString();
            bool ok = DatabaseHelper.TestConnection();
            lblStatus.Text      = ok ? "✅  Connection successful!" : "❌  Connection failed. Check settings.";
            lblStatus.ForeColor = ok ? Color.DarkGreen : Color.DarkRed;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            DatabaseHelper.ConnectionString = BuildConnectionString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
