using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TH3.Models;

namespace TH3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Gather stats for the dashboard
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalSalesList = await _context.Orders.Select(o => o.TotalPrice).ToListAsync();
            var totalSales = totalSalesList.Sum();
            var totalCategories = await _context.Categories.CountAsync();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalCategories = totalCategories;

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}
