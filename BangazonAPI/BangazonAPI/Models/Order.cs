using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int id { get; set; }
        public int CustomerId { get; set; }
        public int PaymentTypeId { get; set; }
        public Customer ordersCustomer { get; set; } 

        public PaymentType ordersPayment { get; set; } = new PaymentType();
        public List<Product> Products { get; set; } = new List<Product>();
        
    }
}
