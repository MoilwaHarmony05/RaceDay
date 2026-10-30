using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("AzureBlobStorage");
        var containerName = config["AzureBlob:ContainerName"] ?? "event-images";
        var service = new BlobServiceClient(connectionString);
        _container = service.GetBlobContainerClient(containerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        var blob = _container.GetBlobClient(fileName);
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
        return blob.Uri.ToString();
    }
}