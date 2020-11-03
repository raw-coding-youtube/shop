using System;
using System.Linq;
using System.Threading.Tasks;
using RawCoding.Shop.Domain.Extensions;
using RawCoding.Shop.Domain.Interfaces;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Application.CartActions
{
    [Service]
    public class GetCart
    {
        private readonly ICartManager _cartManager;

        public GetCart(ICartManager cartManager)
        {
            _cartManager = cartManager;
        }

        public async Task<object> GetCartForComponent(string userId)
        {
            var cart = await ByUserId(userId);

            return new
            {
                Items = cart.Products.Select(x => new
                {
                    x.StockId,
                    x.Qty,
                    ProductName = x.Stock.Product.Name,
                    Image = x.Stock.Product.Images.FirstOrDefault()?.Path,
                    StockDescription = x.Stock.Description,
                    Value = x.Stock.Value.ToMoney(),
                    TotalValue = (x.Qty * x.Stock.Value).ToMoney(),
                }),
                ShippingCharge = cart.ShippingCharge.ToMoney(),
                Total = cart.Total().ToMoney(),
            };
        }

        public async Task<Cart> ByUserId(string userId)
        {
            var cart = await _cartManager.GetCartByUserId(userId) ?? await _cartManager.SaveCart(new Cart
            {
                UserId = userId,
                // todo: come up with a better way to generate shipping charge
                ShippingCharge = 300,
            });

            return cart;
        }

        public Task<int> Id(string userId)
        {
            return _cartManager.GetCartId(userId);
        }
    }
}