using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using LibraryManagement.DAL;

namespace LibraryManagement.Forms
{
    public class BranchesForm : BaseCrudForm
    {
        private TextBox txtBranchID, txtName, txtAddress;
        private int selectedBranchID = -1;

        public BranchesForm() : base("🏢  Manage Library Branches") { LoadGrid(); }

        protected override void BuildInputPanel()
        {
            var tbl = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new Padding(4) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtBranchID = MakeTextBox(); txtBranchID.ReadOnly = true; txtBranchID.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            txtName     = MakeTextBox(220);
            txtAddress  = MakeTextBox(220);

            tbl.Controls.Add(MakeLabel("Branch ID"),  0, 0); tbl.Controls.Add(txtBranchID, 1, 0);
            tbl.Controls.Add(MakeLabel("Branch Name*"),0,1); tbl.Controls.Add(txtName,     1, 1);
            tbl.Controls.Add(MakeLabel("Address"),    0, 2); tbl.Controls.Add(txtAddress,  1, 2);

            inputPanel.Controls.Add(tbl);
        }

        public override void LoadGrid()
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT library_branch_BranchID AS [ID],
                       library_branch_BranchName AS [Branch Name],
                       library_branch_BranchAddress AS [Address]
                FROM tbl_library_branch ORDER BY library_branch_BranchName");
            ToggleEditing(false);
        }

        protected override void SearchGrid(string term)
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT library_branch_BranchID AS [ID],
                       library_branch_BranchName AS [Branch Name],
                       library_branch_BranchAddress AS [Address]
                FROM tbl_library_branch
                WHERE library_branch_BranchName LIKE @t OR library_branch_BranchAddress LIKE @t",
                new[] { new SqlParameter("@t", "%" + term + "%") });
        }

        protected override void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = grid.Rows[e.RowIndex];
            selectedBranchID    = Convert.ToInt32(row.Cells["ID"].Value);
            txtBranchID.Text    = selectedBranchID.ToString();
            txtName.Text        = row.Cells["Branch Name"].Value?.ToString();
            txtAddress.Text     = row.Cells["Address"].Value?.ToString();
            isEditing           = true;
            ToggleEditing(true);
            btnDelete.Enabled   = true;
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Branch Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (selectedBranchID <= 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO tbl_library_branch (library_branch_BranchName, library_branch_BranchAddress) VALUES (@n,@a)",
                    new[] { new SqlParameter("@n", txtName.Text.Trim()), new SqlParameter("@a", txtAddress.Text.Trim()) });
                MessageBox.Show("Branch added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE tbl_library_branch SET library_branch_BranchName=@n, library_branch_BranchAddress=@a WHERE library_branch_BranchID=@id",
                    new[] { new SqlParameter("@n", txtName.Text.Trim()), new SqlParameter("@a", txtAddress.Text.Trim()), new SqlParameter("@id", selectedBranchID) });
                MessageBox.Show("Branch updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ClearInputs(); LoadGrid();
        }

        protected override void DeleteRecord()
        {
            if (selectedBranchID <= 0) return;
            if (MessageBox.Show("Delete this branch? All related data will be removed.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book_loans  WHERE book_loans_BranchID=@id",  new[] { new SqlParameter("@id", selectedBranchID) });
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book_copies WHERE book_copies_BranchID=@id", new[] { new SqlParameter("@id", selectedBranchID) });
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_library_branch WHERE library_branch_BranchID=@id", new[] { new SqlParameter("@id", selectedBranchID) });
            ClearInputs(); LoadGrid();
        }

        protected override void ClearInputs()
        {
            txtBranchID.Text = ""; txtName.Text = ""; txtAddress.Text = "";
            selectedBranchID = -1; ToggleEditing(false);
        }
    }
}
