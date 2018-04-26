using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace MagicMirrorTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        const bool SECOND_MONITOR = false;

        Dictionary<String, List<String>> folders = new Dictionary<String, List<String>>();
        Dictionary<String, MediaElement> movieMedia = new Dictionary<String, MediaElement>();
        String[] dbgText = new String[3];
        private const String PORT = "8080";

        private Dictionary<String, String> wsIcons = new Dictionary<String, String>();

        public MainWindow()
        {
            InitializeComponent();
            initVideoFiles();
            initWebServer();
        }

        private void initVideoFiles()
        {
            List<String> misc = new List<String>();
            List<String> pumpkin = new List<String>();
            List<String> princess = new List<String>();
            List<String> skull = new List<String>();
            List<String> pirate = new List<String>();
            List<String> apparitionsGG = new List<String>();
            List<String> apparitionsBB = new List<String>();
            List<String> ghosts = new List<String>();
            List<String> skeletons = new List<String>();

            foreach (String file in Directory.GetFiles("movies/misc", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                misc.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/pumpkin", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                pumpkin.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/princess", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                princess.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/skull", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                skull.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/pirate", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                pirate.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/apparitions/ghoulish_girl", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                apparitionsGG.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/apparitions/beckoning_beauty", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                apparitionsBB.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/ghost", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                ghosts.Add(moviesName);
            }

            foreach (String file in Directory.GetFiles("movies/skeleton", "*.mp4")) {
                string moviesName = Path.GetFileNameWithoutExtension(file);
                skeletons.Add(moviesName);
            }



            folders.Add("skeleton", skeletons);
            folders.Add("ghost", ghosts);
            folders.Add("apparitions\\beckoning_beauty", apparitionsBB);
            folders.Add("apparitions\\ghoulish_girl", apparitionsGG);
            folders.Add("skull", skull);
            folders.Add("pirate", pirate);
            folders.Add("pumpkin", pumpkin);
            folders.Add("princess", princess);
            folders.Add("misc", misc);

            for (int i = folders.Count - 1; i >= 0; i--) {
                var item = folders.ElementAt(i);
                String itemKey = item.Key;
                List<String> itemValue = item.Value;
                foreach (String movie in itemValue) {
                    preloadMovie(itemKey.Replace("\\", "/") + "/" + movie);
                }
            }

            dbgText[2] = "Animations: "+ movieMedia.Count;
            updateDebugText();
        }

        private void preloadMovie(String movie)
        {
            Console.WriteLine("Preload: " + movie);
            MediaElement mediaElement = new MediaElement();
            mediaElement.LoadedBehavior = MediaState.Pause;
            mediaElement.Source = new Uri(System.IO.Path.GetFullPath("movies\\" + movie + ".mp4"));
            mediaElement.MediaEnded += mediaElement_MediaEnded;
            movieMedia.Add(movie, mediaElement);
        }


        private void initWebServer()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }

            WebServer ws = new WebServer(SendResponse, "http://" + localIP + ":" + PORT + "/");
            ws.Run();

            //labelDebugInfo.Content = "IP: " + localIP;
            dbgText[0] = "IP: " + localIP + ":" + PORT;
            dbgText[1] = "Face: null";
            updateDebugText();

            labelDebugInfo.Visibility = Visibility.Visible;
        }

        public string SendResponse(HttpListenerRequest request)
        {

            if (request.RawUrl.Contains("wsfiles/")) {
                return ResponceFile(request);
            }

            String toReturn = "<html>";
            toReturn += "<head>";

            toReturn += "<script src=\"/wsfiles/jquery-3.3.1.min.js\"></script>";
            toReturn += "<script src=\"/wsfiles/script.js\" defer></script>";
            toReturn += "<link rel=\"stylesheet\" href=\"/wsfiles/style.css\">";

            toReturn += "</head>";
            toReturn += "<body>";

            if (request.RawUrl.StartsWith("/play/")) {
                toReturn += "<div class=\"shouldnot icon-back\"><a href=\"/\" >Back</a></div>";
                toReturn += ResponcePlay(request);
            }
            else if (request.RawUrl.StartsWith("/settings/")) {
                toReturn += "<div class=\"shouldnot icon-back\"><a href=\"/\" >Back</a></div>";
                toReturn += ResponceSettings(request);
            }
            else {
                toReturn += ResponceMain(request);
            }

            toReturn += "</body>";
            toReturn += "</html>";

            return toReturn;
        }

        private String ResponceMain(HttpListenerRequest request)
        {
            String toReturn = "";

            toReturn += "<div class=\"shouldnot icon-princess\"><a href=\"/play/\">Faces</a></div>";
            toReturn += "<div class=\"shouldnot icon-settings\"><a href=\"/settings/\" >Settings</a></div>";

            return toReturn;
        }

        private String ResponceSettings(HttpListenerRequest request)
        {
            string theSetting = request.RawUrl;

            if (theSetting.Contains("/change/")) {
                this.Dispatcher.Invoke(delegate {
                    if (theSetting.Contains("debugText")) {
                        labelDebugInfo.Visibility = ToggleVisibility(labelDebugInfo.Visibility);
                    }
                });
                return "";
            }
            

            String toReturn = "";

            toReturn += "<div class=\"should icon-debugText\"><a href=\"/settings/change/debugText/\">Debug Text</a></div>";

            return toReturn;
        }

        private String ResponceFile(HttpListenerRequest request)
        {
            string filename = request.Url.AbsolutePath;
            string path = filename.Substring(1);

            return File.ReadAllText(path);
        }

        private String ResponcePlay(HttpListenerRequest request)
        {

            if (request.RawUrl.StartsWith("/play/movie/")) {
                if (request.RawUrl.Contains("stop")) {
                    this.Dispatcher.Invoke(delegate {
                        StopAllMovies();
                    });
                }
                else {
                    string theMovie = request.RawUrl.Substring(12);
                    this.Dispatcher.Invoke(delegate {
                        PlayMovie(theMovie);
                    });
                }
                return "";
            }


            String toReturn = "";

            toReturn += "<div class=\"should icon-stop\"\"><a href=\"/play/movie/stop\">STOP ALL</a></div>";

            //TODO: THIS IS WHY EVERYTHING IS BACKWARDS
            for (int i = folders.Count - 1; i >= 0; i--) {
                var item = folders.ElementAt(i);
                String itemKey = item.Key;
                List<String> itemValue = item.Value;
                foreach (String movie in itemValue) {

                    String iconClass = "icon-" + itemKey.Replace('\\', '-').Replace('_', '-');

                    toReturn += "<div class=\"should " + iconClass + "\"><a href=\"/play/movie/" + itemKey + "/" + movie + "\">" + movie + "</a></div>";
                    
                }
            }

            return toReturn;
        }

        void StopAllMovies()
        {
            foreach (UIElement o in dockP.Children) {
                if (o is MediaElement) {
                    ((MediaElement)o).Stop();
                }
            }
            dockP.Children.Clear();
            dbgText[1] = "Face: null";
            updateDebugText();
        }

        void PlayMovie(string name)
        {
            MediaElement mediaElement = movieMedia[name];

            StopAllMovies();

            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Visibility = Visibility.Visible;
            dockP.Children.Add(mediaElement);
            mediaElement.Play();
            dbgText[1] = "Face: " + name.Replace("_", "-");
            updateDebugText();
        }

        private Visibility ToggleVisibility(Visibility visibility)
        {
            return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopAllMovies();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                this.Close();
            }
        }

        private void updateDebugText()
        {
            String toPrint = "";
            for(int i = 0; i < dbgText.Length; i++) {
                toPrint += dbgText[i] + "\r\n\r\n";
            }
            labelDebugInfo.Content = toPrint;
        }

        //TODO: Long run this might not be nessessarry (Button)
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.WindowState = WindowState.Normal;

            if (SECOND_MONITOR)
            {
                //Second monitor
                

                var screen = System.Windows.Forms.Screen.AllScreens[1];
                var workingArea = screen.WorkingArea;
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                //Nice shitty bug workaround : https://stackoverflow.com/questions/4189660/why-does-wpf-mediaelement-not-work-on-secondary-monitor
                var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                if (hwndSource != null) {
                    var hwndTarget = hwndSource.CompositionTarget;
                    if (hwndTarget != null) hwndTarget.RenderMode = RenderMode.SoftwareOnly;
                }
            }

        }
        //TODO: Long run this might not be nessessarry (Button)
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (SECOND_MONITOR)
            {
                this.WindowState = WindowState.Maximized; //Second monitor
            }
        }
    }
}
