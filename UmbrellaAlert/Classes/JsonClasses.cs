using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace UmbrellaAlert.Classes
{
    public class WeatherJson
    {
        [JsonProperty("city")]
        public City city { get; set; }

        [JsonProperty("list")]
        public List<WeatherList> list { get; set; }
    }

    public class City
    {
        [JsonProperty("name")]
        public string name { get; set; }
    }

    public class WeatherList
    {
        [JsonProperty("weather")]
        public List<Weather> weather { get; set; }
    }

    public class Weather
    {
        [JsonProperty("id")]
        public int id { get; set; }
    }

    public class TileStatus
    {
        public Color color { get; set; }
        public String title { get; set; }
        public String subtitle { get; set; }
        public BitmapImage image { get; set; }
        public String city { get; set; }
    }
   
}
