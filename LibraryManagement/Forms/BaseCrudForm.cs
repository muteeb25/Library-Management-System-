using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms
{
    public abstract class BaseCrudForm : Form
    {
        protected DataGridView grid;
        protected Panel inputPanel;
        protected TextBox txtSearch;
        protected Button btnSearch, btnAdd, btnSave, btnDelete, btnClear, btnRefresh;
        protected Label lblFormTitle;
        protected bool isEditing = false;

        protected BaseCrudForm(string title)
        {
            this.Text = title;
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);
            BuildLayout(title);
        }

        private void BuildLayout(string title)
        {
            // TITLE BAR
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(36, 78, 138) };
            lblFormTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            titleBar.Controls.Add(lblFormTitle);

            // SEARCH BAR
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.FromArgb(225, 232, 245),
                Padding = new Padding(8, 6, 8, 6)
            };
            txtSearch = new TextBox { PlaceholderText = "Search...", Width = 250, Dock = DockStyle.Left };
            btnSearch = MakeButton("Search", Color.FromArgb(36, 78, 138));
            btnRefresh = MakeButton("Refresh", Color.FromArgb(90, 90, 100));
            btnSearch.Dock = DockStyle.Left;
            btnRefresh.Dock = DockStyle.Left;
            btnSearch.Click += (s, e) => SearchGrid(txtSearch.Text.Trim());
            btnRefresh.Click += (s, e) => { txtSearch.Text = ""; LoadGrid(); };
            searchPanel.Controls.AddRange(new Control[] { btnRefresh, btnSearch, txtSearch });

            // ACTION BAR
            var actionBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(225, 232, 245)
            };
            btnAdd = MakeButton("New", Color.FromArgb(14, 130, 80));
            btnSave = MakeButton("Save", Color.FromArgb(36, 78, 138));
            btnDelete = MakeButton("Delete", Color.FromArgb(180, 40, 40));
            btnClear = MakeButton("Clear", Color.FromArgb(100, 100, 115));
            btnAdd.Location = new Point(8, 10);
            btnSave.Location = new Point(118, 10);
            btnDelete.Location = new Point(228, 10);
            btnClear.Location = new Point(338, 10);
            btnAdd.Click += (s, e) => { isEditing = false; ClearInputs(); ToggleEditing(true); };
            btnSave.Click += (s, e) => SaveRecord();
            btnDelete.Click += (s, e) => DeleteRecord();
            btnClear.Click += (s, e) => { ClearInputs(); ToggleEditing(false); };
            actionBar.Controls.AddRange(new Control[] { btnAdd, btnSave, btnDelete, btnClear });

            // RIGHT PANEL
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 340,
                BackColor = Color.White
            };
            inputPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14, 12, 14, 8),
                AutoScroll = true
            };
            BuildInputPanel();
            rightPanel.Controls.Add(inputPanel);

            // GRID
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(220, 225, 235),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(36, 78, 138),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    SelectionBackColor = Color.FromArgb(36, 78, 138)
                },
                RowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(40, 40, 60),
                    SelectionBackColor = Color.FromArgb(190, 215, 250),
                    SelectionForeColor = Color.FromArgb(20, 20, 50)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(240, 244, 252)
                }
            };
            grid.CellClick += Grid_CellClick;
            ToggleEditing(false);

            // Add in correct order: Fill last
            this.Controls.Add(grid);
            this.Controls.Add(rightPanel);
            this.Controls.Add(actionBar);
            this.Controls.Add(searchPanel);
            this.Controls.Add(titleBar);
        }

        protected void ToggleEditing(bool editing)
        {
            if (btnSave == null || grid == null || inputPanel == null) return;
            btnSave.Enabled = editing;
            btnDelete.Enabled = !editing && grid.SelectedRows.Count > 0;
            btnClear.Enabled = editing;
            btnAdd.Enabled = !editing;
            foreach (Control c in inputPanel.Controls)
                c.Enabled = editing;
        }

        protected Button MakeButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 30,
                Width = 100,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
        }

        protected Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = 130,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 60, 80)
            };
        }

        protected TextBox MakeTextBox(int width = 190)
        {
            return new TextBox
            {
                Width = width,
                Height = 24,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        protected DateTimePicker MakeDatePicker()
        {
            return new DateTimePicker
            {
                Width = 190,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5f)
            };
        }

        protected abstract void BuildInputPanel();
        public abstract void LoadGrid();
        protected abstract void SearchGrid(string term);
        protected abstract void Grid_CellClick(object sender, DataGridViewCellEventArgs e);
        protected abstract void SaveRecord();
        protected abstract void DeleteRecord();
        protected abstract void ClearInputs();
    }
}
