using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagement.DAL;

namespace LibraryManagement.Forms
{
    public class BooksForm : BaseCrudForm
    {
        private TextBox  txtBookID, txtTitle;
        private ComboBox cboPublisher;
        private int      selectedBookID = -1;

        public BooksForm() : base("📖  Manage Books") { LoadGrid(); }

        protected override void BuildInputPanel()
        {
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top, ColumnCount = 2,
                AutoSize = true, Padding = new Padding(4)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtBookID   = MakeTextBox(); txtBookID.ReadOnly = true; txtBookID.BackColor = Color.FromArgb(240, 240, 240);
            txtTitle    = MakeTextBox(220);
            cboPublisher = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };

            LoadPublishers();

            tbl.Controls.Add(MakeLabel("Book ID"),    0, 0); tbl.Controls.Add(txtBookID,    1, 0);
            tbl.Controls.Add(MakeLabel("Title *"),    0, 1); tbl.Controls.Add(txtTitle,     1, 1);
            tbl.Controls.Add(MakeLabel("Publisher *"),0, 2); tbl.Controls.Add(cboPublisher, 1, 2);

            var lblHint = new Label { Text = "* Required fields", ForeColor = Color.Gray, Font = new Font("Segoe UI", 8f), AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
            tbl.Controls.Add(lblHint, 1, 3);

            inputPanel.Controls.Add(tbl);
        }

        private void LoadPublishers()
        {
            cboPublisher.Items.Clear();
            var dt = DatabaseHelper.ExecuteQuery("SELECT publisher_PublisherName FROM tbl_publisher ORDER BY publisher_PublisherName");
            foreach (DataRow r in dt.Rows)
                cboPublisher.Items.Add(r[0].ToString());
        }

        public override void LoadGrid()
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT b.book_BookID AS [ID], b.book_Title AS [Title],
                       b.book_PublisherName AS [Publisher],
                       a.book_authors_AuthorName AS [Author]
                FROM tbl_book b
                LEFT JOIN tbl_book_authors a ON b.book_BookID = a.book_authors_BookID
                ORDER BY b.book_Title");
            ToggleEditing(false);
        }

        protected override void SearchGrid(string term)
        {
            grid.DataSource = DatabaseHelper.ExecuteQuery(@"
                SELECT b.book_BookID AS [ID], b.book_Title AS [Title],
                       b.book_PublisherName AS [Publisher],
                       a.book_authors_AuthorName AS [Author]
                FROM tbl_book b
                LEFT JOIN tbl_book_authors a ON b.book_BookID = a.book_authors_BookID
                WHERE b.book_Title LIKE @t OR b.book_PublisherName LIKE @t OR a.book_authors_AuthorName LIKE @t
                ORDER BY b.book_Title",
                new[] { new SqlParameter("@t", "%" + term + "%") });
        }

        protected override void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = grid.Rows[e.RowIndex];
            selectedBookID       = Convert.ToInt32(row.Cells["ID"].Value);
            txtBookID.Text       = selectedBookID.ToString();
            txtTitle.Text        = row.Cells["Title"].Value?.ToString();
            cboPublisher.Text    = row.Cells["Publisher"].Value?.ToString();
            isEditing            = true;
            ToggleEditing(true);
            btnDelete.Enabled    = true;
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || cboPublisher.SelectedIndex < 0)
            { MessageBox.Show("Please fill in all required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (selectedBookID <= 0)  // INSERT
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO tbl_book (book_Title, book_PublisherName) VALUES (@t, @p)",
                    new[] { new SqlParameter("@t", txtTitle.Text.Trim()),
                            new SqlParameter("@p", cboPublisher.SelectedItem.ToString()) });
                MessageBox.Show("Book added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else  // UPDATE
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE tbl_book SET book_Title=@t, book_PublisherName=@p WHERE book_BookID=@id",
                    new[] { new SqlParameter("@t",  txtTitle.Text.Trim()),
                            new SqlParameter("@p",  cboPublisher.SelectedItem.ToString()),
                            new SqlParameter("@id", selectedBookID) });
                MessageBox.Show("Book updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ClearInputs(); LoadGrid();
        }

        protected override void DeleteRecord()
        {
            if (selectedBookID <= 0) return;
            if (MessageBox.Show("Delete this book? This will also remove related loans and copies.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book_loans  WHERE book_loans_BookID=@id", new[] { new SqlParameter("@id", selectedBookID) });
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book_copies WHERE book_copies_BookID=@id", new[] { new SqlParameter("@id", selectedBookID) });
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book_authors WHERE book_authors_BookID=@id", new[] { new SqlParameter("@id", selectedBookID) });
            DatabaseHelper.ExecuteNonQuery("DELETE FROM tbl_book WHERE book_BookID=@id", new[] { new SqlParameter("@id", selectedBookID) });
            ClearInputs(); LoadGrid();
        }

        protected override void ClearInputs()
        {
            txtBookID.Text = ""; txtTitle.Text = "";
            cboPublisher.SelectedIndex = -1;
            selectedBookID = -1;
            ToggleEditing(false);
        }
    }
}
