using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RawCoding.Shop.Domain.Interfaces;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Database
{
    public class CartManager : ICartManager
    {
        private readonly ApplicationDbContext _ctx;

        public CartManager(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Cart> SaveCart(Cart cart)
        {
            _ctx.Add(cart);
            await _ctx.SaveChangesAsync();
            return cart;
        }

        public Task<int> UpdateCart(Cart cart)
        {
            _ctx.Carts.Update(cart);
            return _ctx.SaveChangesAsync();
        }

        public async Task<int> RemoveStock(int stockId, string userId)
        {
            var cart = _ctx.Carts.AsNoTracking().FirstOrDefault(x => x.UserId == userId);

            if (cart == null)
            {
                return -1;
            }

            var stock = _ctx.CartProducts
                .FirstOrDefault(x => x.StockId == stockId && x.CartId == cart.Id);

            if (stock == null)
            {
                return -1;
            }

            _ctx.CartProducts.Remove(stock);
            await _ctx.SaveChangesAsync();

            return stock.Qty;
        }

        public async Task<int> GetCartId(string userId)
        {
            var cart = await _ctx.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.Closed);

            return cart?.Id ?? 0;
        }

        public Task<Cart> GetCartById(int cartId)
        {
            return GetCart(x => x.Id == cartId);
        }

        public Task<Cart> GetCartByUserId(string userId)
        {
            return GetCart(x => x.UserId == userId && !x.Closed);
        }

        private Task<Cart> GetCart(Expression<Func<Cart, bool>> condition)
        {
            return _ctx.Carts
                .Where(condition)
                .Include(x => x.Products)
                .ThenInclude(x => x.Stock)
                .ThenInclude(x => x.Product)
                .ThenInclude(x => x.Images)
                .FirstOrDefaultAsync();
        }
    }
}