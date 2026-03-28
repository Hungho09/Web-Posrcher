using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TH3.Extensions;
using TH3.Models;
using TH3.Repositories;

namespace TH3.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartController(IProductRepository productRepository, ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _productRepository = productRepository;
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem == null)
            {
                cart.Add(new CartItem { ProductId = productId, Name = product.Name, Price = product.Price, Quantity = quantity, ImageUrl = product.ImageUrl });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            order.UserId = user.Id;
            order.OrderDate = DateTime.Now;
            order.TotalPrice = cart.Sum(c => c.Price * c.Quantity);
            order.OrderDetails = cart.Select(c => new OrderDetail
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                Price = c.Price
            }).ToList();

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            return View(orderId);
        }
    }
}
