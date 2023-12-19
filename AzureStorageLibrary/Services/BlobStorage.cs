using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using AzureStorageLibrary.Model;

namespace AzureStorageLibrary.Services;

public class BlobStorage : IBlobStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorage()
    {
        var connectionString = "DefaultEndpointsProtocol=https;AccountName=projectstorage00;AccountKey=SZvnl6Nj761fMfRf4Y2oirwIpL3lSHSCcOvzPxLNlnDqTrQ6+mV6x72JgDyR7Skfn9dqCXeFSyb9+AStTTozPg==;EndpointSuffix=core.windows.net";
        _blobServiceClient = new BlobServiceClient(connectionString);
        //_blobServiceClient = new BlobServiceClient(ConnectionStrings.AzureStorageConnectionString);
    }

    public string BlobUrl => "https://projectstorage00.blob.core.windows.net";
    
    public async Task DeleteAsync(string fileName, ContainerName containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToString());

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteAsync();
    }

    public async Task<Stream> DownloadAsync(string fileName, ContainerName containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToString());

        var blobClient = containerClient.GetBlobClient(fileName);

        var info = await blobClient.DownloadAsync();

        return info.Value.Content;
    }

    public async Task<List<string>> GetLogAsync(string fileName)
    {
        List<string> logs = new List<string>();
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName.logs.ToString());

        await containerClient.CreateIfNotExistsAsync();

        var appendBlobClient = containerClient.GetAppendBlobClient(fileName);

        await appendBlobClient.CreateIfNotExistsAsync();

        var info = await appendBlobClient.DownloadAsync();

        using (StreamReader sr = new StreamReader(info.Value.Content))
        {
            string line = string.Empty;

            while ((line = sr.ReadLine()) != null)
                logs.Add(line);
        }
        return logs;
    }

    public List<string> GetNames(ContainerName containerName)
    {
        List<string> blobNames = new List<string>();

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToString());

        var blobs = containerClient.GetBlobs();

        blobs.ToList().ForEach(x =>
        {
            blobNames.Add(x.Name);
        });

        return blobNames;
    }

    public async Task SetLogAsync(string text, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName.logs.ToString());

        var appendBlobClient = containerClient.GetAppendBlobClient(fileName);

        await appendBlobClient.CreateIfNotExistsAsync();

        using (MemoryStream ms = new MemoryStream())
        {
            using (StreamWriter sw = new StreamWriter(ms))
            {
                sw.Write($"{DateTime.Now}: {text}\n");

                sw.Flush();
                ms.Position = 0;

                await appendBlobClient.AppendBlockAsync(ms);
            }
        }
    }

    public async Task UploadAsync(Stream fileStream, string fileName, ContainerName containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToString());

        await containerClient.CreateIfNotExistsAsync();

        await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(fileStream, true);
    }
}
