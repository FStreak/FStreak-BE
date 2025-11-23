using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FStreak.Application.Configuration;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FStreak.Application.Services.Implementation
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> config, ILogger<CloudinaryService> logger)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _logger = logger;
        }

        public async Task<Result<CloudinaryUploadResult>> UploadImageAsync(IFormFile file, string folder = "images")
        {
            if (file == null || file.Length == 0)
                return Result<CloudinaryUploadResult>.Failure("File is empty");

            // Validate image type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return Result<CloudinaryUploadResult>.Failure("Invalid image format. Only JPEG, PNG, GIF, WEBP are allowed");

            // Max 10MB for images
            if (file.Length > 10_000_000)
                return Result<CloudinaryUploadResult>.Failure("Image size must be less than 10MB");

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto"),
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    return Result<CloudinaryUploadResult>.Failure($"Upload failed: {uploadResult.Error.Message}");
                }

                var result = new CloudinaryUploadResult
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url.ToString(),
                    SecureUrl = uploadResult.SecureUrl.ToString(),
                    Format = uploadResult.Format,
                    Size = uploadResult.Length,
                    Width = uploadResult.Width,
                    Height = uploadResult.Height
                };

                return Result<CloudinaryUploadResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                return Result<CloudinaryUploadResult>.Failure("Failed to upload image");
            }
        }

        public async Task<Result<CloudinaryUploadResult>> UploadVideoAsync(IFormFile file, string folder = "videos")
        {
            if (file == null || file.Length == 0)
                return Result<CloudinaryUploadResult>.Failure("File is empty");

            // Validate video type
            var allowedTypes = new[] { "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo", "video/webm" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return Result<CloudinaryUploadResult>.Failure("Invalid video format. Only MP4, MPEG, MOV, AVI, WEBM are allowed");

            // Max 100MB for videos
            if (file.Length > 100_000_000)
                return Result<CloudinaryUploadResult>.Failure("Video size must be less than 100MB");

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto"),
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    return Result<CloudinaryUploadResult>.Failure($"Upload failed: {uploadResult.Error.Message}");
                }

                var result = new CloudinaryUploadResult
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url.ToString(),
                    SecureUrl = uploadResult.SecureUrl.ToString(),
                    Format = uploadResult.Format,
                    Size = uploadResult.Length,
                    Width = uploadResult.Width,
                    Height = uploadResult.Height
                };

                return Result<CloudinaryUploadResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video to Cloudinary");
                return Result<CloudinaryUploadResult>.Failure("Failed to upload video");
            }
        }

        public async Task<Result<CloudinaryUploadResult>> UploadDocumentAsync(IFormFile file, string folder = "documents")
        {
            if (file == null || file.Length == 0)
                return Result<CloudinaryUploadResult>.Failure("File is empty");

            // Validate document type
            var allowedTypes = new[] { "application/pdf", "application/msword", 
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };
            
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return Result<CloudinaryUploadResult>.Failure("Invalid document format");

            // Max 20MB for documents
            if (file.Length > 20_000_000)
                return Result<CloudinaryUploadResult>.Failure("Document size must be less than 20MB");

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    return Result<CloudinaryUploadResult>.Failure($"Upload failed: {uploadResult.Error.Message}");
                }

                var result = new CloudinaryUploadResult
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url.ToString(),
                    SecureUrl = uploadResult.SecureUrl.ToString(),
                    Format = uploadResult.Format,
                    Size = uploadResult.Length
                };

                return Result<CloudinaryUploadResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to Cloudinary");
                return Result<CloudinaryUploadResult>.Failure("Failed to upload document");
            }
        }

        public async Task<Result<bool>> DeleteFileAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result == "ok")
                    return Result<bool>.Success(true);

                _logger.LogWarning("Failed to delete file from Cloudinary: {PublicId}", publicId);
                return Result<bool>.Failure("Failed to delete file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Cloudinary");
                return Result<bool>.Failure("Failed to delete file");
            }
        }
    }
}
