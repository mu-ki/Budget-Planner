# Budget Planner

A personal budget planning application built with ASP.NET Core MVC, Entity Framework Core, and SQLite. Add your income and expenses, and the app tells you **what to pay or allocate each month** and **your balance in the bank**.

## Requirements / How It Works

### Goal

1. Add your **income** and **expenses**
2. The app shows **what expenses to handle that month**
3. The app shows **your balance** in the bank account

### Workflow

- **Monthly bills** (rent, utilities, insurance, etc.)  
  → Move to savings account and pay from there.

- **Chit funds** (loans, 4‑month/6‑month recurring)  
  → Keep in bank account and accumulate. Pay only when the chit term comes.

### What the App Shows

| Section | Meaning |
|--------|---------|
| **Move to Savings & Pay** | Monthly bills and yearly bills when due – move to savings and pay |
| **Chit Due – Pay from Bank Account** | Chits due this month – pay from bank account |
| **Chit – Keep in Bank Account** | Chits not due – keep in bank account, accumulate, pay when term comes |
| **Balance in Bank** | What you have left after all allocations (salary − total allocations) |

### Summary

- Monthly bills → move to savings and pay  
- Chits stay in bank account, accumulate, pay when chit term comes  
- Balance = Salary − (move to savings + chit payment + chit allocation)

---

## Features

- **Login/Register** – ASP.NET Core Identity for user authentication
- **Dashboard** – Monthly budget overview: income, expenses, and net balance
- **Income** – Add, edit, and delete income entries per month
- **Expenses** – Three options:
  - **Monthly Bill** – Fixed amount every month (rent, utilities)
  - **Yearly Bill** – Once per year
  - **Recurring (every N months)** – Chits, quarterly, etc. (e.g. every 3, 4, 6 months)
- **Monthly Breakdown** – Grouped by: Move to savings & pay / Chit due / Chit keep in bank account

## Tech Stack

- ASP.NET Core 10 MVC
- Entity Framework Core 10 with SQLite (lightweight, file-based database)
- ASP.NET Core Identity for authentication

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)

### Run the Application

```bash
cd BudgetPlanner.Web
dotnet run
# Or with auto-restart on changes:
dotnet watch run
```

Then open `https://localhost:5001` or `http://localhost:5000` in your browser.

Migrations are applied automatically on startup. The SQLite database file `app.db` is created in the project folder.

### First Use

1. Register a new account
2. Log in
3. Add your income and expenses
4. Use the Dashboard and Monthly Breakdown to plan your budget

## Project Structure

```
BudgetPlanner.Web/
├── Controllers/
│   └── BudgetController.cs   # Dashboard, Income, Expenses, Monthly Breakdown
├── Models/
│   ├── Income.cs
│   ├── Expense.cs
│   ├── ExpenseType.cs
│   └── MonthlyBreakdownItem.cs
├── Services/
│   └── BudgetService.cs      # Income/expense calculations, monthly breakdown logic
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
└── Views/Budget/
```

## Expense Types

| Type | Description | Example |
|------|-------------|---------|
| Monthly Bill | Fixed amount every month – move to savings & pay | Rent, utilities, subscription |
| Yearly Bill | Once per year – move to savings & pay when due | Insurance, membership |
| Recurring (every N months) | Chits/funds – keep in bank account, accumulate, pay when term comes | 4‑month chit, 6‑month loan |
