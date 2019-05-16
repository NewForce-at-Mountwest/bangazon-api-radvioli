using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class PaymentType
    {
        public int id { get; set; }
        public string name { get; set; }
        public int accountNumber { get; set; }
        public List<Customer> customer = new List<Customer>();
        public Customer getCustomerById(int id)
        {
            return customer[id];
        }
    }
}
