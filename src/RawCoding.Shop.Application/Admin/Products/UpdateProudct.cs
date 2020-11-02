using System.Threading.Tasks;
using RawCoding.Shop.Domain.Interfaces;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Application.Admin.Products
{
    [Service]
    public class UpdateProduct
    {
        private readonly IProductManager _productManager;

        public UpdateProduct(IProductManager productManager)
        {
            _productManager = productManager;
        }

        public Task Update(Product product)
        {
            return _productManager.UpdateProduct(product);
        }

        public Task Publish(int id)
        {
            var product = _productManager.GetAdminPanelProduct(id);
            product.Published = true;
            return _productManager.UpdateProduct(product);
        }

        public Task Archive(int id)
        {
            var product = _productManager.GetAdminPanelProduct(id);
            product.Published = false;
            return _productManager.UpdateProduct(product);
        }
    }
}