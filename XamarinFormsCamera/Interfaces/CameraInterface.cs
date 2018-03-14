using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsCamera
{
    public interface CameraInterface
    {
        void LaunchCamera(FileFormatEnum imageType, string imageId = null);
        void LaunchGallery(FileFormatEnum imageType, string imageId = null);
    }
}
