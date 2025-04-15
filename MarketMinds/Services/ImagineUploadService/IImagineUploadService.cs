using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using DomainLayer.Domain;

namespace MarketMinds.Services.ImagineUploadService
{
    /// <summary>
    /// Interface for ImageUploadService to manage image upload operations.
    /// </summary>
    public interface IImageUploadService
    {
        /// <summary>
        /// Uploads an image using a file picker.
        /// </summary>
        /// <param name="window">The parent window for the file picker.</param>
        /// <returns>The URL of the uploaded image.</returns>
        Task<string> UploadImage(Window window);

        /// <summary>
        /// Simulates an image upload using a file path.
        /// </summary>
        /// <param name="filePath">The file path of the image.</param>
        /// <returns>True if the upload is successful, otherwise false.</returns>
        Task<bool> UploadImageAsync(string filePath);

        /// <summary>
        /// Creates an Image object from a file path.
        /// </summary>
        /// <param name="path">The file path of the image.</param>
        /// <returns>An Image object.</returns>
        Image CreateImageFromPath(string path);

        /// <summary>
        /// Formats a list of images into a single string.
        /// </summary>
        /// <param name="images">The list of images.</param>
        /// <returns>A formatted string of image URLs.</returns>
        string FormatImagesString(List<Image> images);

        /// <summary>
        /// Parses a string of image URLs into a list of Image objects.
        /// </summary>
        /// <param name="imagesString">The string of image URLs.</param>
        /// <returns>A list of Image objects.</returns>
        List<Image> ParseImagesString(string imagesString);

        /// <summary>
        /// Adds an image to an existing collection of images.
        /// </summary>
        /// <param name="window">The parent window for the file picker.</param>
        /// <param name="currentImagesString">The current collection of image URLs as a string.</param>
        /// <returns>The updated collection of image URLs as a string.</returns>
        Task<string> AddImageToCollection(Window window, string currentImagesString);
    }
}
