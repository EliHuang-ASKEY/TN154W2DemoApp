using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class Map : Page
    {
        public Map()
        {
            this.InitializeComponent();
            BasicGeoposition snPosition = new BasicGeoposition() { Latitude = 47.620, Longitude = -122.349 };
            Geopoint snPoint = new Geopoint(snPosition);

            // Center the map over the POI.
            myMap.Center = snPoint;
            myMap.ZoomLevel = 12;
        }

        private async void Locat_Click(object sender, RoutedEventArgs e)
        {
            tablePanel.Visibility = Visibility.Collapsed;
            PopupResetSPanel.Visibility = Visibility.Visible;

            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync();
                Geolocator geolocator = new Geolocator { ReportInterval = 2000 };
                Geoposition pos = await geolocator.GetGeopositionAsync();

                if(pos != null)
                {
                    BasicGeoposition loact = new BasicGeoposition() { Latitude = pos.Coordinate.Point.Position.Latitude, Longitude = pos.Coordinate.Point.Position.Longitude };
                    Geopoint locatPoint = new Geopoint(loact);

                    // Create a MapIcon.
                    var mapIcon1 = new MapIcon
                    {
                        Location = locatPoint,
                        NormalizedAnchorPoint = new Point(0.5, 1.0),
                        Title = "You are Here",
                        ZIndex = 0,
                    };


                    // Add the MapIcon to the map.
                    myMap.MapElements.Add(mapIcon1);
                    // Center the map over the POI.
                    myMap.Center = locatPoint;
                    myMap.ZoomLevel = 14;

                    PopupResetSPanel.Visibility = Visibility.Collapsed;
                    Lat.Text = pos.Coordinate.Point.Position.Latitude.ToString();
                    Lon.Text = pos.Coordinate.Point.Position.Longitude.ToString();
                    Sou.Text = pos.Coordinate.PositionSource.ToString();
                    Acc.Text = pos.Coordinate.Accuracy.ToString();

                }
                
            }
            catch(Exception ex)
            {
                PopupResetSPanel.Visibility = Visibility.Collapsed;
                PopupfailSPanel.Visibility = Visibility.Visible;
            }
            


        }
        private void SPTap(object sender, TappedRoutedEventArgs e)
        {
            PopupfailSPanel.Visibility = Visibility.Collapsed;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void Detail_Click(object sender, RoutedEventArgs e)
        {     

            if (tablePanel.Visibility == Visibility.Visible)
            {
                tablePanel.Visibility = Visibility.Collapsed;
                return;
            }

            tablePanel.Visibility = Visibility.Visible;

        }
    }
}
