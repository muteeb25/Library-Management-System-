# 📚 Library Management System — Setup Guide
## C# Windows Forms + SQL Server

---

## STEP 1 — Run the SQL Script

1. Open **SQL Server Management Studio (SSMS)**
2. Open the provided SQL file (`db_LibraryManagement.sql`)
3. Click **Execute (F5)**
   - This creates the database, all tables, indexes, views, stored procedures, and triggers
   - It also inserts all sample data

---

## STEP 2 — Open the C# Project

### Prerequisites
- Visual Studio 2022 (Community or higher) — **free** at https://visualstudio.microsoft.com/
  - During install select: **".NET desktop development"** workload
- OR: .NET 6 SDK + VS Code with C# extension

### Open in Visual Studio
1. Open **Visual Studio 2022**
2. Click **"Open a project or solution"**
3. Navigate to the `LibraryManagement` folder
4. Select `LibraryManagement.csproj`
5. Press **F5** to build and run

---

## STEP 3 — Configure the Database Connection

When the app launches:
1. Go to **⚙️ Settings → Connection Settings**
2. Enter your SQL Server details:

| Field    | Example value             |
|----------|--------------------------|
| Server   | `.\SQLEXPRESS`           |
| Server   | `localhost`              |
| Server   | `MYPC\SQLEXPRESS`        |
| Database | `db_LibraryManagement`   |

3. Check **Windows Authentication** (recommended, uses your Windows login)
4. Click **🔌 Test** — you should see "✅ Connection successful!"
5. Click **✔ Apply**

> **Can't connect?** Make sure SQL Server is running. Open **SQL Server Configuration Manager**
> and verify "SQL Server (SQLEXPRESS)" or "SQL Server (MSSQLSERVER)" is Started.

---

## FEATURES

### 📖 Books
- Add, edit, delete books
- Linked to publishers from `tbl_publisher`
- Search by title, publisher, or author

### 👤 Borrowers
- Full CRUD for borrowers
- Search by name, address, or phone

### 🏢 Branches
- Manage library branches
- Cascades: deleting a branch removes its loans and copies

### 📋 Loans
- Create, update, delete loan records
- Dropdowns auto-populated from books, branches, borrowers
- **Overdue loans highlighted in red**
- Date validation (due date must be after date out)

### 📊 Reports
| Report | SQL Concept Used |
|--------|-----------------|
| Total Loans Per Branch | `GROUP BY` + `COUNT` |
| Books by Author at Branch | Multi-table `JOIN` + filter |
| Active Loans View | Reads from `vw_ActiveLoans` (SQL VIEW) |
| Borrowers with 2+ Books | `GROUP BY` + `HAVING COUNT(*) >= 2` |
| Audit Log | AFTER INSERT / AFTER DELETE triggers |

### 🔍 Audit Log
- Automatically filled by SQL **TRIGGERS**:
  - `trg_AfterLoanInsert` — logs every new loan
  - `trg_AfterLoanDelete` — logs every deleted loan
- Green rows = INSERT, Red rows = DELETE

---

## PROJECT STRUCTURE

```
LibraryManagement/
├── Program.cs                      ← Application entry point
├── LibraryManagement.csproj        ← Project file (.NET 6)
├── DAL/
│   └── DatabaseHelper.cs           ← SQL connection + query helpers
└── Forms/
    ├── MainForm.cs                 ← MDI shell, menu, status bar
    ├── BaseCrudForm.cs             ← Reusable CRUD layout (abstract)
    ├── BooksForm.cs                ← Books CRUD
    ├── BorrowersForm.cs            ← Borrowers CRUD
    ├── BranchesForm.cs             ← Branches CRUD
    ├── LoansForm.cs                ← Loans CRUD + overdue highlighting
    ├── ReportForms.cs              ← All 4 report windows
    └── AuditAndSettingsForms.cs    ← Audit log + Connection settings
```

---

## SQL CONCEPTS COVERED

| Concept | Where Used |
|---------|-----------|
| DDL (CREATE, DROP, ALTER) | Setup script |
| DML (INSERT, UPDATE, DELETE) | All CRUD forms |
| SELECT + WHERE + LIKE | Search in every form |
| BETWEEN, IN, ORDER BY | Query helpers |
| GROUP BY + HAVING | Loans Per Branch report, Heavy Borrowers report |
| INNER JOIN, LEFT JOIN | All multi-table queries |
| Subqueries | Borrowers with no loans |
| Aggregate Functions (COUNT, SUM, AVG) | Reports |
| VIEWS | Active Loans report reads `vw_ActiveLoans` |
| STORED PROCEDURES | Available via SSMS EXEC commands |
| TRIGGERS | Audit log auto-filled on loan insert/delete |
| INDEXES | Created on title, borrower name, due date |
| ROLLUP / CUBE | Available directly in SSMS |
| CAST / CONVERT / GETDATE | Loan date handling |
| ISNULL / COALESCE | NULL-safe publisher queries |

---

## TROUBLESHOOTING

**"A network-related error..."** — SQL Server not running. Start it in Services or SQL Server Configuration Manager.

**"Cannot open database..."** — Database name wrong, or script hasn't been run yet.

**"Login failed..."** — Use Windows Authentication, or check username/password.

**Audit log is empty** — It fills up when you create or delete loans via the Loans form.
