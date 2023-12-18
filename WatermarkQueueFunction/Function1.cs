using System.Drawing;
using System.Drawing.Imaging;
using AzureStorageLibrary;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WatermarkQueueFunction
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task Run([QueueTrigger("watermarkqueue")] PictureWatermarkQueue myQueueItem, ILogger log)
        {
            IBlobStorage blobStorage = new BlobStorage();

            INoSqlStorage<UserPicture> noSqlStorage = new TableStorage<UserPicture>();

            foreach (var item in myQueueItem.Pictures)
            {
                using var stream = await blobStorage.DownloadAsync(item, ContainerName.pictures);
                using var memoryStream = AddWaterMark(myQueueItem.WatermarkText, stream);

                await blobStorage.UploadAsync(memoryStream, item, ContainerName.watermarkpictures);

                _logger.LogInformation($"{item} resmine watermark eklenmiþtir.");
            }

            var userpicture = await noSqlStorage.Get(myQueueItem.UserId, myQueueItem.City);

            if (userpicture.WatermarkRawPaths != null)
                myQueueItem.Pictures.AddRange(userpicture.WatermarkPaths);

            userpicture.WatermarkPaths = myQueueItem.Pictures;

            await noSqlStorage.Add(userpicture);

            HttpClient httpClient = new HttpClient();

            var response = await httpClient.GetAsync("https://localhost:44367/api/Notifications/CompleteWatermarkProcess/" + myQueueItem.ConnectionId);

            _logger.LogInformation($"Client{myQueueItem.ConnectionId} Bilgiþendirilmiþtir!");
        }

        public static MemoryStream AddWaterMark(string watermarkText, Stream PictureStream)
        {
            MemoryStream ms = new MemoryStream();

            using (Image image = Bitmap.FromStream(PictureStream))
            {
                using (Bitmap tempBitmap = new Bitmap(image.Width, image.Height))
                {
                    using (Graphics gph = Graphics.FromImage(tempBitmap))
                    {
                        gph.DrawImage(image, 0, 0);

                        var font = new Font(FontFamily.GenericSansSerif, 32, FontStyle.Bold);

                        var color = Color.FromArgb(255, 0, 0);

                        var brush = new SolidBrush(color);

                        var point = new Point(20, image.Height - 50);

                        gph.DrawString(watermarkText, font, brush, point);

                        tempBitmap.Save(ms, ImageFormat.Jpeg);
                    }
                }
            }

            ms.Position = 0;

            return ms;
        }
    }
}
