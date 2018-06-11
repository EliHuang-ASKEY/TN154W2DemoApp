using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Power;
using Windows.System.Power;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.System;
using Windows.Devices.Gpio;
//using AS7000HRM;
using System.Diagnostics;
using Windows.Devices.I2c;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace IOTCoreMasterApp.DataModel
{
    public class AppDataModel : INotifyPropertyChanged
    {
        public static bool _SensorHub = true;
        private bool isloaded = false;
        //private Class1 _AS700HRM = null;
        private GpioPin _GpioPin;

        const string LISTEN_PORT = "2200";
        const string hostName = "192.168.1.246";
        IBuffer buffer;

        private string _currentStatusText = "Hallo";
        public string currentStatusText
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    return "Design Status";
                }
                else
                {
                    return _currentStatusText;
                }

            }
            set { _currentStatusText = value; OnPropertyChanged(); }
        }

        private string _batterySatus = "unknown";
        public string batteryStatus
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    return "Charging";
                }
                else
                {
                    return _batterySatus;
                }

            }
            set { _batterySatus = value; OnPropertyChanged(); }
        }

        private string _batteryPercent = "50%";
        public string batteryPercent
        {
            get
            {
                /*
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    return 30;
                }
                else
                {
                    return _batteryPercent;
                }
                */

                return _batteryPercent;

            }
            set { _batteryPercent = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AppListItem> _appList= new ObservableCollection<AppListItem>();
        public ObservableCollection<AppListItem> appList
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        AppListItem item = new AppListItem();
                        item.Name = "Name " + i;
                        _appList.Add(item);
                    }
                }
                return _appList;
            }
            set { _appList = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private CoreDispatcher dispatcher;

        public async void GetBatteryStatus()
        {
            var deviceInfo = await DeviceInformation.FindAllAsync(Battery.GetDeviceSelector());

            foreach (DeviceInformation device in deviceInfo)
            {
                try
                {
                    // Create battery object
                    var battery = await Battery.FromIdAsync(device.Id);

                    // Get report
                    var report = battery.GetReport();
                    batteryStatus = report.Status.ToString(); 



                }
                catch (Exception e) {
                    /* Add error handling, as applicable */
                }
            }

            batteryPercent = PowerManager.RemainingChargePercent.ToString()+"%";
            
            
        }

        private async void PowerManager_PowerSupplyStatusChanged(object sender, object e)
        {
            
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GetBatteryStatus();

            }); 
        }

        private async void PowerManager_RemainingChargePercentChanged(object sender, object e)
        {
            
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GetBatteryStatus();
            });
            //Debug.WriteLine("BatteryPercent" + PowerManager.RemainingChargePercent.ToString());
            //Battery Perenct < 5  device shutdown
            //if (PowerManager.RemainingChargePercent < 5)
                //ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0.5));
            
        }

        private async void PowerManager_RemainingDischargeTimeChanged(object sender, object e)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GetBatteryStatus(); //
            });
        }

        public async void LoadAppList()
        {
            checkSensorHub();
            await Task.Delay(800);


            if (isloaded == true) return;
            SetGPIO();



            var pm = new PackageManager();
            var packages = pm.FindPackagesForUserWithPackageTypes("", PackageTypes.Main);

            //Add local App 
            AppListItem litem = new AppListItem();
            litem.PackageFullName = "local:AppClock";
            //StorageFile file;
            Uri clocklogo = new Uri("ms-appx:///Assets/clock_44_n.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Clock"; 
            appList.Add(litem);
            

            litem = new AppListItem();
            litem.PackageFullName = "local:Setting";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/setting.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Setting";
            appList.Add(litem);

            //Add local App 
            litem = new AppListItem();
            litem.PackageFullName = "local:ConnectMainPackage";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/bluetooth.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "WiFi&BT";
            appList.Add(litem);

            
            litem = new AppListItem();
            litem.PackageFullName = "local:Flashlight";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/flashlight.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "FLASH";
            appList.Add(litem);
            

            litem = new AppListItem();
            litem.PackageFullName = "local:Camera2";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/camera.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Camera";
            appList.Add(litem);

            litem = new AppListItem();
            litem.PackageFullName = "local:AudioRecord";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/Microphone.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "AudioRecord";
            appList.Add(litem);

            litem = new AppListItem();
            litem.PackageFullName = "local:MediaPlayer";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/MediaPlayer_150.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "MediaPlayer";
            appList.Add(litem);

            if (!_SensorHub)
            {
                Debug.WriteLine("_SensorHub == false");

                litem = new AppListItem();
                litem.PackageFullName = "local:Accelerometer";
                //StorageFile file;
                clocklogo = new Uri("ms-appx:///Assets/accelerometer.png");
                litem.ImageSrc = new BitmapImage();
                litem.ImageSrc.UriSource = clocklogo;
                //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
                //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
                litem.Name = "Accelerometer";
                appList.Add(litem);


                litem = new AppListItem();
                litem.PackageFullName = "local:SensorGyrometer";
                //StorageFile file;
                clocklogo = new Uri("ms-appx:///Assets/gyrometer.png");
                litem.ImageSrc = new BitmapImage();
                litem.ImageSrc.UriSource = clocklogo;
                //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
                //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
                litem.Name = "SensorGyrometer";
                appList.Add(litem);

            }
            
            
            /*
            litem = new AppListItem();
            litem.PackageFullName = "local:SensorCompass";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/compass.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Compass";
            appList.Add(litem);
            */


            litem = new AppListItem();
            litem.PackageFullName = "local:Vibrator";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/vibrator.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Vibrator";
            appList.Add(litem);

            litem = new AppListItem();
            litem.PackageFullName = "local:NFCTest";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/nfc.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "NFC";
            appList.Add(litem);

            //linda 0511 add
            litem = new AppListItem();
            litem.PackageFullName = "local:WifiConnectFromFile";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/wifi.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "WifiConnectFromFile";
            appList.Add(litem);
            //

            litem = new AppListItem();
            litem.PackageFullName = "local:Location2";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/Location.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Location";
            appList.Add(litem);

            /*
            litem = new AppListItem();
            litem.PackageFullName = "local:Map";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/Map.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Map";
            appList.Add(litem);
            */
            

            litem = new AppListItem();
            litem.PackageFullName = "local:DeviceContrl";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/brightness.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Brightness";
            appList.Add(litem);

            litem = new AppListItem();
            litem.PackageFullName = "local:ShowMessage";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/barcode.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Message";
            appList.Add(litem);

            if (_AS7000)
            {
                litem = new AppListItem();
                litem.PackageFullName = "local:AS7000HRM";
                //StorageFile file;
                clocklogo = new Uri("ms-appx:///Assets/AS7000HRM.png");
                litem.ImageSrc = new BitmapImage();
                litem.ImageSrc.UriSource = clocklogo;
                //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
                //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
                litem.Name = "HRM";
                appList.Add(litem);
            }


            



            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            foreach (Package p in packages)
            {
                var applist = await p.GetAppListEntriesAsync();
                foreach (AppListEntry entry in applist)
                {
                    try
                    {
                        AppListItem item = new AppListItem();
                        item.Name = entry.DisplayInfo.DisplayName;
                        var logo = entry.DisplayInfo.GetLogo(new Windows.Foundation.Size(150.0, 150.0));
                        item.ImageSrc = new BitmapImage();
                        item.PackageFullName = p.Id.FullName;
                        item.AppEntry = entry;
                        IRandomAccessStreamWithContentType logostream = await logo.OpenReadAsync();
                        await item.ImageSrc.SetSourceAsync(logostream);
                        
                        if (item.Name != "Work or school account" &
                            item.Name != "Purchase Dialog" &
                            item.Name != "ZWaveHeadlessAdapterApp" &
                            item.Name != "IoTOnboardingTask" &
                            item.Name != "IoTUAPOOBE" &
                            item.Name != "ZWave Adapter Headless Host" &
                            item.Name != "Sign In" &
                            item.Name != "Email and accounts" &
                            item.Name != "Microsoft account" &
                            item.Name != "IOTCoreDefaultApplication" &
                            item.Name != "IOT Core Master Application" &
                            item.Name != "Search" &
                            item.Name != "Cortana Reminders" &
                            item.Name != "IoTShellExperienceHost" &
                            item.Name != "OnScreenKeyboard" &
                            item.Name != "Connect" &
                            (_SensorHub | item.Name != "BHI160Test"))    
                        {
                        
                            appList.Add(item);
                       }
                    }
                    catch {
                    }
                }
            }

           

            litem = new AppListItem();
            litem.PackageFullName = "local:About";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/information.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "Information";
            appList.Add(litem);

            /*
            litem = new AppListItem();
            litem.PackageFullName = "local:autotest";
            //StorageFile file;
            clocklogo = new Uri("ms-appx:///Assets/AutoTest.png");
            litem.ImageSrc = new BitmapImage();
            litem.ImageSrc.UriSource = clocklogo;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem.Name = "AutoTest";
            appList.Add(litem);
            */
            



            //Add local App 
            AppListItem litem2 = new AppListItem();
            litem2.PackageFullName = "local:Shutdown";
            //StorageFile file;
            Uri clocklogo2 = new Uri("ms-appx:///Assets/device-power-glyph.png");
            litem2.ImageSrc = new BitmapImage();
            litem2.ImageSrc.UriSource = clocklogo2;
            //albumArtCache[song.AlbumArtUri.ToString()] = litem.ImageSrc;
            //IRandomAccessStreamWithContentType Clocklogostream = await clocklogo.OpenReadAsync();
            litem2.Name = "Shutdown";
            appList.Add(litem2);


            GetBatteryStatus();
            PowerManager.RemainingDischargeTimeChanged += PowerManager_RemainingDischargeTimeChanged;
            PowerManager.RemainingChargePercentChanged += PowerManager_RemainingChargePercentChanged;
            PowerManager.PowerSupplyStatusChanged += PowerManager_PowerSupplyStatusChanged;
            
            
            isloaded = true; 
        }


        #region ProcessLauncher
        public async void _setTime(int hours, int minutes)
        {
            string s = "/SETACVALUEINDEX SCHEME_ALL SUB_VIDEO VIDEOIDLE  20";
            Debug.WriteLine("Run time"+s);
            await App.ViewModel.RunProcess("c:\\windows\\system32\\powercfg.exe", s);


        }

        
        public async Task RunProcess(string cmd, string args)
        {
            var options = new ProcessLauncherOptions();
            var standardOutput = new InMemoryRandomAccessStream();
            var standardError = new InMemoryRandomAccessStream();
            options.StandardOutput = standardOutput;
            options.StandardError = standardError;

            await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    var result = await ProcessLauncher.RunToCompletionAsync(cmd, args == null ? string.Empty : args, options);

                    //ProcessExitCode.Text += "Process Exit Code: " + result.ExitCode;

                    using (var outStreamRedirect = standardOutput.GetInputStreamAt(0))
                    {
                        var size = standardOutput.Size;
                        using (var dataReader = new DataReader(outStreamRedirect))
                        {
                            var bytesLoaded = await dataReader.LoadAsync((uint)size);
                            var stringRead = dataReader.ReadString(bytesLoaded);
                            Debug.WriteLine(cmd+"Output:"+ stringRead);
                            //StdOutputText.Text += stringRead;

                        }
                    }

                    using (var errStreamRedirect = standardError.GetInputStreamAt(0))
                    {
                        using (var dataReader = new DataReader(errStreamRedirect))
                        {
                            var size = standardError.Size;
                            var bytesLoaded = await dataReader.LoadAsync((uint)size);
                            var stringRead = dataReader.ReadString(bytesLoaded);
                            Debug.WriteLine(cmd + "Error:" + stringRead);
                            //StdErrorText.Text += stringRead;
                        }
                    }
                    
                }
                catch (UnauthorizedAccessException uex)
                {
                }
                catch (Exception ex)
                {
                }
            });
            
        }
        #endregion ProcessLauncher


        private void SetGPIO()
        {
            var gpio = GpioController.GetDefault();
            const int AS7000GpioPin = 8;
            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                
                Debug.WriteLine("There is no GPIO controller on this device.");
                return;
            }
            _GpioPin = gpio.OpenPin(AS7000GpioPin);
            _GpioPin.Write(GpioPinValue.Low);
            _GpioPin.SetDriveMode(GpioPinDriveMode.Output);
            Debug.WriteLine("GPIO 8 Low");
            StopGpio();
            checkAS7000();
        }

        private void StopGpio()
        {
            if (_GpioPin != null)
            {
                _GpioPin.Dispose();
                _GpioPin = null;
            }
        }
        private const byte PORT_EXPANDER_I2C_ADDRESS = 0x30;
        private I2cDevice i2cPortExpander, i2cPortSensorHub;
        private bool _AS7000 = true;
        
        private async void checkAS7000()
        {
            
            
            byte[] i2CReadBuffer;
            

            var i2cSettings = new I2cConnectionSettings(PORT_EXPANDER_I2C_ADDRESS);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            //var controller = await I2cController.GetDefaultAsync();
            string deviceSelector = I2cDevice.GetDeviceSelector("I2C6");
            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            i2cPortExpander = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);

            Debug.WriteLine("i2cDeviceControllers = "+ i2cDeviceControllers.Count);
            if(i2cPortExpander == null) Debug.WriteLine("i2cPortExpander Null ");
            else { 
                
                try
                {
                
                // initialize local copies of the IODIR, GPIO, and OLAT registers
                    i2CReadBuffer = new byte[1];

                // read in each register value on register at a time (could do this all at once but
                // for example clarity purposes we do it this way)
                    i2cPortExpander.WriteRead(new byte[] { 0x00 }, i2CReadBuffer);
                    Debug.WriteLine("iodirRegister" + i2CReadBuffer[0]);
                    /*
                    if (i2CReadBuffer[0] == 2) _AS7000 &= true;
                    i2cPortExpander.WriteRead(new byte[] { 0x01 }, i2CReadBuffer);
                    Debug.WriteLine(" iodirRegister" + i2CReadBuffer[0]);
                    if (i2CReadBuffer[0] == 0) _AS7000 &= true;
                    i2cPortExpander.WriteRead(new byte[] { 0x02 }, i2CReadBuffer);
                    Debug.WriteLine(" iodirRegister" + i2CReadBuffer[0]);
                    if (i2CReadBuffer[0] == 77) _AS7000 &= true;
                    i2cPortExpander.WriteRead(new byte[] { 0x03 }, i2CReadBuffer);
                    Debug.WriteLine(" iodirRegister" + i2CReadBuffer[0]);
                    if (i2CReadBuffer[0] == 22) _AS7000 &= true;
                    */
                    //i2cPortExpander.Read(i2CReadBuffer);


                    


                }
                catch (Exception e)
                {
                //ButtonStatusText.Text = "Failed to initialize I2C port expander: " + e.Message;
                    Debug.WriteLine("Failed to initialize I2C port expander");
                    _AS7000 = false;
                    
                }
                
            }
            i2cPortExpander.Dispose();
        }


        public async void checkSensorHub()
        {


            byte[] i2CReadBuffer;


            var i2cSettings = new I2cConnectionSettings(0x28);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            //var controller = await I2cController.GetDefaultAsync();
            string deviceSelector = I2cDevice.GetDeviceSelector("I2C1");
            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            i2cPortSensorHub = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);

            Debug.WriteLine("i2cDeviceControllers = " + i2cDeviceControllers.Count);
            if (i2cPortSensorHub == null) Debug.WriteLine("i2cPortSensorHub Null ");

            else
            {

                try
                {
                    // initialize local copies of the IODIR, GPIO, and OLAT registers
                    i2CReadBuffer = new byte[1];

                    // read in each register value on register at a time (could do this all at once but
                    // for example clarity purposes we do it this way)
                    i2cPortSensorHub.WriteRead(new byte[] { 0x90 }, i2CReadBuffer);
                    Debug.WriteLine("iodirRegister" + i2CReadBuffer[0]);
 

                }
                catch (Exception e)
                {
                    //ButtonStatusText.Text = "Failed to initialize I2C port expander: " + e.Message;
                    Debug.WriteLine("Failed to initialize SensorHub I2C port expander");
                    _SensorHub = false;
                    Debug.WriteLine("_SensorHub should be false:"+ _SensorHub);
                }

            }
            
            i2cPortSensorHub.Dispose();
        }


        /*
        private int checkAS7000HRM()
        {
            _AS700HRM = new Class1();

            int i = 99;
               i = _AS700HRM.AS7000_Init();
            Debug.WriteLine("Return 995 ");
            //if (i==1) Debug.WriteLine("Return 1 ");
            //else Debug.WriteLine("Return 0 ");
            Debug.WriteLine("Return  "+i);
            return 0;

        }
        */

        public async void readResultFile(string TestCase)
        {
            Debug.WriteLine("readResultFileStart!");
            string testItem = TestCase;
            Debug.WriteLine("TestCase" + TestCase);
            switch (TestCase)
            {
                case "TestVibrator":
                    testItem += ".txt";
                    break;
                case "TestAccelerometer":
                    testItem += ".txt";
                    break;
                case "TestGyrometer":
                    testItem += ".txt";
                    break;
                case "TestAS7000HRM":
                    testItem += ".txt";
                    break;

            }



            StorageFolder storageFolder;
            StorageFile sampleFile;




            if (testItem == "TestHeartRate.txt")
            {
                storageFolder = KnownFolders.DocumentsLibrary;
                var Documentfile = await storageFolder.TryGetItemAsync(testItem);
                if (Documentfile == null)
                {
                    return;
                }
                else
                {
                    sampleFile = await storageFolder.GetFileAsync(testItem);
                }
            }
            else
            {
                var file = await KnownFolders.VideosLibrary.TryGetItemAsync(testItem);


                if (file != null)
                {
                    storageFolder = KnownFolders.VideosLibrary;
                    sampleFile = await storageFolder.GetFileAsync(testItem);

                }
                else return;
            }








            Debug.WriteLine("StartTCPSend:" + testItem + "\n");



            if (sampleFile != null)
            {
                using (IRandomAccessStream stream = await sampleFile.OpenReadAsync())
                {

                    using (DataReader reader = new DataReader(stream.GetInputStreamAt(0UL)))
                    {
                        uint len = (uint)stream.Size;
                        // 載入資料
                        await reader.LoadAsync(len);
                        buffer = reader.ReadBuffer(len);
                        // 暫存在Tag屬性中，稍後用到
                        //this.Tag = buffer;
                    }
                }
            }

            //buffer = this.Tag as IBuffer;
            sendFileTCP(buffer, testItem);


        }
        private async void sendFileTCP(IBuffer buffer, string TestCase)
        {
            Debug.WriteLine("Entry_sendFileTCP!");
            using (StreamSocket socket = new StreamSocket())
            {
                try
                {
                    // 發起連線
                    await socket.ConnectAsync(new HostName(hostName), LISTEN_PORT);
                    Debug.WriteLine("connect status:" + socket.Information);
                    // 準備傳送資料
                    using (DataWriter writer = new DataWriter(socket.OutputStream))
                    {
                        // 首先寫入圖形資料
                        uint len = buffer.Length;
                        writer.WriteUInt32(len); //長度
                        writer.WriteBuffer(buffer);
                        // 接著寫入文字

                        len = writer.MeasureString(TestCase);
                        writer.WriteUInt32(len); //長度
                        writer.WriteString(TestCase);

                        // 傳送資料到流
                        await writer.StoreAsync();
                        Debug.WriteLine("CloseTCPSend\n");
                    }

                    socket.Dispose();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(TestCase + ": TCP " + ex.Message);
                }
            }
            /*
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(TestCase);

            if (file != null)
            {
                await file.DeleteAsync();
                Debug.WriteLine("Delete file :" + TestCase + "\n");
                return;
            }
            */
            if (TestCase == "TestHeartRate.txt")
            {
                StorageFolder storageFolder = KnownFolders.DocumentsLibrary;
                var Documnentfile = await storageFolder.TryGetItemAsync(TestCase);

                if (Documnentfile != null)
                {
                    try
                    {
                        StorageFile sampleFile = await storageFolder.GetFileAsync(TestCase);
                        await sampleFile.DeleteAsync();
                        Debug.WriteLine("file :" + TestCase + "\nDelete file successful");

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Can not find file :" + TestCase + "\n TO Delete fail");
                        return;
                    }
                }
            }


        }
    }
}
