using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Convnet.Common
{
    public class BitmapToImage : Image
    {
        public BitmapToImage(System.Drawing.Bitmap bitmap) : base()
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource destination = decoder.Frames[0];
            if (destination.CanFreeze)
                destination.Freeze();
            this.Source = destination;
        }
    }
}
