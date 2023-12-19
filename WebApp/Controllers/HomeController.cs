using AzureStorageLibrary.Model;
using AzureStorageLibrary.Services;
using AzureStorageLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public string UserId { get; set; } = "12345";
        public string City { get; set; } = "Bursa";

        private readonly INoSqlStorage<UserPicture> _noSqlStorage;
        private readonly IBlobStorage _blobStorage;

        public HomeController(INoSqlStorage<UserPicture> noSqlStorage, IBlobStorage blobStorage)
        {
            _noSqlStorage = noSqlStorage;
            _blobStorage = blobStorage;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserId = UserId;
            ViewBag.City = City;

            List<FileBlob> fileBlobs = new List<FileBlob>();

            var user = await _noSqlStorage.Get(UserId, City);

            if (user != null)
            {
                user.Paths.ForEach(x => {
                    fileBlobs.Add(new FileBlob { Name = x, Url = $"{_blobStorage.BlobUrl}/{ContainerName.pictures}/{x}" });
                });
            }

            ViewBag.fileBlobs = fileBlobs;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IEnumerable<IFormFile> pictures)
        {
            List<string> picturesList = new List<string>();

            foreach (var item in pictures)
            {
                var newPictureName = $"{Guid.NewGuid()}{Path.GetExtension(item.FileName)}";

                await _blobStorage.UploadAsync(item.OpenReadStream(), newPictureName, ContainerName.pictures);

                picturesList.Add(newPictureName);
            }

            var isUser = await _noSqlStorage.Get(UserId, City);

            if (isUser != null)
            {
                picturesList.AddRange(isUser.Paths);
                isUser.Paths = picturesList;
            }
            else
            {
                isUser = new UserPicture();

                isUser.RowKey = UserId;
                isUser.PartitionKey = City;
                isUser.Paths = picturesList;
            }

            await _noSqlStorage.Add(isUser);

            return RedirectToAction("index");
        }

        [HttpPost]
        public async Task<IActionResult> AddWatermark(PictureWatermarkQueue pictureWatermarkQueue)

        {
            var jsonString = JsonConvert.SerializeObject(pictureWatermarkQueue);

            string jsonStringBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

            AzureQueue azQueue = new AzureQueue("watermarkqueue");

            await azQueue.SendMessageAsync(jsonStringBase64);

            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> ShowWatermark()
        {
            List<FileBlob> watermarkBlobs = new List<FileBlob>();

            UserPicture userPicture = await _noSqlStorage.Get(UserId, City);

            ViewBag.pictures = userPicture.WatermarkPaths;

            userPicture.WatermarkPaths.ForEach(x => {
                watermarkBlobs.Add(new FileBlob { Name = x, Url = $"{_blobStorage.BlobUrl}/{ContainerName.watermarkpictures}/{x}" });
            });

            ViewBag.watermarkBlobs = watermarkBlobs;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
