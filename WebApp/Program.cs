using AzureStorageLibrary;
using AzureStorageLibrary.Services;
using Microsoft.Extensions.Azure;
using WebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var configuration = builder.Configuration;

ConnectionStrings.AzureStorageConnectionString = configuration.GetSection("AzureConnectionStrings")["StorageConStr"];

builder.Services.AddScoped(typeof(INoSqlStorage<>), typeof(TableStorage<>));
builder.Services.AddSingleton<IBlobStorage, BlobStorage>();

builder.Services.AddSignalR();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConStr:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConStr:queue"], preferMsi: true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<NotificationHub>("/NotificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
