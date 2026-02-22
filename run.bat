@echo off
echo Stopping Budget Planner...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5000 "') do taskkill /F /PID %%a 2>nul
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5001 "') do taskkill /F /PID %%a 2>nul
timeout /t 2 /nobreak >nul

echo Building...
dotnet build BudgetPlanner.sln
if %errorlevel% neq 0 (
    echo Build failed.
    pause
    exit /b 1
)

echo Running...
dotnet run --project BudgetPlanner.Web\BudgetPlanner.Web.csproj
