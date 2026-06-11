using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagement.DAL;

namespace LibraryManagement.Forms
{
    // ─────────────────────────────────────────────────────────────
    //  BASE REPORT FORM  (read-only grid + export info)
    // ─────────────────────────────────────────────────────────────
    public abstract class BaseReportForm : Form
    {
        protected DataGridView grid;
        protected Label        lblRowCount;

        // Subclasses that need extra filter controls override this
        protected virtual void BuildLayout_Extra(Panel toolBar) { }

        protected BaseReportForm(string title)
        {
            this.Text          = title;
            this.Size          = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor     = Color.FromArgb(245, 247, 250);
            this.Font          = new Font("Segoe UI", 9f);
            BuildLayout(title);
        }

        private void BuildLayout(string title)
        {
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(36, 78, 138) };
            var lbl = new Label { Text = "📊  " + title, Dock = DockStyle.Fill, ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) };
            titleBar.Controls.Add(lbl);

            var toolBar = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.FromArgb(225, 232, 245), Padding = new Padding(8, 5, 8, 5) };
            var btnRefresh = new Button { Text = "↺ Refresh", Width = 90, Dock = DockStyle.Left,
                BackColor = Color.FromArgb(36, 78, 138), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => RunReport();
            lblRowCount = new Label { Dock = DockStyle.Right, AutoSize = false, Width = 160, TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(60, 60, 80) };
            toolBar.Controls.AddRange(new Control[] { btnRefresh, lblRowCount });

            grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(220, 225, 235),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                { BackColor = Color.FromArgb(36, 78, 138), ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold) },
                RowsDefaultCellStyle = new DataGridViewCellStyle
                { BackColor = Color.White, SelectionBackColor = Color.FromArgb(190, 215, 250), SelectionForeColor = Color.FromArgb(20,20,50) },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 244, 252) }
            };

            this.Controls.Add(grid);
            this.Controls.Add(toolBar);
            this.Controls.Add(titleBar);

            this.Load += (s, e) => RunReport();
        }

        protected void SetGrid(DataTable dt)
        {
            grid.DataSource = dt;
            lblRowCount.Text = $"{dt.Rows.Count} row(s) returned";
        }

        protected abstract void RunReport();
    }

    // ─────────────────────────────────────────────────────────────
    //  REPORT 1 — Total Loans Per Branch  (GROUP BY + COUNT)
    // ─────────────────────────────────────────────────────────────
    public class ReportLoansPerBranchForm : BaseReportForm
    {
        public ReportLoansPerBranchForm() : base("Total Loans Per Branch") { }

        protected override void RunReport()
        {
            SetGrid(DatabaseHelper.ExecuteQuery(@"
                SELECT branch.library_branch_BranchName AS [Branch],
                       COUNT(*) AS [Total Loans]
                FROM tbl_book_loans loans
                JOIN tbl_library_branch branch ON loans.book_loans_BranchID = branch.library_branch_BranchID
                GROUP BY branch.library_branch_BranchName
                ORDER BY [Total Loans] DESC"));
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  REPORT 2 — Books by Author & Branch  (multi-JOIN)
    // ─────────────────────────────────────────────────────────────
    public class ReportBooksByAuthorForm : BaseReportForm
    {
        private ComboBox cboAuthor;
        private ComboBox cboBranch;

        public ReportBooksByAuthorForm() : base("Books by Author at Branch") { }

        protected override void RunReport()
        {
            string author = cboAuthor?.SelectedItem?.ToString() ?? "J.K. Rowling";
            string branch = cboBranch?.SelectedItem?.ToString() ?? "Central";

            SetGrid(DatabaseHelper.ExecuteQuery(@"
                SELECT branch.library_branch_BranchName AS [Branch],
                       book.book_Title AS [Book Title],
                       copies.book_copies_No_Of_Copies AS [Copies]
                FROM tbl_book_authors authors
                JOIN tbl_book           book   ON authors.book_authors_BookID  = book.book_BookID
                JOIN tbl_book_copies    copies ON book.book_BookID             = copies.book_copies_BookID
                JOIN tbl_library_branch branch ON copies.book_copies_BranchID = branch.library_branch_BranchID
                WHERE authors.book_authors_AuthorName = @a AND branch.library_branch_BranchName = @b",
                new[] { new SqlParameter("@a", author), new SqlParameter("@b", branch) }));
        }

        protected override void BuildLayout_Extra(Panel toolBar)
        {
            // Author combo
            cboAuthor = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Left, Font = new Font("Segoe UI", 9f) };
            var authors = DatabaseHelper.ExecuteQuery("SELECT DISTINCT book_authors_AuthorName FROM tbl_book_authors ORDER BY book_authors_AuthorName");
            foreach (DataRow r in authors.Rows) cboAuthor.Items.Add(r[0]);
            if (cboAuthor.Items.Count > 0) cboAuthor.SelectedIndex = 0;
            cboAuthor.SelectedIndexChanged += (s, e) => RunReport();

            // Branch combo
            cboBranch = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Left, Font = new Font("Segoe UI", 9f) };
            var branches = DatabaseHelper.ExecuteQuery("SELECT library_branch_BranchName FROM tbl_library_branch ORDER BY library_branch_BranchName");
            foreach (DataRow r in branches.Rows) cboBranch.Items.Add(r[0]);
            if (cboBranch.Items.Count > 0) cboBranch.SelectedIndex = 0;
            cboBranch.SelectedIndexChanged += (s, e) => RunReport();

            var lblA = new Label { Text = "Author:", Dock = DockStyle.Left, Width = 55, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(40, 40, 60) };
            var lblB = new Label { Text = "Branch:", Dock = DockStyle.Left, Width = 55, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(40, 40, 60) };

            toolBar.Controls.AddRange(new Control[] { cboBranch, lblB, cboAuthor, lblA });
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  REPORT 3 — Active Loans View  (reads the SQL VIEW)
    // ─────────────────────────────────────────────────────────────
    public class ReportActiveLoansForm : BaseReportForm
    {
        public ReportActiveLoansForm() : base("Active Loans (vw_ActiveLoans)") { }

        protected override void RunReport()
        {
            SetGrid(DatabaseHelper.ExecuteQuery("SELECT * FROM vw_ActiveLoans ORDER BY DueDate"));
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  REPORT 4 — Borrowers with 2+ books  (GROUP BY + HAVING)
    // ─────────────────────────────────────────────────────────────
    public class ReportHeavyBorrowersForm : BaseReportForm
    {
        public ReportHeavyBorrowersForm() : base("Borrowers with 2 or More Books Checked Out") { }

        protected override void RunReport()
        {
            SetGrid(DatabaseHelper.ExecuteQuery(@"
                SELECT borrower.borrower_BorrowerName    AS [Name],
                       borrower.borrower_BorrowerAddress AS [Address],
                       COUNT(*) AS [Books Checked Out]
                FROM tbl_book_loans loans
                JOIN tbl_borrower borrower ON loans.book_loans_CardNo = borrower.borrower_CardNo
                GROUP BY borrower.borrower_BorrowerName, borrower.borrower_BorrowerAddress
                HAVING COUNT(*) >= 2
                ORDER BY [Books Checked Out] DESC"));
        }
    }
}
