using System.Linq;
using RawCoding.Shop.Domain.Extensions;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Application.Projections
{
    public class OrderProjections
    {
        public static object Project(Order order) => new
        {
            order.Id,
            order.Status,

            order.Cart.Name,
            order.Cart.Email,
            order.Cart.Phone,

            order.Cart.Address1,
            order.Cart.Address2,
            order.Cart.City,
            order.Cart.Country,
            order.Cart.PostCode,
            order.Cart.State,

            Products = order.Cart.Products.Select(x => new
            {
                x.Stock.Product.StockDescription,
                StockText = x.Stock.Description,

                x.Qty,
                Total = (x.Qty * x.Stock.Value).ToMoney(),

                x.Stock.Product.Name,
                x.Stock.Product.Series,
                x.Stock.Product.Description,
                DefaultImage = x.Stock.Product.Images[0].Path,
            }),
            Total = order.Cart.Total().ToMoney(),
        };

    }
}