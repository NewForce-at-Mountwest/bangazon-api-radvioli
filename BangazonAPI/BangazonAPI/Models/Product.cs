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
        public List<ProductType> productType = new List<ProductType>();
        public ProductType getPTypeById(int id)
        {
            return productType[id];
        }
        public List<Customer> customer = new List<Customer>();
        public Customer getCustomerById(int id)
        {
            return customer[id];
        }
    }
}
