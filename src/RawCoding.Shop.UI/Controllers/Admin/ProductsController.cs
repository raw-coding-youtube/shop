using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RawCoding.S3;
using RawCoding.Shop.Application.Admin.Products;
using RawCoding.Shop.Application.Admin.Stocks;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.UI.Controllers.Admin
{
    public class ProductsController : AdminBaseController
    {
        private readonly IWebHostEnvironment _env;

        public ProductsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IEnumerable<object> GetProducts(
            [FromServices] GetProducts getProducts) =>
            getProducts.Do();

        [HttpGet("{id}")]
        public object GetProduct(int id, [FromServices] GetProduct getProduct) =>
            getProduct.Do(id);

        [HttpGet("{id}/stocks")]
        public IActionResult GetProductStock(int id, [FromServices] GetStock getStock) =>
            Ok(getStock.ForProduct(id));


        [HttpPut("{id}/stocks")]
        public Task UpdateStock(int id,
            [FromBody] IEnumerable<UpdateStock.StockForm> stocks,
            [FromServices] UpdateStock updateStock) =>
            updateStock.ForProduct(id, stocks);


        [HttpPut("{id}/publish")]
        public Task PublishProduct(int id, [FromServices] UpdateProduct updateProduct) =>
            updateProduct.Publish(id);

        [HttpPut("{id}/archive")]
        public Task ArchiveProduct(int id, [FromServices] UpdateProduct updateProduct) =>
            updateProduct.Archive(id);

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<object> CreateProduct(
            [FromForm] ProductForm form,
            [FromServices] CreateProduct createProduct,
            [FromServices] S3Client s3Client)
        {
            var product = new Product
            {
                Name = form.Name,
                Slug = form.Name.Replace(" ", "-").ToLower(),
                Description = form.Description,
                Series = form.Series,
                StockDescription = form.StockDescription
            };

            if (form.Images != null)
            {
                var results = await Task.WhenAll(UploadFiles(s3Client, form.Images));

                product.Images.AddRange(results.Select((path, index) => new Image
                {
                    Index = index,
                    Path = path,
                }));
            }

            return await createProduct.Do(product);
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateProduct(
            [FromForm] ProductForm form,
            [FromServices] GetProduct getProduct,
            [FromServices] UpdateProduct updateProduct,
            [FromServices] S3Client s3Client)
        {
            var product = getProduct.Do(form.Id);
            product.Description = form.Description;
            product.Series = form.Series;
            product.StockDescription = form.StockDescription;

            if (form.Images != null && form.Images.Any())
            {
                product.Images = new List<Image>();
                var results = await Task.WhenAll(UploadFiles(s3Client, form.Images));

                product.Images.AddRange(results.Select((path, index) => new Image
                {
                    Index = index,
                    Path = path,
                }));
            }

            await updateProduct.Update(product);
            return Ok();
        }

        public class ProductForm
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Series { get; set; }
            public string StockDescription { get; set; }
            public IEnumerable<IFormFile> Images { get; set; }
        }

        private static IEnumerable<Task<string>> UploadFiles(S3Client s3Client, IEnumerable<IFormFile> files)
        {
            var index = 0;
            foreach (var image in files)
            {
                var fileName = $"{DateTime.Now.Ticks}_{index++}{Path.GetExtension(image.FileName)}";
                yield return s3Client.SavePublicFile($"images/{fileName}", image.OpenReadStream());
            }
        }
    }
}