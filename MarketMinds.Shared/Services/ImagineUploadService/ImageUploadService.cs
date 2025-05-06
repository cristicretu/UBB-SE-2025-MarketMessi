using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using MarketMinds.Shared.Models;
using Newtonsoft.Json;
using System.IO;

namespace MarketMinds.Shared.Services.ImagineUploadService
{
    public class ImageUploadService : IImageUploadService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private const int MAX_IMAGE_SIZE = 10 * 1024 * 1024; // 10MB
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY = 2; // seconds
        private const string IMGUR_API_URL = "https://api.imgur.com/3/image";
        // Ensure this Client ID is correctly configured, potentially via IConfiguration for better practice
        private const string IMGUR_CLIENT_ID_PLACEHOLDER = "YOUR_IMGUR_CLIENT_ID"; 
        private const string IMAGE_CONTENT_TYPE = "image/png"; // Or determine from fileName extension
        // private const string IMAGE_NAME = "image.png"; // Will use fileName parameter
        private const int IMPUR_CLIENT_ID_LENGTH = 20; // Typo: IMGUR_CLIENT_ID_LENGTH
        private const int BASE_RETRY = 0;

        // Changed signature: removed Window, added Stream and fileName
        public async Task<string> UploadImage(Stream imageStream, string fileName)
        {
            if (imageStream == null)
            {
                throw new ArgumentNullException(nameof(imageStream));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                // Fallback or throw, Imgur might require a name or use a default
                // For now, let UploadToImgur handle or use a default if needed
            }
            // The file picker logic is removed. The stream is now passed directly.
            return await UploadToImgur(imageStream, fileName);
        }

        // Changed signature: takes Stream and fileName instead of StorageFile
        private async Task<string> UploadToImgur(Stream imageStream, string fileName)
        {
            try
            {
                string clientId = GetImgurClientId();
                if (string.IsNullOrEmpty(clientId) || clientId == IMGUR_CLIENT_ID_PLACEHOLDER)
                {
                    throw new InvalidOperationException("Imgur Client ID is not configured. Please check your appsettings.json file or environment configuration.");
                }
                if (clientId.Length > IMPUR_CLIENT_ID_LENGTH) // Typo: IMGUR_CLIENT_ID_LENGTH
                {
                    throw new InvalidOperationException("Client ID format appears invalid. Please ensure you're using the Client ID, not the Client Secret.");
                }

                // Use MemoryStream to buffer the input stream if it's not seekable or to get its length easily.
                // This also helps ensure we're not re-reading a non-seekable stream multiple times.
                using (var memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    if (memoryStream.Length == 0)
                    {
                        throw new InvalidOperationException("Image stream is empty.");
                    }
                    if (memoryStream.Length > MAX_IMAGE_SIZE)
                    {
                        throw new InvalidOperationException($"File size ({memoryStream.Length} bytes) exceeds Imgur's 10MB limit.");
                    }

                    byte[] buffer = memoryStream.ToArray();

                    int maxRetries = MAX_RETRIES;
                    int currentRetry = BASE_RETRY;
                    TimeSpan delay = TimeSpan.FromSeconds(RETRY_DELAY);

                    while (currentRetry < maxRetries)
                    {
                        try
                        {
                            using (var content = new MultipartFormDataContent())
                            {
                                var imageContent = new ByteArrayContent(buffer);
                                // Potentially set content type based on fileName extension if available
                                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(IMAGE_CONTENT_TYPE); 
                                content.Add(imageContent, "image", fileName ?? "image.png"); // Use provided fileName

                                using (var request = new HttpRequestMessage(HttpMethod.Post, IMGUR_API_URL))
                                {
                                    request.Headers.Add("Authorization", $"Client-ID {clientId}");
                                    request.Content = content;
                                    var response = await HttpClient.SendAsync(request);
                                    var responseBody = await response.Content.ReadAsStringAsync();

                                    if (response.IsSuccessStatusCode)
                                    {
                                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                                        string link = jsonResponse?.data?.link;
                                        if (!string.IsNullOrEmpty(link))
                                        {
                                            return link;
                                        }
                                    }
                                    // Log non-success response for debugging if needed
                                }
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            // Log exception but continue with retry
                            // Consider logging ex.Message
                            if (currentRetry >= maxRetries -1) throw; // Rethrow if last attempt
                        }

                        if (currentRetry < maxRetries - 1)
                        {
                            await Task.Delay(delay);
                            delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                        }
                        currentRetry++;
                    }
                    // If loop finishes without returning, it means all retries failed.
                    throw new InvalidOperationException("Failed to upload image to Imgur after multiple retries.");
                }
            }
            catch (Exception ex)
            {
                // Log or handle more gracefully if needed, but rethrowing preserves the error for the caller.
                throw new InvalidOperationException($"Imgur upload failed: {ex.Message}", ex);
            }
        }

        private string GetImgurClientId()
        {
            try
            {
                // This method of getting config is fragile. Consider IConfiguration for robustness.
                string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MarketMinds.Shared", "appsettings.json");

                if (!File.Exists(appSettingsPath))
                {
                    string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    string targetDirName = "MarketMinds.Shared";
                    string rootPath = Path.GetPathRoot(currentDir);
                    while (currentDir != null && currentDir != rootPath && !Directory.Exists(Path.Combine(currentDir, targetDirName)))
                    {
                        currentDir = Directory.GetParent(currentDir)?.FullName;
                    }

                    if (currentDir != null && Directory.Exists(Path.Combine(currentDir, targetDirName)))
                    {
                        appSettingsPath = Path.Combine(currentDir, targetDirName, "appsettings.json");
                    } else {
                        // Attempt to find it in the main project's output if shared is a subfolder from perspective of execution
                        string mainProjectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."); // Go up one level potentially
                        if (Directory.Exists(Path.Combine(mainProjectDir, targetDirName)))
                        {
                             appSettingsPath = Path.Combine(mainProjectDir, targetDirName, "appsettings.json");
                        }
                    }
                }
                
                if (File.Exists(appSettingsPath))
                {
                    string json = File.ReadAllText(appSettingsPath);
                    dynamic settings = JsonConvert.DeserializeObject(json);
                    return settings?.ImgurSettings?.ClientId?.ToString();
                }
                // Fallback or throw an error if configuration is critical
                return IMGUR_CLIENT_ID_PLACEHOLDER; // Return placeholder or empty to be caught by caller
            }
            catch (Exception ex) // Catch specific exceptions if possible
            {
                // Log ex.Message for debugging
                return IMGUR_CLIENT_ID_PLACEHOLDER; // Fallback on any error
            }
        }

        public async Task<bool> UploadImageAsync(string filePath)
        {
            // This method needs proper implementation if used.
            // It should open a FileStream and call UploadImage(stream, Path.GetFileName(filePath)).
            // However, direct file path access is platform-dependent for permissions.
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                 // Log error or throw ArgumentException
                return false;
            }
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    string uploadedUrl = await UploadImage(fileStream, Path.GetFileName(filePath));
                    return !string.IsNullOrEmpty(uploadedUrl);
                }
            }
            catch (Exception ex)
            {
                // Log ex.Message
                return false;
            }
        }

        public Image CreateImageFromPath(string path)
        {
            return new Image(path);
        }

        public string FormatImagesString(List<Image> images)
        {
            return images != null ? string.Join("\n", images.Select(img => img.Url)) : string.Empty;
        }

        public List<Image> ParseImagesString(string imagesString)
        {
            if (string.IsNullOrEmpty(imagesString))
            {
                return new List<Image>();
            }

            return imagesString.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(url => !string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .Select(url => new Image(url))
                .ToList();
        }

        // Changed signature: removed Window, added Stream and fileName
        public async Task<string> AddImageToCollection(Stream imageStream, string fileName, string currentImagesString)
        {
            string imgurLink = await UploadImage(imageStream, fileName);
            if (!string.IsNullOrEmpty(imgurLink))
            {
                // Ensure new link is not already present if that's a requirement
                var existingImages = ParseImagesString(currentImagesString).Select(i => i.Url).ToList();
                if (!existingImages.Contains(imgurLink))
                {
                     return string.IsNullOrEmpty(currentImagesString)
                        ? imgurLink
                        : currentImagesString + "\n" + imgurLink;
                }
            }
            return currentImagesString;
        }
    }
}