﻿using AutomotiveShop.model;
using AutomotiveShop.model.Infrastructure;
using AutomotiveShop.service.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AutomotiveShop.service.Service
{
    public class OrderService
    {
        private AutomotiveShopDbContext _dbContext = new AutomotiveShopDbContext();
        private SessionManager _sessionManager = new SessionManager();
        private ProductService _productService = new ProductService();

        public Guid Create(DeliveryAddress deliveryAddress, ApplicationUser user)
        {
            List<Product> products = new List<Product>();
            foreach (ItemInCartViewModel item in GetCart())
            {
                if (item.Quantity <= item.Product.ItemsAvailable)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        products.Add(item.Product);
                    }
                }
                else
                {
                    throw new Exception("Not enough products in stock");
                }
            }
            Order newOrder = new Order();
            newOrder.OrderId = Guid.NewGuid();
            newOrder.DateOfPurchase = DateTime.Now;
            newOrder.OrderState = OrderState.New;
            newOrder.DeliveryAddress = _dbContext.DeliveryAddresses.FirstOrDefault(da => da.DeliveryAddressId != null);
            newOrder.UserId = user.Id;
            _dbContext.Orders.Add(newOrder);
            foreach (Product product in products)
            {
                Product tempProd = _dbContext.Products.FirstOrDefault(p => p.ProductId == product.ProductId);
                if (tempProd.ItemsAvailable > 0)
                {
                    ProductCopy newCopy = new ProductCopy();
                    newCopy.ProductCopyId = Guid.NewGuid();
                    newCopy.ProductId = product.ProductId;
                    newCopy.Price = product.Price;
                    newCopy.OrderId = newOrder.OrderId;
                    _dbContext.ProductsCopies.Add(newCopy);
                    tempProd.ItemsAvailable--;
                    _dbContext.Entry(tempProd).Property(p => p.ItemsAvailable).IsModified = true;
                    _dbContext.SaveChanges();
                }
            }
            _dbContext.SaveChanges();
            EmptyCart();
            return newOrder.OrderId;

        }

        public List<ItemInCartViewModel> GetCart()
        {
            List<ItemInCartViewModel> cart;

            if (_sessionManager.Get<List<ItemInCartViewModel>>(Consts.CartSessionKey) == null)
            {
                cart = new List<ItemInCartViewModel>();
            }
            else
            {
                cart = _sessionManager.Get<List<ItemInCartViewModel>>(Consts.CartSessionKey) as List<ItemInCartViewModel>;
            }

            return cart;
        }

        public List<DeliveryAddress> GetDeliveryAddressesByUser(ApplicationUser user)
        {
            return _dbContext.DeliveryAddresses.Where(o => o.UserId == user.Id).ToList();
        }

        public void AddToCart(Guid productId)
        {
            List<ItemInCartViewModel> cart = GetCart();
            ItemInCartViewModel item = cart.Find(p => p.Product.ProductId == productId);
            if (item != null && item.Quantity >= _productService.GetProductById(productId).ItemsAvailable)
            {
                throw new Exception("Not enough items in stock");
            }
            else if (item != null)
            {
                item.Quantity++;
                item.Value += _productService.GetProductById(item.Product.ProductId).Price;
            }
            else
            {
                var productToAdd = _productService.GetProductById(productId);

                if (productToAdd != null)
                {
                    ItemInCartViewModel newItem = new ItemInCartViewModel()
                    {
                        Product = productToAdd,
                        Quantity = 1,
                        Value = productToAdd.Price
                    };
                    cart.Add(newItem);
                }
            }

            _sessionManager.Set(Consts.CartSessionKey, cart);
        }

        public List<Order> GetOrdersByUser(ApplicationUser user)
        {
            return _dbContext.Orders.Where(o => o.UserId == user.Id).ToList();
        }

        public int RemoveFromCart(Guid productId)
        {
            var cart = GetCart();
            var item = cart.Find(p => p.Product.ProductId == productId);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    return item.Quantity;
                }
                else
                {
                    cart.Remove(item);
                }
            }

            return 0;
        }

        public double GetCartValue()
        {
            var cart = GetCart();
            return cart.Sum(i => i.Value);
        }

        public int GetNumberOfItemsInCart()
        {
            var cart = GetCart();
            int quantity = cart.Sum(c => c.Quantity);
            return quantity;
        }

        public void EmptyCart()
        {
            _sessionManager.Set<List<ItemInCartViewModel>>(Consts.CartSessionKey, null);
        }
    }
}