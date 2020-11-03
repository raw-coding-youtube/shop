using System;
using System.Collections.Generic;

namespace RawCoding.Shop.Domain.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public bool DeliveryInformationComplete { get; set; }
        public bool Closed { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string State { get; set; }

        public int ShippingCharge { get; set; }

        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
        public IList<CartProduct> Products { get; set; } = new List<CartProduct>();
    }
}