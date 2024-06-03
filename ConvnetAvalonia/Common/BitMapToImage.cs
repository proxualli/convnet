using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace ConvnetAvalonia.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public class BitmapToImage : Image
    {
        public BitmapToImage(Bitmap bitmap) : base()
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, null);

            PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource destination = decoder.Frames[0];
            if (destination.CanFreeze)
                destination.Freeze();
            this.Source = destination;
        }
    }
}
