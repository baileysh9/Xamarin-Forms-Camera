using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(XamarinFormsCamera.iOS.CameraIOS))]
namespace XamarinFormsCamera.iOS
{
    public class CameraIOS : CameraInterface
    {
        public async void LaunchCamera(FileFormatEnum imageType, string imageId = null)
        {
            //Check if we have permission to use the camera
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            //If we don't have access, and have never asked before, prompt them
            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                var access = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);

                //If access was granted we can proceed, if not, you can add an else statement and implement an error message or something more helpful
                if (access)
                {
                    if(imageId == null)
                        GotAccessToCamera(imageType);
                    else
                    {
                        var fileName = imageId + "." + imageType.ToString();
                        GotAccessToCamera(imageType, fileName);
                    }
                }
            }
            else
            {
                //We've already been given access
                if (imageId == null)
                    GotAccessToCamera(imageType);
                else
                {
                    var fileName = imageId + "." + imageType.ToString();
                    GotAccessToCamera(imageType, fileName);
                }
            }
        }

        public void LaunchGallery(FileFormatEnum imageType, string imageId = null)
        {
            try
            {
                var imagePicker = new UIImagePickerController { SourceType = UIImagePickerControllerSourceType.PhotoLibrary, MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary) };
                imagePicker.AllowsEditing = true;

                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }

                vc.PresentViewController(imagePicker, true, null);

                imagePicker.FinishedPickingMedia += (sender, e) =>
                {
                    UIImage originalImage = e.Info[UIImagePickerController.EditedImage] as UIImage;
                    if (originalImage != null)
                    {
                        NSData pngImage = null;

                        if (imageType == FileFormatEnum.JPEG)
                            pngImage = originalImage.AsJPEG();
                        else
                            pngImage = originalImage.AsPNG();

                        byte[] myByteArray = new byte[pngImage.Length];
                        System.Runtime.InteropServices.Marshal.Copy(pngImage.Bytes, myByteArray, 0, Convert.ToInt32(pngImage.Length));

                        MessagingCenter.Send<byte[]>(myByteArray, "ImageSelected");
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        vc.DismissViewController(true, null);
                    });
                };

                imagePicker.Canceled += (sender, e) => vc.DismissViewController(true, null);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        private void GotAccessToCamera(FileFormatEnum imageType, string imageId = null)
        {
            var imagePicker = new UIImagePickerController { SourceType = UIImagePickerControllerSourceType.Camera };
            var window = UIApplication.SharedApplication.KeyWindow;
            var vc = window.RootViewController;

            while (vc.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }

            vc.PresentViewController(imagePicker, true, null);

            imagePicker.FinishedPickingMedia += (sender, e) =>
            {
                UIImage image = (UIImage)e.Info.ObjectForKey(new NSString("UIImagePickerControllerOriginalImage"));
                UIImage rotateImage = RotateImage(image, image.Orientation);
                rotateImage = rotateImage.Scale(new CGSize(rotateImage.Size.Width, rotateImage.Size.Height), 0.5f);

                NSData imgData = null;

                if (imageType == FileFormatEnum.PNG)
                    imgData = rotateImage.AsPNG();
                else
                    imgData = rotateImage.AsJPEG();

                byte[] myByteArray = new byte[imgData.Length];
                System.Runtime.InteropServices.Marshal.Copy(imgData.Bytes, myByteArray, 0, Convert.ToInt32(imgData.Length));

                if(imageId != null)
                    SavePhoto(rotateImage, imageId, imageType);

                MessagingCenter.Send<byte[]>(myByteArray, "ImageSelected");

                Device.BeginInvokeOnMainThread(() =>
                {
                    vc.DismissViewController(true, null);
                });
            };

            imagePicker.Canceled += (sender, e) => vc.DismissViewController(true, null);
        }

        public void SavePhoto(UIImage photo, string imageName, FileFormatEnum imageType)
        {
            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(documentsDirectory, imageName);
            NSData imgData;

            if (imageType == FileFormatEnum.PNG)
                imgData = photo.AsPNG();
            else
                imgData = photo.AsJPEG();

            NSError err = null;
            if (imgData.Save(filePath, false, out err))
            {
                Console.WriteLine("Saved image to " + filePath);
            }
            else
            {
                //Handle the Error!
                Console.WriteLine("Could NOT save to " + filePath + " because" + err.LocalizedDescription);
            }
        }

        double radians(double degrees) { return degrees * Math.PI / 180; }

        private UIImage RotateImage(UIImage src, UIImageOrientation orientation)
        {
            UIGraphics.BeginImageContext(src.Size);

            if (orientation == UIImageOrientation.Right)
            {
                CGAffineTransform.MakeRotation((nfloat)radians(90));
            }
            else if (orientation == UIImageOrientation.Left)
            {
                CGAffineTransform.MakeRotation((nfloat)radians(-90));
            }
            else if (orientation == UIImageOrientation.Down)
            {
                // NOTHING
            }
            else if (orientation == UIImageOrientation.Up)
            {
                CGAffineTransform.MakeRotation((nfloat)radians(90));
            }

            src.Draw(new CGPoint(0, 0));
            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}
