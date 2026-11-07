using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices
{
    public class AzureBlobImageStorage : IAzureBlobImageStorage
    {
        private readonly BlobContainerClient _containerClient;
        public AzureBlobImageStorage(BlobContainerClient containerClient)
        {
            _containerClient = containerClient;
        }
        public async Task<string> UploadAsync(Stream image, string contentType, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(image);
            var extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => throw new ArgumentException("Solo se permiten imágenes JPEG o PNG.", nameof(contentType))
            };
            var blobName = $"pets/{Guid.NewGuid():N}{extension}";
            var blobClient = _containerClient.GetBlobClient(blobName);
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },
                Conditions = new BlobRequestConditions
                {
                    IfNoneMatch = ETag.All
                }
            };
            await blobClient.UploadAsync(image, options, cancellationToken);
            return blobName;
        }
        public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(snapshotsOption: DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }

    }
}