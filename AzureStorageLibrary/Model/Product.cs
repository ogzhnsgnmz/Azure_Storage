using Azure;
using Microsoft.Azure.Cosmos.Table;
using S = Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorageLibrary.Model;

public class Product : TableEntity
{
    public string Name { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public string Color { get; set; }
}
