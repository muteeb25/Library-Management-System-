using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagement.DAL;

namespace LibraryManagement.Forms
{
    public class BorrowersForm : BaseCrudForm
    {
        private TextBox txtCardNo, txtName, txtAddress, txtPhone;
        private int selectedCardNo = -1;

        public BorrowersForm() : base("👤  Manage Borrowers") { LoadGrid(); }

        protected override void BuildInputPanel()
        {
            var tbl = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new Padding(4) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtCardNo  = MakeTextBox(); txtCardNo.ReadOnly = true; txtCardNo.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            txtName    = MakeTextBox(220);
            txtAddress = MakeTextBox(220);
            txtPhone   = MakeTextBox(220);

            tbl.Controls.Add(MakeLabel("Card No"),   0, 0); tbl.Controls.Add(txtCardNo,  1, 0);
            tbl.Controls.Add(MakeLabel("Name *"),    0, 1); tbl.Controls.Add(txtName,    1, 1);
            tbl.Controls.Add(MakeLabel("Address"),   0, 2); tbl.Controls.Add(txtAddress, 1, 2);
            tbl.Controls.Add(MakeLabel("Phone"),     0, 3); tbl.Controls.Add(txtPhone,   1, 3);

            inputPanel.Controls.Add(tbl);
        }

        public override void LoadGrid()
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT borrower_CardNo AS [Card No],
                       borrower_BorrowerName AS [Name],
                       borrower_BorrowerAddress AS [Address],
                       borrower_BorrowerPhone AS [Phone]
                FROM tbl_borrower ORDER BY borrower_BorrowerName");
            ToggleEditing(false);
        }

        protected override void SearchGrid(string term)
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT borrower_CardNo AS [Card No], borrower_BorrowerName AS [Name],
                       borrower_BorrowerAddress AS [Address], borrower_BorrowerPhone AS [Phone]
                FROM tbl_borrower
                WHERE borrower_BorrowerName LIKE @t OR borrower_BorrowerAddress LIKE @t OR borrower_BorrowerPhone LIKE @t
                ORDER BY borrower_BorrowerName",
                new[] { new SqlParameter("@t", "%" + term + "%") });
        }

        protected override void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = grid.Rows[e.RowIndex];
            selectedCardNo    = Convert.ToInt32(row.Cells["Card No"].Value);
            txtCardNo.Text    = selectedCardNo.ToString();
            txtName.Text      = row.Cells["Name"].Value?.ToString();
            txtAddress.Text   = row.Cells["Address"].Value?.ToString();
            txtPhone.Text     = row.Cells["Phone"].Value?.ToString();
            isEditing         = true;
            ToggleEditing(true);
            btnDelete.Enabled = true;
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (selectedCardNo <= 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO tbl_borrower (borrower_BorrowerName, borrower_BorrowerAddress, borrower_BorrowerPhone) VALUES (@n,@a,@p)",
                    new[] { new SqlParameter("@n", txtName.Text.Trim()),
                            new SqlParameter("@a", txtAddress.Text.Trim()),
                            new SqlParameter("@p", txtPhone.Text.Trim()) });
                MessageBox.Show("Borrower added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE tbl_borrower SET borrower_BorrowerName=@n, borrower_BorrowerAddress=@a, borrower_BorrowerPhone=@p WHERE borrower_CardNo=@id",
                    new[] { new SqlParameter("@n",  txtName.Text.Trim()),
                            new SqlParameter("@a",  txtAddress.Text.Trim()),
                            new SqlParameter("@p",  txtPhone.Text.Trim()),
                            new SqlParameter("@id", selectedCardNo) });
                MessageBox.Show("Borrower updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ClearInputs(); LoadGrid();
        }

        protected override void DeleteRecord()
        {
            if (selectedCardNo <= 0) return;
            if (MessageBox.Show("Delete this borrower?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book_loans WHERE book_loans_CardNo=@id", new[] { new SqlParameter("@id", selectedCardNo) });
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_borrower WHERE borrower_CardNo=@id",     new[] { new SqlParameter("@id", selectedCardNo) });
            ClearInputs(); LoadGrid();
        }

        protected override void ClearInputs()
        {
            txtCardNo.Text = ""; txtName.Text = ""; txtAddress.Text = ""; txtPhone.Text = "";
            selectedCardNo = -1;
            ToggleEditing(false);
        }
    }
}
