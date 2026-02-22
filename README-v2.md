# Budget Planner (v2)

A personal budget planning application built with ASP.NET Core MVC, Entity Framework Core, and SQLite. Add your income and expenses, and the app tells you **what to pay or allocate each month** and **your balance in the bank**.

## Requirements / How It Works

### Goal

1. Add your **income** and **expenses**
2. The app shows **what expenses to handle that month**
3. The app shows **your balance** in the bank account

---

## Accounts (per user)

Users configure these account types to reflect where money flows:

| Account Type | Purpose |
|--------------|---------|
| **Salary** | Where income lands |
| **Current** | Day-to-day spending and bill payments |
| **Savings** | Savings and chit payments |

---

## Payment Strategy Engine (exactly two strategies)

| Strategy | Meaning | When |
|----------|---------|------|
| **Pay Now** | Pay the full amount in the current month | Obligation is due this month |
| **Accumulate in Bank** | Allocate monthly; keep in bank until due (sinking fund) | Obligation is not due this month |

There are no other modes. Keep it to these two only.

---

## Obligations & Money Flow

| Obligation | Strategy | Money flow |
|------------|----------|------------|
| **Monthly bills** | Pay Now | Salary → Current → pay |
| **One-time expenses** | Pay Now | Salary → Current → pay |
| **Yearly bills** | Accumulate in Bank | Allocate monthly; stay in bank | Salary → Savings → pay 
| **Chit** | Accumulate in Bank | Allocate monthly; stay in bank | Salary → Savings → pay 
| **Recurring N‑month bills** | Accumulate in Bank | Allocate monthly; stay in bank | Salary → Savings → pay 
| **Loan EMI** | Pay Now | Salary → Current → pay |

### Summary

| From | To | Use case |
|------|----|----------|
| Salary | Current | Monthly bills, one-time **loan EMI** |
| Salary | Savings | Chit / recurring installments |
| — | Bank (same account) | Chit / recurring (Accumulate in Bank) |

---

## Flow Diagram

```
                    ┌─────────────────┐
                    │  Salary Account │
                    └────────┬────────┘
                             │
              ┌──────────────┼────────────────────────────────────
              │                                                  │          
              ▼                                                  ▼          
       ┌──────────┐                                       ┌──────────┐  
       │ Current  │                                       │ Savings  │  
       └────┬─────┘                                       └────┬─────┘   
            │                                                  │
            │ Pay Now                                          │      Accumulate in Bank
            │                                                  │      (allocate monthly,
            ▼                                                          stay in bank)
    ┌───────────────────────────────────────────────┐            
    │ • Monthly bills                               │
    │ • One-time expenses                           │
    │ • Loan EMI                                    │
    └───────────────────────────────────────────────┘
                                                               │
                                                               ▼
                                        ┌───────────────────────────────────────────────┐
                                        │ • Chit installment (when due)                 │
                                        │ • Recurring N‑month installment (when due)    │
                                        └───────────────────────────────────────────────┘
```

---

## Features

- **Login/Register** – ASP.NET Core Identity for user authentication
- **Dashboard** – Monthly budget overview: income, expenses, and net balance
- **Accounts** – Configure Salary, Current, and Savings accounts
- **Income** – Add, edit, and delete income entries (One Time,Recursive)
- **Expenses** – Multiple types: 
(One Time)
  - **Monthly Bill** – Fixed amount every month (rent, utilities)
  - **Loan** – EMI-based loans with principal, tenure, lender
  - **Yearly Bill** – Once per year
(Recursive)
  - **Recurring (every N months)** – Chits, quarterly, etc.

- **Chit/Loan Accounts** – Track allocations and payments for chits and recurring N‑month expenses
- **Monthly Audit** – Grouped by Pay Now vs Accumulate in Bank

---

## Tech Stack

- ASP.NET Core 10 MVC
- Entity Framework Core 10 with SQLite (lightweight, file-based database)
- ASP.NET Core Identity for authentication

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)

```

### First Use

1. Register a new account
2. Log in
3. Add your Accounts (Salary, Current, Savings)
4. Add your income and expenses
5. Use the Dashboard and Monthly Breakdown to plan your budget

---

## Expense Types

| Type | Description | Example |
|------|-------------|---------|
| Monthly Bill | Fixed amount every month | Rent, utilities, subscription |
| Yearly Bill | Once per year | Insurance, membership |
| Recurring (every N months) | Chits, quarterly, etc. | 4‑month chit, 6‑month fund |
| Loan | EMI-based loan | Home loan, car loan |
