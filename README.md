# Library Management System

A desktop application built in C# (Windows Forms, .NET 8) with a Microsoft SQL Server 
backend, developed as a Database Management Systems lab project at Bahria University.

Live preview: librarymanagementsystemdbms.netlify.app

## Features
- 📖 Books, Borrowers, Branches & Loans — full CRUD with search
- 🔴 Overdue loan highlighting with date validation
- 📊 Reports: loans per branch, books by author, active loans, heavy borrowers
- 🪵 Audit Log — auto-populated via SQL AFTER triggers (INSERT/DELETE)
- ⚙️ Runtime connection string configuration (no recompile needed)

## Tech Stack
- **Language:** C# (.NET 8)
- **UI:** Windows Forms (WinForms)
- **Database:** Microsoft SQL Server Express
- **IDE:** Visual Studio 2022

## SQL Concepts Covered
DDL/DML · JOINs · Subqueries · Views · Indexes · Stored Procedures · 
Triggers · GROUP BY/HAVING · ROLLUP/CUBE · Set Operations (UNION, INTERSECT, EXCEPT)

## Getting Started
1. Restore/run the SQL script to create `db_LibraryManagement`
2. Open the solution in Visual Studio 2022 (requires .NET Desktop Development workload)
3. Launch the app and configure the connection string via **Settings → Connection Settings**
