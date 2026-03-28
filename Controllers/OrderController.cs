using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TH3.Models;

namespace TH3.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public OrderController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List all orders for Admin/Employee
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // View details of a specific order
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Ensure the user is either Admin/Employee OR the owner of the order
            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee") && order.UserId != currentUser?.Id)
            {
                return Forbid();
            }

            return View(order);
        }

        // View orders for the current customer
        public async Task<IActionResult> MyOrders()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == currentUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
