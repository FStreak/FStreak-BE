using FStreak.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services.Interface
{
    public interface ICloudinaryService
    {
        Task<Result<CloudinaryUploadResult>> UploadImageAsync(IFormFile file, string folder = "FStreak_images");
        Task<Result<CloudinaryUploadResult>> UploadVideoAsync(IFormFile file, string folder = "FStreak_videos");
        Task<Result<CloudinaryUploadResult>> UploadDocumentAsync(IFormFile file, string folder = "FStreak_documents");
        Task<Result<bool>> DeleteFileAsync(string publicId);
    }
    public class CloudinaryUploadResult
    {
        public string PublicId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public long Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
