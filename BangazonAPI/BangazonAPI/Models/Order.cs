using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int id { get; set; }
        public List<Customer> customer = new List<Customer>();
        public Customer getCustomerById(int id)
        {
            return customer[id];
        }
        public List<PaymentType> paymentType = new List<PaymentType>();
        public PaymentType getPTypeById(int id)
        {
            return paymentType[id];
        }
    }
}
