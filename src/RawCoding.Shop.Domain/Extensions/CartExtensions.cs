using System.Linq;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Domain.Extensions
{
    public static class CartExtensions
    {
        public static int Total(this Cart cart) => cart.ShippingCharge + cart.Products.Sum(x => x.Qty + x.Stock.Value);
    }
}