using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.Devices.Sensors;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.System.Threading;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    /// 
    public sealed partial class Accelerometer2 : Page
    {


        private Windows.Devices.Sensors.Accelerometer _accelerometer;
        private uint _desiredReportInterval;
        int timetick=1000;
        private ThreadPoolTimer Atimer;

        public Accelerometer2()
        {
            this.InitializeComponent();

            _accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                // Select a report interval that is both suitable for the purposes of the app and supported by the sensor.
                // This value will be used later to activate the sensor.
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                _desiredReportInterval = minReportInterval > 16 ? minReportInterval : 16;
            }
            else
            {
                //rootPage.NotifyUser("No accelerometer found", NotifyType.ErrorMessage);
                //MessageBlock.Text = "No accelerometer found";

            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ScenarioEnableButton.IsEnabled = true;
            ScenarioDisableButton.IsEnabled = false;
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ScenarioDisableButton.IsEnabled)
            {
                Window.Current.VisibilityChanged -= new WindowVisibilityChangedEventHandler(VisibilityChanged);
                _accelerometer.ReadingChanged -= new TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

                // Restore the default report interval to release resources while the sensor is not in use
                _accelerometer.ReportInterval = 0;
            }

            if (Atimer != null)
            {
                Atimer.Cancel();
            }
            base.OnNavigatingFrom(e);
        }


        private void VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (ScenarioDisableButton.IsEnabled)
            {
                if (e.Visible)
                {
                    // Re-enable sensor input (no need to restore the desired reportInterval... it is restored for us upon app resume)
                    _accelerometer.ReadingChanged += new TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
                }
                else
                {
                    // Disable sensor input (no need to restore the default reportInterval... resources will be released upon app suspension)
                    _accelerometer.ReadingChanged -= new TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
                }
            }
        }


        async private void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AccelerometerReading reading = e.Reading;
                ScenarioOutput_X.Text = String.Format("{0,5:0.00}", reading.AccelerationX);
                ScenarioOutput_Y.Text = String.Format("{0,5:0.00}", reading.AccelerationY);
                ScenarioOutput_Z.Text = String.Format("{0,5:0.00}", reading.AccelerationZ);
            });

            _accelerometer.ReportInterval = 0;
            _accelerometer.ReadingChanged -= new Windows.Foundation.TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

        }
      private void Accelerometer_Timer_Tick(ThreadPoolTimer timer)
        {

            //Accelerometer event
            uint minReportIntervalMsecs = _accelerometer.MinimumReportInterval;
            _accelerometer.ReportInterval = minReportIntervalMsecs > 16 ? minReportIntervalMsecs : 16;
            _accelerometer.ReadingChanged += new Windows.Foundation.TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
        }


        private void ScenarioEnable(object sender, RoutedEventArgs e)
        {

           

            Atimer = ThreadPoolTimer.CreatePeriodicTimer(Accelerometer_Timer_Tick, TimeSpan.FromMilliseconds(timetick));


                combox.IsEnabled = false;
                ScenarioEnableButton.IsEnabled = false;
                ScenarioDisableButton.IsEnabled = true;

        }


        private void ScenarioDisable(object sender, RoutedEventArgs e)
        {
            Atimer.Cancel();

            Window.Current.VisibilityChanged -= new WindowVisibilityChangedEventHandler(VisibilityChanged);
            _accelerometer.ReadingChanged -= new TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

            // Restore the default report interval to release resources while the sensor is not in use
            _accelerometer.ReportInterval = 0;

            ScenarioEnableButton.IsEnabled = true;
            ScenarioDisableButton.IsEnabled = false;
            combox.IsEnabled = true;
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(Atimer!= null)
            {
                Atimer.Cancel();
            }
            

            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private  async void combox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combox == null) return;
            var combo = (ComboBox)sender;
            var item = (ComboBoxItem)combo.SelectedItem;
            switch (item.Content.ToString())
            {
                case "0.1":
                    timetick = 100;
                    break;
                case "0.5":
                    timetick = 500;
                    break;
                case "1":
                    timetick = 1000;
                    break;
                case "2":
                    timetick = 2000;
                    break;
                case "3":
                    timetick = 3000;
                    break;
                case "5":
                    timetick = 5000;
                    break;
                case "10":
                    timetick = 10000;
                    break;
                case "30":
                    timetick = 30000;
                    break;
                case "60":
                    timetick = 60000;
                    break;
                case "120":
                    timetick = 120000;
                    break;


            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MessageBlockState.Text = "State: Selected time is " + item.Content.ToString();
            });

            

        }
    }
}
