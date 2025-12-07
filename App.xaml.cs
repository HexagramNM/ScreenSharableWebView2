using System.Windows;

namespace ScreenSharableWebView2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int width = 1280;
        public static int height = 710;
        public static System.Uri path
            = new Uri("https://hexagramnm.coresv.com/NM_MicDisplay_Web/index.html");
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length >= 1)
            {
                path = new Uri(e.Args[0]);
            }
            if (e.Args.Length >= 3)
            {
                int.TryParse(e.Args[1], out width);
                int.TryParse(e.Args[2], out height);
            }
        }
    }

}
