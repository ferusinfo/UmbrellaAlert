using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using UmbrellaAlert.Resources;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using UmbrellaAlert.Classes;

namespace UmbrellaAlert
{

    public partial class MainPage : PhoneApplicationPage
    {
        ShellTileSchedule SampleTileSchedule = new ShellTileSchedule();

        public Geoposition position { get; set; }
        public WeatherJson WeatherData { get; set; }
        public BitmapImage UserMapImage { get; set; }
        public TileStatus tileS { get; set; }

        //Status codes 
        List<int> rainCodes = new List<int>(new int[] { 200,201,202,210,211,212,221,230,231, 232,500,501,502,503,504,511,520,521,522,900,901,902,903,904,905,906});
        List<int> littleCodes = new List<int>(new int[] { 300, 301, 302, 310, 311, 312, 321 });
        List<int> maybeCodes = new List<int>(new int[] { 802, 803, 804 });
        List<int> snowyCodes = new List<int>(new int[] { 600, 601, 602, 611, 621 });

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void Download_Complete(Object sender, DownloadStringCompletedEventArgs e) {
            if (!e.Cancelled && e.Error == null)
            {
                string textString = (string)e.Result;

                var WeatherData = JsonConvert.DeserializeObject<WeatherJson>(textString);
                Debug.WriteLine(textString);

                string imageURL = string.Format("http://api.tiles.mapbox.com/v4/ferus.m9b0gfjh/{1},{0},16/{2}x{3}.png?access_token=pk.eyJ1IjoiZmVydXMiLCJhIjoiNmRkOTEzZDQzZmM2NTgzOTQ0NWM1ZTQxNzAzZDAwMDEifQ.sCETqjF6fwWEPC2qbrejqA",
                        position.Coordinate.Latitude,
                        position.Coordinate.Longitude,
                        Application.Current.Host.Content.ActualWidth,
                        Application.Current.Host.Content.ActualHeight
                    );

                mapImage.Width = Application.Current.Host.Content.ActualWidth;
                mapImage.Height = Application.Current.Host.Content.ActualHeight;

                Debug.WriteLine(mapImage.Width);
                Debug.WriteLine(mapImage.Height);


                Uri uri = new Uri(imageURL, UriKind.Absolute);
                UserMapImage = new BitmapImage(uri);
                UserMapImage.CreateOptions = BitmapCreateOptions.None;

                UserMapImage.ImageOpened += (isender, iargs) =>
                {
                    //fire all other things

                    mapImage.Source = UserMapImage;
                    mapImage.Stretch = Stretch.UniformToFill;
                    mapImage.Opacity = 0.00;

                    HideRequest.Begin();

                    HideRequest.Completed += (s, ev) =>
                    {
                        dataMsg.Text = string.Format(AppResources.WillRainIn, WeatherData.city.name);
                        ShowCity.Begin();

                        ShowCity.Completed += (send, cev) =>
                        {
                            refreshbutton.Opacity = 1.00;
                            //200, 300, 800, 819, 600 test codes
                            var weatherCode = 600;//WeatherData.list.First().weather.First().id;
                            Debug.WriteLine(WeatherData.list.First().weather.First().id);
                            Debug.WriteLine(weatherCode);
                            var exists = rainCodes.Contains(weatherCode);

                            string response;
                            string subtitle;
                            string imageName;
                            dynamic anim;
                            Color color;


                            if (rainCodes.Contains(weatherCode))
                            {
                                imageName = "rain.png";
                                response = AppResources.RainYes;
                                subtitle = AppResources.RainYesSubtitle;
                                anim = Rainy;
                                ScaleAnimation.Begin();
                                color = HexToColor("#FF21424D");

                            }
                            else if (littleCodes.Contains(weatherCode))
                            {
                                imageName = "drizzle.png";
                                response = AppResources.Drizzle;
                                subtitle = AppResources.DrizzleSubtitle;
                                anim = Drizzle;
                                ScaleAnimation.Begin();
                                color = Colors.Purple;
                            }
                            else if (maybeCodes.Contains(weatherCode))
                            {
                                imageName = "maybe.png";
                                response = AppResources.Maybe;
                                subtitle = AppResources.MaybeSubtitle;
                                anim = Maybe;
                                color = Colors.Brown;
                                ScaleAnimation.Begin();
                            }
                            else if (snowyCodes.Contains(weatherCode))
                            {
                                imageName = "snow.png";
                                response = AppResources.Snow;
                                subtitle = AppResources.SnowSubtitle;
                                anim = Snow;
                                color = HexToColor("#FF008B8B");
                                ScaleAnimation.Begin();
                            }
                            else
                            {
                                imageName = "sun.png";
                                response = AppResources.Sunny;
                                subtitle = AppResources.SunnySubtitle;
                                anim = Sunny;
                                color = HexToColor("#FFB8860B");
                                SpinningAnimation.Begin();
                                ScaleAnimation.Begin();
                            }

                            string imagePath = string.Format("/Assets/WeatherImages/{0}", imageName);
                            Debug.WriteLine(frameBrush.Color);
                            
                            willRain.Text = response;
                            willRain_subtitle.Text = subtitle;
                            BitmapImage img = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                            rainImage.Source = img;

                            tileS = new TileStatus();
                            tileS.color = color;
                            tileS.title = response;
                            tileS.subtitle = subtitle;
                            tileS.image = img;
                            tileS.city = WeatherData.city.name;

                            updateTile();


                            anim.Begin();
                            ShowResult.Begin();

                        };
                    };


                };

             
             
            
            
            }
             
                
               
        }

        protected void updateTile()
        {
            IconicTileData iconicTileData = new IconicTileData();
            iconicTileData.Count = 0;
            iconicTileData.Title = string.Format("{0} ({1})",tileS.title, tileS.city);
            iconicTileData.IconImage = tileS.image.UriSource;
            iconicTileData.SmallIconImage = tileS.image.UriSource;
            iconicTileData.WideContent1 = tileS.subtitle;
            iconicTileData.WideContent2 = tileS.city;
            iconicTileData.BackgroundColor = tileS.color;

            ShellTile primaryTile = ShellTile.ActiveTiles.First();
            primaryTile.Update(iconicTileData);
        }

     public static Color HexToColor(string hex)
     {
        return Color.FromArgb(
        Convert.ToByte(hex.Substring(1, 2), 16),
        Convert.ToByte(hex.Substring(3, 2), 16),
        Convert.ToByte(hex.Substring(5, 2), 16),
        Convert.ToByte(hex.Substring(7, 2), 16)
        );
     }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            
            willRain.Opacity = 0.00;
            willRain_subtitle.Opacity = 0.00;
            rainImage.Opacity = 0.00;
            ScaleAnimation.Stop();
            SpinningAnimation.Stop();
            checkRain();
        }

        protected async void checkRain()
        {
            
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            geolocator.MovementThreshold = 5;
            geolocator.ReportInterval = 500;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(0),
                    timeout: TimeSpan.FromSeconds(20)
                    );

                position = geoposition;

                //Download weather
                WebClient client = new WebClient();
                    client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Download_Complete);
                    string forecastURL = string.Format("http://api.openweathermap.org/data/2.5/forecast/daily?lat={0}&lon={1}&cnt=1&mode=json&units=metric&lang={2}", position.Coordinate.Latitude,
                        position.Coordinate.Longitude, AppResources.WeatherLang);
                    client.DownloadStringAsync(new Uri(forecastURL));
               
              }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show(AppResources.LocationDisabled);
                    dataMsg.Text = AppResources.NoGPS;
                }
                else
                {
                    MessageBox.Show(AppResources.UnknownError);
                }
            }
        }

        private void refreshbutton_Click(object sender, RoutedEventArgs e)
        {
            refreshbutton.Opacity = 0.00;
            rainImage.Opacity = 0.00;
            willRain.Text = "";
            willRain_subtitle.Text = "";
            dataMsg.Text = AppResources.ObtainingLocation;
            ScaleAnimation.Stop();
            SpinningAnimation.Stop();
            RequestingDataColor.Begin();
            checkRain();
        }
    }
}