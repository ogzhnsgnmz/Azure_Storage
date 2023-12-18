using AzureStorageLibrary;
using AzureStorageLibrary.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using System.Net;

namespace WebApp.Controllers
{
    public class TableStoragesController : Controller
    {
        private readonly INoSqlStorage<Product> _noSqlStorage;

        public TableStoragesController(INoSqlStorage<Product> noSqlStorage)
        {
            _noSqlStorage = noSqlStorage;
        }

        public IActionResult Index()
        {
            ViewBag.products = _noSqlStorage.All().ToList();
            ViewBag.isUpdate = false;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            product.RowKey = Guid.NewGuid().ToString();
            product.PartitionKey = "Kalemler";

            await _noSqlStorage.Add(product);

            return RedirectToAction("index");
        }

        public async Task<IActionResult> Update(string rowKey, string partitionKey)
        {
            var product = await _noSqlStorage.Get(rowKey, partitionKey);

            ViewBag.products = _noSqlStorage.All().ToList();
            ViewBag.isUpdate = true;

            return View("index", product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Product product)
        {
            product.ETag = "*";
            ViewBag.IsUpdate = true;
            await _noSqlStorage.Update(product);

            /*
            try
            {
                await _noSqlStorage.Update(product);
            }
            catch(StorageException ex)
            {
                ex.RequestInformation.HttpStatusCode = (int)HttpStatusCode.PreconditionFailed;
                throw;
            }
            */

            return RedirectToAction("index");
        }

        public async Task<IActionResult> Delete(string rowKey, string partitionKey)
        {
            await _noSqlStorage.Delete(rowKey, partitionKey);
            return RedirectToAction("index");
        }

        [HttpGet]
        public IActionResult Query(int price)
        {
            ViewBag.isUpdate = false;
            ViewBag.products = _noSqlStorage.Query(x=>x.Price>price).ToList();
            return View("index");
        }
    }
}
