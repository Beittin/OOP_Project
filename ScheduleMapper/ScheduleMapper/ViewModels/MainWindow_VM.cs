using ScheduleMapper.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Data.Entity;

namespace ScheduleMapper.ViewModels
{
    public class MainWindow_VM : ViewModel_Base
    {
        #region Variables
        private CoordinateModel _context = new CoordinateModel();
        private Properties.Settings Set = Properties.Settings.Default;
        private string _title;
        private ObservableCollection<Coordinate> _coordinates;
        private ObservableCollection<ImageSource> _images;
        private int _originIndex;
        private int _destinationIndex;
        private ObservableCollection<int> _waypointIndex;
        #endregion

        #region Properties
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Coordinate> Coordinates
        {
            get { return _coordinates; }
            set { _coordinates = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ImageSource> Images
        {
            get { return _images; }
            set { _images = value; OnPropertyChanged(); }
        }
        public string Origin
        { get { return Coordinates[OriginIndex].ToString(); } }
        public string Destination
        { get { return Coordinates[DestinationIndex].ToString(); } }
        public int OriginIndex
        {
            get { return _originIndex; }
            set { _originIndex = value; OnPropertyChanged(); }
        }
        public int DestinationIndex
        {
            get { return _destinationIndex; }
            set { _destinationIndex = value; OnPropertyChanged(); }
        }
        public ObservableCollection<int> WaypointIndex
        {
            get { return _waypointIndex; }
            set { _waypointIndex = value; OnPropertyChanged(); }
        }
        #endregion

        #region Constructors
        public MainWindow_VM()
        {
            Initialize();
            //string responseFromServer = GetServerResponse();
            //string poly;
            //dynamic obj = JsonConvert.DeserializeObject(responseFromServer);
            //poly = obj.routes[0].overview_polyline.points.Value;
            //string filename = @"tmp.png";
            //Test = GetImage(filename, poly);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes starting values for Properties
        /// </summary>
        private void Initialize()
        {
            _context.Configuration.AutoDetectChangesEnabled = false;
            _context.Configuration.LazyLoadingEnabled = false;
            _context.Coordinates.Load();
            Coordinates = new ObservableCollection<Coordinate>();
            foreach (Coordinate c in _context.Coordinates.Local)
                Coordinates.Add(c);
            WaypointIndex = new ObservableCollection<int>();
            //for (int i = 1; i < 22; ++i)
            //    WaypointIndex.Add(i);
            Images = new ObservableCollection<ImageSource>();
            Title = GetTitle();
            OriginIndex = DestinationIndex = 0;
        }
        /// <summary>
        /// Runs through schedule mapping when called
        /// </summary>
        public void Run()
        {
            string response = GetServerResponse();
            string poly;
            dynamic obj = JsonConvert.DeserializeObject(response);
            poly = obj.routes[0].overview_polyline.points.Value;
            int imageIndex = Images.Count;
            string filename = "tmp" + imageIndex + ".png";
            Images.Add(GetImage(filename, poly));
        }
        /// <summary>
        /// Returns the title attribute of the executing assembly
        /// </summary>
        /// <returns>title <see cref="string"/></returns>
        private string GetTitle()
        {
            Assembly ex = Assembly.GetExecutingAssembly();
            Attribute att = Attribute.GetCustomAttribute(ex, typeof(AssemblyTitleAttribute), false);
            return ((AssemblyTitleAttribute)att)?.Title;
        }
        /// <summary>
        /// Queries the Directions Google API Server for directions between
        /// <see cref="Origin"/> and <see cref="Destination"/>.
        /// </summary>
        /// <returns>json-formatted <see cref="string"/></returns>
        private string GetServerResponse()
        {
            if (Coordinates.Count < 1)
                return "";

            string url = Set.UseJSON ? GetDirectionsURL_JSON() : GetDirectionsURL_XML();

            WebRequest request = WebRequest.Create(url);

            using (WebResponse response = request.GetResponse())
            {
                using (Stream data = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(data))
                    {
                        // json- or xml-formatted string from maps api
                        try { return reader.ReadToEnd(); }
                        catch (Exception) { return ""; }
                    }
                }
            }
        }
        /// <summary>
        /// Queries the Static Map Google API Server for a static map showing
        /// the directions obtained from <see cref="GetServerResponse"/> results
        /// </summary>
        /// <param name="filename">The filename to store the PNG image</param>
        /// <param name="line">
        /// The encoded polyline from the <see cref="GetServerResponse"/> results
        /// </param>
        /// <returns><see cref="BitmapImage"/> of PNG image</returns>
        private BitmapImage GetImage(string filename, string line)
        {
            string url = GetStaticMapURL(line);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            BitmapImage bmpImg = new BitmapImage();
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream strm = resp.GetResponseStream())
                {
                    try
                    {
                        Image.FromStream(strm).Save(filename, ImageFormat.Png);
                    }
                    catch (Exception) { return default(BitmapImage); }
                }
            }

            Uri u = new Uri(filename, UriKind.Relative);
            bmpImg.BeginInit();
            bmpImg.UriSource = u;
            bmpImg.EndInit();
            bmpImg.Freeze();
            return bmpImg;
        }
        /// <summary>
        /// Builds a URL to query for a json-formatted string
        /// </summary>
        /// <returns>URL <see cref="string"/></returns>
        private string GetDirectionsURL_JSON()
        {
            StringBuilder url_sb = new StringBuilder();
            string initial_url = @"https://maps.googleapis.com/maps/api/directions/json";
            string encodedKey = Properties.Resources.DirectionsKey;
            url_sb.Append(initial_url);
            url_sb.Append("?origin=");
            url_sb.Append(Origin);
            url_sb.Append("&destination=");
            url_sb.Append(Destination);
            if (WaypointIndex.Count > 0)
                url_sb.Append("&waypoints=");
            foreach(int i in WaypointIndex)
            {
                url_sb.Append(Coordinates[i].ToString());
                if (i != WaypointIndex.Last())
                    url_sb.Append("|");
            }
            url_sb.Append("&mode=");
            url_sb.Append("walking");
            url_sb.Append("&key=");
            url_sb.Append(Encoding.UTF8.GetString(Convert.FromBase64String(encodedKey)));
            return url_sb.ToString();
        }
        /// <summary>
        /// Builds a URL to query for an xml-formatted string
        /// </summary>
        /// <returns>URL <see cref="string"/></returns>
        private string GetDirectionsURL_XML()
        {
            StringBuilder url_sb = new StringBuilder();
            string initial_url = @"https://maps.googleapis.com/maps/api/directions/xml";
            string encodedKey = "QUl6YVN5QkRudnFEdTZDcEExano1cWlMTHFPaFRoay16a2xnMHBR";
            url_sb.Append(initial_url);
            url_sb.Append("?origin=");
            url_sb.Append(Origin);
            url_sb.Append("&destination=");
            url_sb.Append(Destination);
            if (WaypointIndex.Count > 0)
                url_sb.Append("&waypoints=");
            foreach (int i in WaypointIndex)
            {
                url_sb.Append(Coordinates[i].ToString());
                if (i != WaypointIndex.Last())
                    url_sb.Append("|");
            }
            url_sb.Append("&mode=");
            url_sb.Append("walking");
            url_sb.Append("&key=");
            url_sb.Append(Encoding.UTF8.GetString(Convert.FromBase64String(encodedKey)));
            return url_sb.ToString();
        }
        /// <summary>
        /// Builds a URL to query for a static PNG image
        /// </summary>
        /// <param name="line">
        /// The encoded polyline from the <see cref="GetServerResponse"/> results
        /// </param>
        /// <returns>URL <see cref="string"/></returns>
        private string GetStaticMapURL(string line)
        {
            StringBuilder url_sb = new StringBuilder();
            string init = @"https://maps.googleapis.com/maps/api/staticmap";
            url_sb.Append(init);
            url_sb.Append("?center=" + Properties.Resources.MapCenterX + "," +
                Properties.Resources.MapCenterY);
            url_sb.Append("&size=640x520");
            url_sb.Append("&zoom=16");
            url_sb.Append("&scale=2");
            url_sb.Append("&markers=");
            url_sb.Append(Origin.ToString() + "|" + Destination.ToString());
            if (WaypointIndex.Count > 0)
            {
                url_sb.Append("|");
                foreach (int i in WaypointIndex)
                {
                    url_sb.Append(Coordinates[i].ToString());
                    if (!Coordinates[i].Equals(Coordinates.Last()))
                        url_sb.Append("|");
                }
            }
            if (!string.IsNullOrWhiteSpace(line))
                url_sb.Append("&path=enc:" + WebUtility.UrlEncode(line));
            url_sb.Append("&key=");
            url_sb.Append(Encoding.UTF8.GetString(Convert
                .FromBase64String(Properties.Resources.DirectionsKey)));
            return url_sb.ToString();
        }
        #endregion Methods
    }
}
