using IOTCoreMasterApp.DataModel;
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
using Windows.ApplicationModel.Core;
using IOTCoreMasterApp.LocalApps;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using Windows.Devices.Gpio;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media.Core;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IOTCoreMasterApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //private ObservableCollection<AppListItem> _items;
        private const int BUTTON_PIN = 92;
        private GpioPin buttonPin;
        private bool wavPlayer = true;
        private Windows.Media.Playback.MediaPlayer _mediaPlayer = new Windows.Media.Playback.MediaPlayer();

        private const int Flash_PIN = 112;
        private GpioPin flashPin112;
        private GpioOpenStatus openStatus;

        public MainPage()
        {
            this.InitializeComponent();
            App.ViewModel.LoadAppList();
            Debug.WriteLine("Main Test1= " + this.wavPlayer);
            InitGPIO112();
            //Windows.System.Power.PowerManager.RemainingChargePercentChanged += PowerManager_RemainingChargePercentChanged;
            batteryPercent.SizeChanged += PowerManager_RemainingChargePercentChanged;
            
        }
        private void PowerManager_RemainingChargePercentChanged(object sender, object e)
        {

           
            Debug.WriteLine("BatteryPercentMainXaml" + Windows.System.Power.PowerManager.RemainingChargePercent.ToString());
            if (Windows.System.Power.PowerManager.RemainingChargePercent < 8)
                this.Frame.Navigate(typeof(AutoShutdown));
            //Battery Perenct < 5  device shutdown
            //if (Windows.System.Power.PowerManager.RemainingChargePercent < 5)
            //ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0.5));

        }




        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
           
            

        }
        
 
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StopGpio();
            var pageName = this.Frame.Content.GetType().ToString();
            if (pageName != "IOTCoreMasterApp.LocalApps.Camera2")
                InitGPIO();
            

            //Debug.WriteLine("Name= "+ this.Frame.Content.GetType());

            if (wavPlayer)
            {
                CheckSoundEffect();
                this.wavPlayer = false;
            }


        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            
            
        }
        

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            
            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                //status.Text = "There is no GPIO controller on this device.";
                return;
            }

            buttonPin = gpio.OpenPin(BUTTON_PIN);



            // Check if input pull-up resistors are supported
            if (buttonPin.IsDriveModeSupported(Windows.Devices.Gpio.GpioPinDriveMode.InputPullUp))
                buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                buttonPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonPin.ValueChanged += buttonPin_ValueChanged;



        }

        private void InitGPIO112()
        {
            Debug.WriteLine("Main: InitGPIO112");
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                
                Debug.WriteLine("Flash: There is no GPIO controller on this device.");
                return;
            }

            try {
                gpio.TryOpenPin(Flash_PIN, GpioSharingMode.Exclusive, out flashPin112, out openStatus);
                if (openStatus == 0)
                {
                    Debug.WriteLine("Flash: GPIO pins 112 value" + flashPin112.Read().ToString() + flashPin112.GetDriveMode().ToString());
                    flashPin112.SetDriveMode(GpioPinDriveMode.Output);
                    flashPin112.Write(GpioPinValue.Low);

                    //status.Text = "GPIO pins initialized correctly. OPEN GPIO 112 successful\n";
                    Debug.WriteLine("Flash: GPIO pins 112 initialized correctly");
                }

                else
                {
                    Debug.WriteLine("Flash: GPIO pins 112 value" + flashPin112.Read().ToString() + flashPin112.GetDriveMode().ToString());

                    //status.Text += "OPEN GPIO 112 fail\n";
                    Debug.WriteLine("GPIO pins initialized fail. OPEN GPIO 112 fail\n");
                    //toggleSwitch_FLASH112.IsEnabled = false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("GPIO pins initialized fail. OPEN GPIO 112 fail\n");
            }


        }


        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            
            //Debug.WriteLine("Name5= ");

            
            // need to invoke UI updates on the UI thread because this event
            // handler gets invoked on a separate thread.
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (e.Edge == GpioPinEdge.FallingEdge)
                {
                    //ledEllipse.Fill = (ledPinValue == GpioPinValue.Low) ? 
                    //takePhoto();
                    //status.Text = "Button Pressed";
                    
                    //Debug.WriteLine("Name6");

                    StopGpio();
                    this.Frame.Navigate(typeof(Camera2));
                    

                }
                else
                {

                    //status.Text = "Button Released";
                }
            });
        }
        private void StopGpio()
        {
            if (buttonPin != null)
            {
                buttonPin.Dispose();
                buttonPin = null;
            }

        }



        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AppListItem item = (AppListItem)appListControl.SelectedItem;
            if (item!=null)
            {
                if (item.PackageFullName.Contains("AppClock"))
                {
                    this.Frame.Navigate(typeof(AppClock));
                    
                }
                else if (item.PackageFullName.Contains("ConnectMainPackage"))
                {
                    this.Frame.Navigate(typeof(ConnectMainPackage));
                }
                else if (item.PackageFullName.Contains("Shutdown"))
                {
                    this.Frame.Navigate(typeof(Shutdown));
                }
                else if (item.PackageFullName.Contains("Camera2"))
                {
                    StopGpio();
                    this.Frame.Navigate(typeof(Camera2));
                }
                else if (item.PackageFullName.Contains("AudioRecord"))
                {
                    this.Frame.Navigate(typeof(AudioRecord));
                }
                else if (item.PackageFullName.Contains("MediaPlayer"))
                {
                    this.Frame.Navigate(typeof(MediaPlayer));
                }
                else if (item.PackageFullName.Contains("Accelerometer"))
                {
                    this.Frame.Navigate(typeof(Accelerometer));
                }
                
                else if (item.PackageFullName.Contains("SensorCompass"))
                {
                    this.Frame.Navigate(typeof(SensorCompass));
                }
                
                else if (item.PackageFullName.Contains("SensorGyrometer"))
                {
                    this.Frame.Navigate(typeof(SensorGyrometer));
                }
                else if (item.PackageFullName.Contains("Vibrator"))
                {
                    this.Frame.Navigate(typeof(Vibrator));
                }
                else if (item.PackageFullName.Contains("NFCTest"))
                {
                    this.Frame.Navigate(typeof(NFCTest));
                }
                else if (item.PackageFullName.Contains("Location2"))
                {
                    this.Frame.Navigate(typeof(Location2));
                }
                else if (item.PackageFullName.Contains("Setting"))
                {
                    this.Frame.Navigate(typeof(Setting));
                }
                else if (item.PackageFullName.Contains("DeviceContrl"))
                {
                    this.Frame.Navigate(typeof(DeviceContrl));
                }
                else if (item.PackageFullName.Contains("ShowMessage"))
                {
                    this.Frame.Navigate(typeof(ShowMessage));
                }
                else if (item.PackageFullName.Contains("About"))
                {
                    this.Frame.Navigate(typeof(about));
                }
                else if (item.PackageFullName.Contains("Flashlight"))
                {
                    if(openStatus ==0)
                        this.Frame.Navigate(typeof(FlashLight), flashPin112);
                    else
                        this.Frame.Navigate(typeof(FlashLight), null);
                }
                else
                {
                    await item.AppEntry.LaunchAsync();
                }
                //Unable to cast object of type 'Windows.ApplicationModel.Package' to type 'Windows.ApplicationModel.IPackageWithMetadata'.
            }
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };


        private async void CheckSoundEffect()
        {

            string text;
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync("soundeffects.txt");
            if (item == null)
                text = "True";
            else
            {
                text = await PathIO.ReadTextAsync("ms-appdata:///local/SoundEffects.txt");
            }
            


            if (text == "True") SoundEffect();
            

        }

        private void SoundEffect()
        {
            

                this.mediaPlayerAudio.SetMediaPlayer(_mediaPlayer);
                this.mediaPlayerAudio.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Danzon_De_Pasion_Sting.mp3"));
                this._mediaPlayer = mediaPlayerAudio.MediaPlayer;
                this._mediaPlayer.Volume = 1f;
                this._mediaPlayer.Play();
                
            
        }


    }
}
