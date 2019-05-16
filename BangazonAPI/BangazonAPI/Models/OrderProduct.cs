using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class OrderProduct
    {
        public int id { get; set; }
        public List<Product> product = new List<Product>();
        public Product getProductById(int id)
        {
            return product[id];
        }
        public List<Order> order = new List<Order>();
        public Order getOrderById(int id)
        {
            return order[id];
        }
    }
}
