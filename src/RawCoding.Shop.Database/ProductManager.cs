using Microsoft.EntityFrameworkCore;
using RawCoding.Shop.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RawCoding.Shop.Domain.Interfaces;

namespace RawCoding.Shop.Database
{
    public class ProductManager : IProductManager
    {
        private readonly ApplicationDbContext _ctx;

        public ProductManager(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<int> CreateProduct(Product product)
        {
            _ctx.Products.Add(product);

            return _ctx.SaveChangesAsync();
        }

        public Task<int> DeleteProduct(int id)
        {
            var product = _ctx.Products.FirstOrDefault(x => x.Id == id);
            _ctx.Products.Remove(product);

            return _ctx.SaveChangesAsync();
        }

        public Task<int> UpdateProduct(Product product)
        {
            _ctx.Products.Update(product);
            return _ctx.SaveChangesAsync();
        }

        public Product GetProductBySlug(string slug)
        {
            return _ctx.Products
                .AsNoTracking()
                .Where(x => x.Slug.ToLower() == slug.ToLower() && x.Published)
                .Include(x => x.Stock)
                .Include(x => x.Images)
                .FirstOrDefault();
        }

        public IEnumerable<Product> GetFrontPageProducts()
        {
            return _ctx.Products
                .Where(x => x.Stock.Count > 0 && x.Published)
                .Include(x => x.Stock)
                .Include(x => x.Images)
                .ToList();
        }

        public Task UpdateProductStock(int id, IEnumerable<Stock> stocks)
        {
            var product = _ctx.Products
                .Include(x => x.Stock)
                .FirstOrDefault(x => x.Id == id);

            product.Stock = stocks.ToList();

            return _ctx.SaveChangesAsync();
        }

        public IEnumerable<Product> GetAdminPanelProducts()
        {
            return _ctx.Products.ToList();
        }

        public Product GetAdminPanelProduct(int id)
        {
            return _ctx.Products
                .Include(x => x.Images)
                .FirstOrDefault(x => x.Id == id);
        }
    }
}