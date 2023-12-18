using AzureStorageLibrary.Model;
using AzureStorageLibrary;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class BlobsController : Controller
{
    private readonly IBlobStorage _blobStorage;

    public BlobsController(IBlobStorage blobStorage)
    {
        _blobStorage = blobStorage;
    }

    public async Task<IActionResult> Index()
    {
        var names = _blobStorage.GetNames(ContainerName.pictures);
        string blobUrl = $"{_blobStorage.BlobUrl}/{ContainerName.pictures.ToString()}";
        ViewBag.blobs = names.Select(x => new FileBlob { Name = x, Url = $"{blobUrl}/{x}" }).ToList();

        ViewBag.logs = await _blobStorage.GetLogAsync("controller.txt");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile picture)
    {
        await _blobStorage.SetLogAsync("Upload methoduna giriş yapıldı", "controller.txt");

        var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(picture.FileName);

        await _blobStorage.UploadAsync(picture.OpenReadStream(), newFileName, ContainerName.pictures);

        await _blobStorage.SetLogAsync("Upload methodundan çıkış yapıldı", "controller.txt");
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Download(string fileName)
    {
        var stream = await _blobStorage.DownloadAsync(fileName, ContainerName.pictures);

        return File(stream, "application/octet-stream", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string fileName)
    {
        await _blobStorage.DeleteAsync(fileName, ContainerName.pictures);
        return RedirectToAction("Index");
    }
}
