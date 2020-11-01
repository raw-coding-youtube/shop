using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RawCoding.Shop.Database;
using RawCoding.Shop.Domain.Enums;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            try
            {
                using var scope = host.Services.CreateScope();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManger = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                if (env.IsDevelopment())
                {
                    if (!context.Products.Any())
                    {
                        var stock1 = new Stock {Description = "Small", Value = 1000, Qty = 100,};
                        var stock2 = new Stock {Description = "Medium", Value = 1000, Qty = 100,};
                        var stock3 = new Stock {Description = "Large", Value = 1000, Qty = 1,};
                        context.Add(new Product
                        {
                            Name = "Test",
                            Description = "Test Product",
                            Series = "Original",
                            Slug = "original-test",
                            StockDescription = "Size",
                            Stock = new List<Stock> {stock1, stock2, stock3},
                            Images = new List<Image>
                            {
                                new Image {Index = 0, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/book.jpg"},
                                new Image {Index = 1, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/book2.jpg"},
                                new Image {Index = 2, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/pen.jpg"},
                                new Image {Index = 3, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/shirt.jpg"},
                            },
                            Published = true,
                        });

                        context.Add(new Product
                        {
                            Name = "Out Of Stock",
                            Description = "Test Out Of Stock Product",
                            Series = "Original",
                            Slug = "original-out-of-stock",
                            StockDescription = "Size",
                            Stock = new List<Stock>
                            {
                                new Stock {Description = "Small", Value = 1000, Qty = 0,},
                                new Stock {Description = "Medium", Value = 1000, Qty = 0,},
                                new Stock {Description = "Large", Value = 1000, Qty = 0,},
                            },
                            Images = new List<Image>
                            {
                                new Image {Index = 0, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/book.jpg"},
                                new Image {Index = 1, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/pen.jpg"},
                                new Image {Index = 2, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/shirt.jpg"},
                            },
                            Published = true,
                        });


                        context.Add(new Product
                        {
                            Name = "Limited",
                            Description = "Test Limited Product",
                            Series = "Original",
                            Slug = "original-limited",
                            StockDescription = "Size",
                            Stock = new List<Stock>
                            {
                                new Stock {Description = "Small", Value = 1000, Qty = 10,},
                                new Stock {Description = "Medium", Value = 1000, Qty = 0,},
                            },
                            Images = new List<Image>
                            {
                                new Image {Index = 0, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/book.jpg"},
                                new Image {Index = 1, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/shirt.jpg"},
                            },
                            Published = true,
                        });

                        context.Add(new Product
                        {
                            Name = "Test2",
                            Description = "Test Product 22",

                            Series = "Original",
                            Slug = "original-test2",
                            Stock = new List<Stock>
                            {
                                new Stock {Value = 2220, Description = "Default", Qty = 100,},
                            },
                            Images = new List<Image>
                            {
                                new Image {Index = 0, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/pen.jpg"},
                                new Image {Index = 1, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/shirt.jpg"},
                            },
                            Published = true,
                        });

                        context.Add(new Product
                        {
                            Name = "Test 33",
                            Description = "Test Product 313",
                            Series = "Original",
                            Slug = "original-test-33",
                            Stock = new List<Stock>
                            {
                                new Stock {Value = 333, Description = "Default", Qty = 100,},
                            },
                            Images = new List<Image>
                            {
                                new Image {Index = 0, Path = "https://aw-test-bucket.eu-central-1.linodeobjects.com/raw-coding-shop/test-images/shirt.jpg"},
                            },
                            Published = true,
                        });
                        context.SaveChanges();

                        context.Add(new Order
                        {
                            Id = "dummy",
                            StripeReference = "dummy",
                            Status = OrderStatus.New,
                            Cart = new Cart
                            {
                                DeliveryInformationComplete = true,
                                Closed = true,

                                Name = nameof(Cart.Name),
                                Email = "info@raw-coding.dev",
                                Phone = nameof(Cart.Phone),

                                Address1 = nameof(Cart.Address1),
                                Address2 = nameof(Cart.Address2),
                                City = nameof(Cart.City),
                                Country = nameof(Cart.Country),
                                PostCode = nameof(Cart.PostCode),
                                State = nameof(Cart.State),

                                Products = new List<CartProduct>
                                {
                                    new CartProduct {StockId = stock1.Id, Qty = 1},
                                    new CartProduct {StockId = stock2.Id, Qty = 3},
                                    new CartProduct {StockId = stock3.Id, Qty = 6},
                                },
                            },
                        });
                        context.SaveChanges();
                    }

                    var managerUser = new IdentityUser
                    {
                        UserName = "Manager",
                        Email = "test@test.com",
                    };

                    userManger.CreateAsync(managerUser, "password").GetAwaiter().GetResult();
                    var managerClaim = new Claim(ShopConstants.Claims.Role, ShopConstants.Roles.ShopManager);
                    userManger.AddClaimAsync(managerUser, managerClaim).GetAwaiter().GetResult();
                }

                if (env.IsDevelopment() || !context.Users.Any())
                {
                    var adminUser = new IdentityUser
                    {
                        UserName = "Admin",
                    };

                    userManger.CreateAsync(adminUser, config["AdminPassword"]).GetAwaiter().GetResult();
                    var adminClaim = new Claim(ShopConstants.Claims.Role, ShopConstants.Roles.Admin);
                    userManger.AddClaimAsync(adminUser, adminClaim).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}