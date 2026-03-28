using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TH3.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        [Required(ErrorMessage = "Full Name is required")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Shipping Address is required")]
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public IdentityUser User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
