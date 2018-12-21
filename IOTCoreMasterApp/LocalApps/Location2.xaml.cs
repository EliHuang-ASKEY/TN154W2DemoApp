using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.UI.Core;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class Location2 : Page
    {
        // Proides access to location data
        private Geolocator _geolocator = null;
        public Location2()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StartTrackingButton.IsEnabled = true;
            StopTrackingButton.IsEnabled = false;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (StopTrackingButton.IsEnabled)
            {
                _geolocator.PositionChanged -= OnPositionChanged;
                _geolocator.StatusChanged -= OnStatusChanged;
            }

            base.OnNavigatingFrom(e);
        }

        private async void StartTracking(object sender, RoutedEventArgs e)
        {
            // Request permission to access location
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    // You should set MovementThreshold for distance-based tracking
                    // or ReportInterval for periodic-based tracking before adding event
                    // handlers. If none is set, a ReportInterval of 1 second is used
                    // as a default and a position will be returned every 1 second.
                    //
                    // Value of 2000 milliseconds (2 seconds) 
                    // isn't a requirement, it is just an example.
#if false  // Ammore Modify - position update issue
                    // We will get position data every 30 sec if we set more than 2 second. 
                    _geolocator = new Geolocator { ReportInterval = 2000 };
#else
                    _geolocator = new Geolocator { ReportInterval = 1000 };
#endif
                    _geolocator.DesiredAccuracy = PositionAccuracy.Default;
                    // Subscribe to PositionChanged event to get updated tracking positions
                    _geolocator.PositionChanged += OnPositionChanged;

                    // Subscribe to StatusChanged event to get updates of location status changes
                    _geolocator.StatusChanged += OnStatusChanged;

                    //_rootPage.NotifyUser("Waiting for update...", NotifyType.StatusMessage);
                    LocationMessage.Text = "Waiting for update...";
                    ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    LocationDisabledMessage.Visibility = Visibility.Collapsed;
                    StartTrackingButton.IsEnabled = false;
                    StopTrackingButton.IsEnabled = true;
                    break;

                case GeolocationAccessStatus.Denied:
                    //_rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                    LocationMessage.Text = "Access to location is denied.";
                    ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    LocationDisabledMessage.Visibility = Visibility.Visible;
                    break;

                case GeolocationAccessStatus.Unspecified:
                    //_rootPage.NotifyUser("Unspecificed error!", NotifyType.ErrorMessage);
                    LocationMessage.Text = "Unspecificed error!";
                    ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    LocationDisabledMessage.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void StopTracking(object sender, RoutedEventArgs e)
        {
            _geolocator.PositionChanged -= OnPositionChanged;
            _geolocator.StatusChanged -= OnStatusChanged;
            _geolocator = null;

            StartTrackingButton.IsEnabled = true;
            StopTrackingButton.IsEnabled = false;

            // Clear status
            // _rootPage.NotifyUser("", NotifyType.StatusMessage);
        }

        async private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //_rootPage.NotifyUser("Location updated.", NotifyType.StatusMessage);
                LocationMessage.Text = "Location updated.";
                ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                UpdateLocationData(e.Position);
            });
        }


        async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Show the location setting message only if status is disabled.
                LocationDisabledMessage.Visibility = Visibility.Collapsed;

                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        // Location platform is providing valid data.
                        ScenarioOutput_Status.Text = "Ready";
                        //_rootPage.NotifyUser("Location platform is ready.", NotifyType.StatusMessage);
                        LocationMessage.Text = "Location platform is ready.";
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                        break;

                    case PositionStatus.Initializing:
                        // Location platform is attempting to acquire a fix. 
                        ScenarioOutput_Status.Text = "Initializing";
                        //_rootPage.NotifyUser("Location platform is attempting to obtain a position.", NotifyType.StatusMessage);
                        LocationMessage.Text = "Location platform is attempting to obtain a position.";
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;

                    case PositionStatus.NoData:
                        // Location platform could not obtain location data.
                        ScenarioOutput_Status.Text = "No data";
                        //_rootPage.NotifyUser("Not able to determine the location.", NotifyType.ErrorMessage);
                        LocationMessage.Text = "Not able to determine the location.";
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;

                    case PositionStatus.Disabled:
                        // The permission to access location data is denied by the user or other policies.
                        ScenarioOutput_Status.Text = "Disabled";
                        //_rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                        LocationMessage.Text = "Access to location is denied.";
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);

                        // Show message to the user to go to location settings
                        LocationDisabledMessage.Visibility = Visibility.Visible;

                        // Clear cached location data if any
                        UpdateLocationData(null);
                        break;

                    case PositionStatus.NotInitialized:
                        // The location platform is not initialized. This indicates that the application 
                        // has not made a request for location data.
                        ScenarioOutput_Status.Text = "Not initialized";
                        //_rootPage.NotifyUser("No request for location is made yet.", NotifyType.StatusMessage);
                        LocationMessage.Text = "No request for location is made yet.";
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;

                    case PositionStatus.NotAvailable:
                        // The location platform is not available on this version of the OS.
                        ScenarioOutput_Status.Text = "Not available";
                        //_rootPage.NotifyUser("Location is not available on this version of the OS.", NotifyType.ErrorMessage);
                        LocationMessage.Text = "Location is not available on this version of the OS.";
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;

                    default:
                        ScenarioOutput_Status.Text = "Unknown";
                        //_rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                        break;
                }
            });
        }

        private void UpdateLocationData(Geoposition position)
        {
            if (position == null)
            {
                ScenarioOutput_Latitude.Text = "No data";
                ScenarioOutput_Longitude.Text = "No data";
                ScenarioOutput_Accuracy.Text = "No data";
                ScenarioOutput_Source.Text = "No data";
            }
            else
            {
                ScenarioOutput_Latitude.Text = position.Coordinate.Point.Position.Latitude.ToString();
                ScenarioOutput_Longitude.Text = position.Coordinate.Point.Position.Longitude.ToString();
                ScenarioOutput_Accuracy.Text = position.Coordinate.Accuracy.ToString();
                ScenarioOutput_Source.Text = position.Coordinate.PositionSource.ToString();

                
            }
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
