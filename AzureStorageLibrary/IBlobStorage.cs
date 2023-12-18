using AzureStorageLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageLibrary
{
    public interface IBlobStorage
    {
        public string BlobUrl { get; }

        Task UploadAsync(Stream fileStream, string fileName, ContainerName containerName);

        Task<Stream> DownloadAsync(string fileName, ContainerName containerName);

        Task DeleteAsync(string fileName, ContainerName containerName);

        Task SetLogAsync(string text, string fileName);

        Task<List<string>> GetLogAsync(string fileName);

        List<string> GetNames(ContainerName containerName);
    }
}
