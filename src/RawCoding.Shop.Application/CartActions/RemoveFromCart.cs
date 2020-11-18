using System.Threading.Tasks;
using RawCoding.Shop.Domain.Interfaces;

namespace RawCoding.Shop.Application.CartActions
{
    [Service]
    public class RemoveFromCart
    {
        private readonly IStockManager _stockManager;
        private readonly ICartManager _cartManager;

        public RemoveFromCart(
            IStockManager stockManager,
            ICartManager cartManager)
        {
            _stockManager = stockManager;
            _cartManager = cartManager;
        }

        public class Form
        {
            public string UserId { get; set; }
            public int StockId { get; set; }
        }

        public async Task<BaseResponse> Do(Form request)
        {
            var removedStock = await _cartManager.RemoveStock(request.StockId, request.UserId);
            if (removedStock < 0)
            {
                return new BaseResponse("Product not found", false);
            }

            var stock = _stockManager.GetStock(request.StockId);

            stock.Qty += removedStock;

            await _stockManager.UpdateStockRange(new[] {stock});

            return new BaseResponse("Removed from cart");
        }
    }
}