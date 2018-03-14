using Xamarin.Forms;

namespace XamarinFormsCamera
{
    public partial class App : Application
    {
        //Static variables for the app
        public static string DefaultImageId = "default_image";
        public static string ImageIdToSave = null;

        public App()
        {
            InitializeComponent();

            MainPage = new XamarinFormsCameraPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
