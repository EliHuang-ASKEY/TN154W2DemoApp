using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class Location : Page
    {


        // Captures the value entered by user.
        private uint _desireAccuracyInMetersValue = 0;
        private CancellationTokenSource _cts = null;


        public Location()
        {
            this.InitializeComponent();
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached. The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetGeolocationButton.IsEnabled = true;
            CancelGetGeolocationButton.IsEnabled = false;
            DesiredAccuracyInMeters.IsEnabled = true;
        }

        /// <summary>
        /// Invoked immediately before the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <param name="e">
        /// Event data that can be examined by overriding code. The event data is representative
        /// of the navigation that will unload the current Page unless canceled. The
        /// navigation can potentially be canceled by setting e.Cancel to true.
        /// </param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// This is the click handler for the 'getGeolocation' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void GetGeolocationButtonClicked(object sender, RoutedEventArgs e)
        {
            GetGeolocationButton.IsEnabled = false;
            CancelGetGeolocationButton.IsEnabled = true;
            LocationDisabledMessage.Visibility = Visibility.Collapsed;

            try
            {
                // Request permission to access location
                var accessStatus = await Geolocator.RequestAccessAsync();

                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:

                        // Get cancellation token
                        _cts = new CancellationTokenSource();
                        CancellationToken token = _cts.Token;

                        //_rootPage.NotifyUser("Waiting for update...", NotifyType.StatusMessage);
                        LocationMessage.Text = "Waiting for update..."; // + MainPage.NotifyType.StatusMessage;
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Green);

                        // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                        Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = _desireAccuracyInMetersValue };

                        // Carry out the operation
                        Geoposition pos = await geolocator.GetGeopositionAsync().AsTask(token);

                        UpdateLocationData(pos);
                        //_rootPage.NotifyUser("Location updated.", NotifyType.StatusMessage);
                        LocationMessage.Text = "Location updated."; // + MainPage.NotifyType.StatusMessage;
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                        break;

                    case GeolocationAccessStatus.Denied:
                        // _rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                        LocationMessage.Text = "Access to location is denied."; // + MainPage.NotifyType.ErrorMessage;
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                        LocationDisabledMessage.Visibility = Visibility.Visible;
                        UpdateLocationData(null);
                        break;

                    case GeolocationAccessStatus.Unspecified:
                        //_rootPage.NotifyUser("Unspecified error.", NotifyType.ErrorMessage);
                        LocationMessage.Text = "Unspecified error."; // + MainPage.NotifyType.ErrorMessage;
                        ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                        UpdateLocationData(null);
                        break;
                }
            }
            catch (TaskCanceledException)
            {
                //_rootPage.NotifyUser("Canceled.", NotifyType.StatusMessage);
                LocationMessage.Text = "Canceled"; //+ MainPage.NotifyType.StatusMessage;
                ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            }
            catch (Exception ex)
            {
                //_rootPage.NotifyUser(ex.ToString(), NotifyType.ErrorMessage);
                LocationMessage.Text = "Unspecified error."; // + MainPage.NotifyType.ErrorMessage;
                ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            }
            finally
            {
                _cts = null;
            }

            GetGeolocationButton.IsEnabled = true;
            CancelGetGeolocationButton.IsEnabled = false;
        }

        /// <summary>
        /// This is the click handler for the 'CancelGetGeolocation' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelGetGeolocationButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }

            GetGeolocationButton.IsEnabled = true;
            CancelGetGeolocationButton.IsEnabled = false;
        }

        void DesiredAccuracyInMeters_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Update with the value entered by user
                _desireAccuracyInMetersValue = uint.Parse(DesiredAccuracyInMeters.Text);

                // Enable the button and clear out status message
                GetGeolocationButton.IsEnabled = true;
                //_rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                LocationMessage.Text = ""; // + MainPage.NotifyType.StatusMessage;
            }
            catch (ArgumentNullException)
            {
                GetGeolocationButton.IsEnabled = false;
            }
            catch (FormatException)
            {
                //_rootPage.NotifyUser("Desired Accuracy must be a number", NotifyType.ErrorMessage);
                LocationMessage.Text = "Desired Accuracy must be a number"; // + MainPage.NotifyType.ErrorMessage;
                ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                GetGeolocationButton.IsEnabled = false;
            }
            catch (OverflowException)
            {
                //_rootPage.NotifyUser("Desired Accuracy is out of bounds", NotifyType.ErrorMessage);
                LocationMessage.Text = "Desired Accuracy is out of bounds"; // + MainPage.NotifyType.ErrorMessage;
                ShowMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                GetGeolocationButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Updates the user interface with the Geoposition data provided
        /// </summary>
        /// <param name="position">Geoposition to display its details</param>
        private void UpdateLocationData(Geoposition position)
        {
            if (position == null)
            {
                ScenarioOutput_Latitude.Text = "No data";
                ScenarioOutput_Longitude.Text = "No data";
                ScenarioOutput_Accuracy.Text = "No data";
                ScenarioOutput_Source.Text = "No data";
                ShowSatalliteData(false);
            }
            else
            {
                ScenarioOutput_Latitude.Text = position.Coordinate.Point.Position.Latitude.ToString();
                ScenarioOutput_Longitude.Text = position.Coordinate.Point.Position.Longitude.ToString();
                ScenarioOutput_Accuracy.Text = position.Coordinate.Accuracy.ToString();
                ScenarioOutput_Source.Text = position.Coordinate.PositionSource.ToString();

                if (position.Coordinate.PositionSource == PositionSource.Satellite)
                {
                    // Show labels and satellite data when available
                    ScenarioOutput_PosPrecision.Text = position.Coordinate.SatelliteData.PositionDilutionOfPrecision.ToString();
                    ScenarioOutput_HorzPrecision.Text = position.Coordinate.SatelliteData.HorizontalDilutionOfPrecision.ToString();
                    //ScenarioOutput_VertPrecision.Text = position.Coordinate.SatelliteData.VerticalDilutionOfPrecision.ToString();
                    ScenarioOutput_VertPrecision.Text = position.Coordinate.SatelliteData.ToString();
                    ShowSatalliteData(true);


                }
                else
                {
                    // Hide labels and satellite data
                    ShowSatalliteData(false);
                }
            }
        }

        private void ShowSatalliteData(bool isVisible)
        {
            var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            ScenarioOutput_PosPrecision.Visibility = visibility;
            ScenarioOutput_HorzPrecision.Visibility = visibility;
            ScenarioOutput_VertPrecision.Visibility = visibility;
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
