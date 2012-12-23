using Bing.Maps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Popups;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Xplore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public CustomPin pin;

        public MainPage()
        {
            this.InitializeComponent();
            pin = new CustomPin();
            NavMaps.Children.Add(pin);
            SettingsPane.GetForCurrentView().CommandsRequested += OnSettingsPaneCommandRequested;
        
        }
        private void OnSettingsPaneCommandRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            // Add the commands one by one to the settings panel
            args.Request.ApplicationCommands.Add(new SettingsCommand("Privacy Statement", "Privacy Statement", DoOperation));
         
            args.Request.ApplicationCommands.Add(new SettingsCommand("a", " Feedback and Support",dO2 ));
        }
       
        private async void DoOperation(IUICommand command)
        {
            Uri uri = new Uri("http://navx.webs.com/windowsstoreapp.htm");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
        private  void dO2(IUICommand command)
        {
           about.IsOpen = true;
           about.FlyoutWidth = 600;
            about.Width=200;
           about.Heading = "navaneeth.a@msn.com";
            
        


        }
        

        void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
           
        }
        Geolocator geolocator;
        Pushpin MyPushPin;
        Location location;
        string URL;
        double FromLatitude, FromLongitude;
        private void rdLocation_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                grdCoord.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                grdLocation.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            catch (Exception)
            {
                return;
            }
        }

        private void rdCoord_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                grdLocation.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                grdCoord.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            catch (Exception)
            {
                return;
            }
        }

        private async void btnGiveDirections_Click(object sender, RoutedEventArgs e)
        {

          tbError.Text = string.Empty;
            if (rdCoord.IsChecked == true)
                URL = "http://dev.virtualearth.net/REST/V1/Routes/Driving?o=json&wp.0=" + txtFromCoord.Text + "&wp.1=" + txtToCoord.Text + "&optmz=distance&rpo=Points&key=" + NavMaps.Credentials;
            else
                URL = "http://dev.virtualearth.net/REST/V1/Routes/Driving?o=json&wp.0=" + txtFromLocation.Text + "&wp.1=" + txtToLocation.Text + "&optmz=distance&rpo=Points&key=" + NavMaps.Credentials;
            Uri geocodeRequest = new Uri(URL);
            BingMapsRESTService.Response r = await GetResponse(geocodeRequest);

            geolocator = new Geolocator();
            MyPushPin = new Pushpin();

            FromLatitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[0][0];
            FromLongitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[0][1];

            location = new Location(FromLatitude, FromLongitude);
            MapLayer.SetPosition(MyPushPin, location);
            NavMaps.Children.Add(MyPushPin);
            NavMaps.SetView(location, 15.0f);

            MapPolyline routeLine = new MapPolyline();
            routeLine.Locations = new LocationCollection();
            routeLine.Color = Windows.UI.Colors.Blue;
            routeLine.Width = 5.0;
            // Retrieve the route points that define the shape of the route.
            int bound = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates.GetUpperBound(0);
            for (int i = 0; i < bound; i++)
            {
                routeLine.Locations.Add(new Location
                {
                    Latitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[i][0],
                    Longitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[i][1]
                });
            }
            MapShapeLayer shapeLayer = new MapShapeLayer();
            shapeLayer.Shapes.Add(routeLine);
            NavMaps.ShapeLayers.Add(shapeLayer);
        }



        private async Task<BingMapsRESTService.Response> GetResponse(Uri uri)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            var response = await client.GetAsync(uri);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BingMapsRESTService.Response));
                return ser.ReadObject(stream) as BingMapsRESTService.Response;
            }
        }


        public async void OnFindClick(object sender, RoutedEventArgs e)
        {
            Geolocator geolocator = new Geolocator();
            var pos = await geolocator.GetGeopositionAsync(TimeSpan.FromDays(10), TimeSpan.FromHours(1));
            Location location = new Location(pos.Coordinate.Latitude, pos.Coordinate.Longitude);

            //Center map view on current location.
            MapLayer.SetPosition(pin, location);
            NavMaps.SetView(location, 15.0f);

            txtFromCoord.Text = pos.Coordinate.Latitude+","+pos.Coordinate.Longitude;
            
        }


        public Geolocator GeoLocator { get; set; }

        private async void OnMapTapped(object sender, TappedRoutedEventArgs e)
        {
            if (txtFromCoord.Text == "")
            {
                String loc = txtFromCoord.Text;
                var pos = e.GetPosition(NavMaps);
                Location location;
                NavMaps.TryPixelToLocation(pos, out location);

                MapLayer.SetPosition(Push, location);
                NavMaps.SetView(location);
                txtFromCoord.Text = Convert.ToString(location.Latitude) + "," + location.Longitude;
              
          



            }
            else
            {
            
                var pos = e.GetPosition(NavMaps);
                Location location;
                NavMaps.TryPixelToLocation(pos, out location);

                MapLayer.SetPosition(Push2, location);
                NavMaps.SetView(location);
                txtToCoord.Text = Convert.ToString(location.Latitude) + "," + location.Longitude;
                URL = "http://dev.virtualearth.net/REST/V1/Routes/Driving?o=json&wp.0=" + txtFromCoord.Text + "&wp.1=" + txtToCoord.Text + "&optmz=distance&rpo=Points&key=" + NavMaps.Credentials;
                Uri geocodeRequest = new Uri(URL);
                BingMapsRESTService.Response r = await GetResponse(geocodeRequest);

                geolocator = new Geolocator();
                MyPushPin = new Pushpin();

                FromLatitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[0][0];
                FromLongitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[0][1];

                location = new Location(FromLatitude, FromLongitude);
                MapLayer.SetPosition(MyPushPin, location);
                NavMaps.Children.Add(MyPushPin);
                NavMaps.SetView(location, 15.0f);

                MapPolyline routeLine = new MapPolyline();
                routeLine.Locations = new LocationCollection();
                routeLine.Color = Windows.UI.Colors.Blue;
                routeLine.Width = 5.0;
                // Retrieve the route points that define the shape of the route.
                int bound = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates.GetUpperBound(0);
                for (int i = 0; i < bound; i++)
                {
                    routeLine.Locations.Add(new Location
                    {
                        Latitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[i][0],
                        Longitude = ((BingMapsRESTService.Route)(r.ResourceSets[0].Resources[0])).RoutePath.Line.Coordinates[i][1]
                    });
                }
                MapShapeLayer shapeLayer = new MapShapeLayer();
                shapeLayer.Shapes.Add(routeLine);
                NavMaps.ShapeLayers.Add(shapeLayer);
                txtFromCoord.Text = "";
                txtToCoord.Text = "";
            }
       
        }

        private void OnHelpTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Help));
        }

      

    }
}