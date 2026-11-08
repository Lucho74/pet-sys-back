using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAzureBlobStorage
    {
        Task<Uri> GetReadUrlAsync(string blobName, CancellationToken cancellationToken = default);
        Task<string> UploadImageAsync(Stream image, string contentType, CancellationToken cancellationToken = default);
        Task<string> UploadAttachmentAsync(Stream file, string contentType, CancellationToken cancellationToken = default);
        Task DeleteBlobAsync(string blobName, CancellationToken cancellationToken = default);

    }
}
