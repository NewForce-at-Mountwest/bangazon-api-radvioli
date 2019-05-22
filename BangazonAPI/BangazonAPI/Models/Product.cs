using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Product
    {
        public int id { get; set; }
        public int price { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public int ProductTypeId { get; set; }
        public int CustomerId { get; set; }

        public ProductType productsType { get; set; } = new ProductType();
    
        public Customer customerPurchasing { get; set; } = new Customer();
       
    }
}
