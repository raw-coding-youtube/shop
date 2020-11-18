using System;
using System.Linq;
using System.Threading.Tasks;
using RawCoding.Shop.Domain.Interfaces;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Application.CartActions
{
    [Service]
    public class UpdateCart
    {
        private readonly ICartManager _cartManager;
        private readonly IStockManager _stockManager;

        public UpdateCart(
            ICartManager cartManager,
            IStockManager stockManager)
        {
            _cartManager = cartManager;
            _stockManager = stockManager;
        }

        public class Form
        {
            public string UserId { get; set; }
            public int StockId { get; set; }
            public int Qty { get; set; }
        }

        public async Task<BaseResponse> Do(Form request)
        {
            var stock = _stockManager.GetStock(request.StockId);
            if (stock == null)
            {
                return new BaseResponse("Product not found", false);
            }

            if (stock.Qty < request.Qty)
            {
                return new BaseResponse("Not Enough Stock", false);
            }

            var cart = await _cartManager.GetCartByUserId(request.UserId);

            var product = cart.Products.FirstOrDefault(x => x.StockId == request.StockId);
            if (product == null)
            {
                cart.Products.Add(new CartProduct
                {
                    StockId = request.StockId,
                    Qty = request.Qty,
                });
            }
            else
            {
                product.Qty += request.Qty;
            }

            stock.Qty -= request.Qty;

            await _cartManager.UpdateCart(cart);

            return new BaseResponse("Product Added");
        }

        public async Task Do(string cartId, Action<Domain.Models.Cart> mutation)
        {
            var cart = await _cartManager.GetCartByUserId(cartId);

            mutation(cart);

            await _cartManager.UpdateCart(cart);
        }
    }
}