using System.Linq.Expressions;
using S = Microsoft.WindowsAzure.Storage;
using ST = Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Cosmos.Table;

namespace AzureStorageLibrary.Services;

public class TableStorage<TEntity> : INoSqlStorage<TEntity>
    where TEntity : TableEntity, new()
{
    private readonly CloudTableClient _cloudTableClient;
    private readonly CloudTable _cloudTable;

    public TableStorage()
    {
        var connectionString = "DefaultEndpointsProtocol=https;AccountName=projectstorage00;AccountKey=SZvnl6Nj761fMfRf4Y2oirwIpL3lSHSCcOvzPxLNlnDqTrQ6+mV6x72JgDyR7Skfn9dqCXeFSyb9+AStTTozPg==;EndpointSuffix=core.windows.net";
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
        //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionStrings.AzureStorageConnectionString);
        _cloudTableClient = storageAccount.CreateCloudTableClient();

        _cloudTable = _cloudTableClient.GetTableReference(typeof(TEntity).Name);

        _cloudTable.CreateIfNotExistsAsync();
    }

    public async Task<TEntity?> Add(TEntity entity)
    {
        var operation = TableOperation.InsertOrMerge(entity);

        var execute = await _cloudTable.ExecuteAsync(operation);

        return execute.Result as TEntity;
    }

    public IQueryable<TEntity> All()
    {
        return _cloudTable.CreateQuery<TEntity>().AsQueryable();
    }

    public async Task Delete(string rowKey, string partitionKey)
    {
        var entity = await Get(rowKey, partitionKey);

        var operation = TableOperation.Delete(entity);

        await _cloudTable.ExecuteAsync(operation);
    }

    public async Task<TEntity?> Get(string rowKey, string partitionKey)
    {
        var operation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

        var execute = await _cloudTable.ExecuteAsync(operation);

        return execute.Result as TEntity;
    }

    public IQueryable<TEntity?> Query(Expression<Func<TEntity, bool>> query)
    {
        return _cloudTable.CreateQuery<TEntity>().Where(query);
    }

    public async Task<TEntity?> Update(TEntity entity)
    {
        var operation = TableOperation.Replace(entity);

        var execute = _cloudTable.ExecuteAsync(operation);

        return execute.Result as TEntity;
    }
}
