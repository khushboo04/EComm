using EComm.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EComm.Web.ViewComponents
{
    public class ProductList: ViewComponent
    {
        private readonly IRepository _repository;

        public ProductList(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IViewComponentResult> InvokeAsync ()
        {
            var products = await _repository.GetAllProducts(includeSuppliers: true);
            return View(products);
        }
    }
}
