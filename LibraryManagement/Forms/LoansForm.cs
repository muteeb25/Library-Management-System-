using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagement.DAL;

namespace LibraryManagement.Forms
{
    public class LoansForm : BaseCrudForm
    {
        private TextBox      txtLoanID;
        private ComboBox     cboBook, cboBranch, cboBorrower;
        private DateTimePicker dtpDateOut, dtpDueDate;
        private int          selectedLoanID = -1;

        public LoansForm() : base("📋  Manage Book Loans") { LoadGrid(); }

        protected override void BuildInputPanel()
        {
            var tbl = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new Padding(4) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtLoanID   = MakeTextBox(); txtLoanID.ReadOnly = true; txtLoanID.BackColor = Color.FromArgb(240, 240, 240);
            cboBook     = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            cboBranch   = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            cboBorrower = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            dtpDateOut  = MakeDatePicker(); dtpDateOut.Value  = DateTime.Today;
            dtpDueDate  = MakeDatePicker(); dtpDueDate.Value  = DateTime.Today.AddDays(30);

            LoadCombos();

            tbl.Controls.Add(MakeLabel("Loan ID"),    0, 0); tbl.Controls.Add(txtLoanID,   1, 0);
            tbl.Controls.Add(MakeLabel("Book *"),     0, 1); tbl.Controls.Add(cboBook,     1, 1);
            tbl.Controls.Add(MakeLabel("Branch *"),   0, 2); tbl.Controls.Add(cboBranch,   1, 2);
            tbl.Controls.Add(MakeLabel("Borrower *"), 0, 3); tbl.Controls.Add(cboBorrower, 1, 3);
            tbl.Controls.Add(MakeLabel("Date Out *"), 0, 4); tbl.Controls.Add(dtpDateOut,  1, 4);
            tbl.Controls.Add(MakeLabel("Due Date *"), 0, 5); tbl.Controls.Add(dtpDueDate,  1, 5);

            inputPanel.Controls.Add(tbl);
        }

        private void LoadCombos()
        {
            cboBook.Items.Clear();
            var books = DatabaseHelper.ExecuteQuery("SELECT book_BookID, book_Title FROM tbl_book ORDER BY book_Title");
            foreach (System.Data.DataRow r in books.Rows)
                cboBook.Items.Add(new ComboItem(r["book_BookID"].ToString(), r["book_Title"].ToString()));

            cboBranch.Items.Clear();
            var branches = DatabaseHelper.ExecuteQuery("SELECT library_branch_BranchID, library_branch_BranchName FROM tbl_library_branch ORDER BY library_branch_BranchName");
            foreach (System.Data.DataRow r in branches.Rows)
                cboBranch.Items.Add(new ComboItem(r["library_branch_BranchID"].ToString(), r["library_branch_BranchName"].ToString()));

            cboBorrower.Items.Clear();
            var borrowers = DatabaseHelper.ExecuteQuery("SELECT borrower_CardNo, borrower_BorrowerName FROM tbl_borrower ORDER BY borrower_BorrowerName");
            foreach (System.Data.DataRow r in borrowers.Rows)
                cboBorrower.Items.Add(new ComboItem(r["borrower_CardNo"].ToString(), r["borrower_BorrowerName"].ToString()));
        }

        public override void LoadGrid()
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT l.book_loans_LoansID AS [Loan ID],
                       b.book_Title         AS [Book],
                       br.library_branch_BranchName AS [Branch],
                       bo.borrower_BorrowerName     AS [Borrower],
                       l.book_loans_DateOut AS [Date Out],
                       l.book_loans_DueDate AS [Due Date]
                FROM tbl_book_loans l
                JOIN tbl_book           b  ON l.book_loans_BookID   = b.book_BookID
                JOIN tbl_library_branch br ON l.book_loans_BranchID = br.library_branch_BranchID
                JOIN tbl_borrower       bo ON l.book_loans_CardNo   = bo.borrower_CardNo
                ORDER BY l.book_loans_DueDate");
            ToggleEditing(false);

            // Highlight overdue rows in red
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["Due Date"].Value is DateTime due && due < DateTime.Today)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }

        protected override void SearchGrid(string term)
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT l.book_loans_LoansID AS [Loan ID],
                       b.book_Title AS [Book],
                       br.library_branch_BranchName AS [Branch],
                       bo.borrower_BorrowerName AS [Borrower],
                       l.book_loans_DateOut AS [Date Out],
                       l.book_loans_DueDate AS [Due Date]
                FROM tbl_book_loans l
                JOIN tbl_book           b  ON l.book_loans_BookID   = b.book_BookID
                JOIN tbl_library_branch br ON l.book_loans_BranchID = br.library_branch_BranchID
                JOIN tbl_borrower       bo ON l.book_loans_CardNo   = bo.borrower_CardNo
                WHERE b.book_Title LIKE @t OR br.library_branch_BranchName LIKE @t OR bo.borrower_BorrowerName LIKE @t",
                new[] { new SqlParameter("@t", "%" + term + "%") });
        }

        protected override void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = grid.Rows[e.RowIndex];
            selectedLoanID = Convert.ToInt32(row.Cells["Loan ID"].Value);
            txtLoanID.Text = selectedLoanID.ToString();

            SetComboByText(cboBook,     row.Cells["Book"].Value?.ToString());
            SetComboByText(cboBranch,   row.Cells["Branch"].Value?.ToString());
            SetComboByText(cboBorrower, row.Cells["Borrower"].Value?.ToString());

            if (row.Cells["Date Out"].Value is DateTime dOut) dtpDateOut.Value = dOut;
            if (row.Cells["Due Date"].Value is DateTime dDue) dtpDueDate.Value = dDue;

            isEditing = true; ToggleEditing(true); btnDelete.Enabled = true;
        }

        private void SetComboByText(ComboBox cbo, string text)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
                if (cbo.Items[i] is ComboItem ci && ci.Text == text) { cbo.SelectedIndex = i; return; }
        }

        protected override void SaveRecord()
        {
            if (cboBook.SelectedIndex < 0 || cboBranch.SelectedIndex < 0 || cboBorrower.SelectedIndex < 0)
            { MessageBox.Show("Please select Book, Branch and Borrower.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (dtpDueDate.Value <= dtpDateOut.Value)
            { MessageBox.Show("Due date must be after date out.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int bookID     = int.Parse(((ComboItem)cboBook.SelectedItem).Value);
            int branchID   = int.Parse(((ComboItem)cboBranch.SelectedItem).Value);
            int cardNo     = int.Parse(((ComboItem)cboBorrower.SelectedItem).Value);

            if (selectedLoanID <= 0)
            {
                DatabaseHelper.ExecuteNonQuery(@"
                    INSERT INTO tbl_book_loans (book_loans_BookID, book_loans_BranchID, book_loans_CardNo, book_loans_DateOut, book_loans_DueDate)
                    VALUES (@b, @br, @c, @do, @dd)",
                    new[] { new SqlParameter("@b",  bookID),
                            new SqlParameter("@br", branchID),
                            new SqlParameter("@c",  cardNo),
                            new SqlParameter("@do", dtpDateOut.Value),
                            new SqlParameter("@dd", dtpDueDate.Value) });
                MessageBox.Show("Loan created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(@"
                    UPDATE tbl_book_loans SET book_loans_BookID=@b, book_loans_BranchID=@br,
                    book_loans_CardNo=@c, book_loans_DateOut=@do, book_loans_DueDate=@dd
                    WHERE book_loans_LoansID=@id",
                    new[] { new SqlParameter("@b",  bookID),
                            new SqlParameter("@br", branchID),
                            new SqlParameter("@c",  cardNo),
                            new SqlParameter("@do", dtpDateOut.Value),
                            new SqlParameter("@dd", dtpDueDate.Value),
                            new SqlParameter("@id", selectedLoanID) });
                MessageBox.Show("Loan updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ClearInputs(); LoadGrid();
        }

        protected override void DeleteRecord()
        {
            if (selectedLoanID <= 0) return;
            if (MessageBox.Show("Delete this loan?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book_loans WHERE book_loans_LoansID=@id", new[] { new SqlParameter("@id", selectedLoanID) });
            ClearInputs(); LoadGrid();
        }

        protected override void ClearInputs()
        {
            txtLoanID.Text = "";
            cboBook.SelectedIndex = cboBranch.SelectedIndex = cboBorrower.SelectedIndex = -1;
            dtpDateOut.Value = DateTime.Today; dtpDueDate.Value = DateTime.Today.AddDays(30);
            selectedLoanID = -1; ToggleEditing(false);
        }
    }

    // Helper for ComboBox items that have ID + display text
    public class ComboItem
    {
        public string Value { get; }
        public string Text  { get; }
        public ComboItem(string value, string text) { Value = value; Text = text; }
        public override string ToString() => Text;
    }
}
