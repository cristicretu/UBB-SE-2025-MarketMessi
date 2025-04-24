using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Marketplace_SE.Utilities;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace MarketMinds.Converters
{
    public class HexToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hexString && !string.IsNullOrEmpty(hexString))
            {
                try
                {
                    byte[] imageBytes = DataEncoder.HexDecode(hexString);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        using (var stream = new InMemoryRandomAccessStream())
                        {
                            stream.WriteAsync(imageBytes.AsBuffer()).AsTask().Wait();

                            stream.Seek(0);

                            var bitmap = new BitmapImage();
                            bitmap.SetSource(stream);
                            return bitmap;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting hex to BitmapImage: {ex.Message}");
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
