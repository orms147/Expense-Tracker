using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Spreadsheet;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DasboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DasboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            // View Transactions from this Week
            //DateTime startDay = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
            //DateTime endDay = DateTime.Now;

            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= startDate && y.Date <= endDate)
                .ToListAsync();

            // Total Income $
            int totalIncome = SelectedTransactions
                .Where(t => t.Category.Type == "Income")
                .Sum(a => a.ParsedAmount);
            ViewBag.TotalIncome = totalIncome.ToString("C0");

            // Total Expense $
            int totalExpense = SelectedTransactions
                .Where(t => t.Category.Type == "Expense")
                .Sum(a => a.ParsedAmount);
            ViewBag.TotalExpense = totalExpense.ToString("C0");

            // Balance 
            decimal Balance = totalIncome - totalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            ViewBag.Balance = String.Format(culture, "{0:C0}", Balance);

            // Create List for chart
            ViewBag.DoughnutChart = SelectedTransactions
                .Where(e => e.Category.Type == "Expense")
                .GroupBy(i => i.Category.CategoryId)
                .Select(g => new
                {
                    categoryTitleWithIcon = g.First().Category.Icon + " " + g.First().Category.Title,
                    parsedAmount = g.Sum(i=>i.ParsedAmount),
                    fomattedAmount = g.Sum(j => j.ParsedAmount).ToString("C0"),
                })
                .OrderByDescending(x=>x.parsedAmount)
                .ToList();

            //Spline Chart
            //Income
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.ParsedAmount)
                })
                .ToList();

            //Expense
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.ParsedAmount)
                })
                .ToList();

            //Combine Income & Expense
            string[] last7Days = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();


            return View();
        }
        
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}
