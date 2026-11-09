using Application.Interfaces;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices
{
    public class AzureBlobStorage : IAzureBlobStorage
    {
        private readonly BlobContainerClient _containerClient;
        public AzureBlobStorage(BlobContainerClient containerClient)
        {
            _containerClient = containerClient;
        }
        public async Task<Uri> GetReadUrlAsync(string blobName, CancellationToken cancellationToken = default)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var serviceClient = _containerClient.GetParentBlobServiceClient();

            var now = DateTimeOffset.UtcNow;
            var startsOn = now.AddMinutes(-5);
            var expiresOn = now.AddMinutes(5);

            var keyOptions = new BlobGetUserDelegationKeyOptions(expiresOn)
            {
                StartsOn = startsOn
            };

            var delegationKey = await serviceClient.GetUserDelegationKeyAsync(keyOptions, cancellationToken);

            var sas = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = startsOn,
                ExpiresOn = expiresOn,
                Protocol = SasProtocol.Https
            };
            sas.SetPermissions(BlobSasPermissions.Read);

            return new BlobUriBuilder(blobClient.Uri)
            {
                Sas = sas.ToSasQueryParameters(delegationKey.Value, serviceClient.AccountName)
            }.ToUri();
        }
        public async Task<string> UploadImageAsync(Stream image, string contentType, CancellationToken cancellationToken = default)
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

        public async Task<string> UploadAttachmentAsync(Stream file, string contentType, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(file);
            var extension = contentType switch
            {
                "application/pdf" => ".pdf",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                _ => throw new ArgumentException("Solo se permiten archivos PDF o WORD", nameof(contentType))
            };
            var blobName = $"consultations/{Guid.NewGuid():N}{extension}";
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
            await blobClient.UploadAsync(file, options, cancellationToken);
            return blobName;
        }

        public async Task DeleteBlobAsync(string blobName, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(snapshotsOption: DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
    }
}
