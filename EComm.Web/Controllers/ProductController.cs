﻿using EComm.Data;
using EComm.Data.Entities;
using EComm.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EComm.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IRepository _repository;

        public ProductController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("product/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _repository.GetProduct(id, includeSuppliers: true);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpGet("product/edit/{id}")]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _repository.GetProduct(id, includeSuppliers: true);
            var suppliers = await _repository.GetAllSuppliers();

            var pvm = new ProductEditViewModel
            {
                Id = product.Id,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice,
                Package = product.Package,
                IsDiscontinued = product.IsDiscontinued,
                SupplierId = product.SupplierId,
                Supplier = product.Supplier,
                Suppliers = suppliers
            };
            return View(pvm);
        }
        [HttpPost("product/edit/{id}")]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel pvm)
        {
            if (!ModelState.IsValid)
            {
                pvm.Suppliers = await _repository.GetAllSuppliers();
                return View(pvm);
            }

            var product = new Product
            {
                Id = id,
                ProductName = pvm.ProductName,
                UnitPrice = pvm.UnitPrice,
                Package = pvm.Package,
                IsDiscontinued = pvm.IsDiscontinued,
                SupplierId = pvm.SupplierId
            };

            await _repository.SaveProduct(product);
            return RedirectToAction("Details", new { id = id });

        }
        [HttpPost("product/addtocart")]
        public async Task<IActionResult> AddToCart(int id, int quantity)
        {
            var product = await _repository.GetProduct(id);
            var totalCost = quantity * product.UnitPrice;

            string message = $"You added {product.ProductName}" +
                             $"(x {quantity}) to you cart " +
                             $"at a total cost of { totalCost:C} .";

            var cart = ShoppingCart.GetFormSession(HttpContext.Session);
            var lineItem = cart.LineItems.SingleOrDefault(item => item.Product.Id == id);

            if (lineItem != null)
            {
                lineItem.Quantity += quantity;
            } else
            {
                cart.LineItems.Add(new ShoppingCart.LineItem
                {
                    Product = product,
                    Quantity = quantity
                });
            }

            ShoppingCart.StoreInSession(cart, HttpContext.Session);
            return PartialView("_AddedToCart", message);
        }

        [HttpGet("product/cart")]
        public IActionResult Cart()
        {
            var cart = ShoppingCart.GetFormSession(HttpContext.Session);
            var cvm = new CartViewModel() { Cart = cart };
            return View(cvm);
        }

        [HttpPost("product/checkout")]
        public IActionResult Checkout(CartViewModel cvm)
        {
            if (!ModelState.IsValid)
            {
                cvm.Cart = ShoppingCart.GetFormSession(HttpContext.Session);
                return View("Cart", cvm);
            }

            HttpContext.Session.Clear();
            return View("ThankYou");
        }
    }
}
