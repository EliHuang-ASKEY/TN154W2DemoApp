using IOTCoreMasterApp.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Sensors;
using Windows.System.Threading;
using Windows.Devices.Haptics;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;



// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class autotest : Page
    {
        private MediaCapture mediaCapture;
        private StorageFile photoFile;


        private bool isPreviewing;
        private bool isRecording;
        private bool isCapturing;

        //private bool _singleControlMode;
        private readonly SystemNavigationManager _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
        private DisplayOrientations _displayOrientation = DisplayOrientations.Portrait;

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");


        private DispatcherTimer timerProgress = new DispatcherTimer();

        double x = 0;


        //add for sensor init s

        private Windows.Devices.Sensors.Accelerometer _accelerometer;
        private uint _desiredReportInterval;
        private Gyrometer _gyrometer;
        private DataModel.TestLog Alog,Glog,Vlog,Hlog;
        private int TestCount=0,TestRound=1;
        private int TestMaxCount = 0;
        private int TestMaxRound = 0;
        private ThreadPoolTimer Atimer,Gtimer,Vtimer,Ctimer,Ftimer,Htimer;

        //add for sensor init e

        //add for Vibrate init s
        private VibrationDevice VibrationDevice;
        private SimpleHapticsControllerFeedback BuzzFeedback;
        //add for Vibrate init e

        //add for flash init s
        private const int Flash_PIN = 112;
        private GpioPin flashPin112;
        private GpioOpenStatus openStatus;
        private GpioController gpio;
        private bool gpioValue;
        //add for flash init e

        //add for Biosensor init s
        private GpioPin _GpioPin;
        const int AS7000GpioPin = 8;
        const int AS7000LedGpioPin = 93;

        private const byte PORT_EXPANDER_I2C_ADDRESS = 0x30;
        private I2cDevice i2cPortExpander;
        private bool _AS7000 = true;
        //add for Biosensor init e







        public autotest()
        {
            this.InitializeComponent();
            //InitCamera();
            InitVibrator();         
            Initsensor();
            InitGPIO112();
            PhotoButton.IsEnabled = false;
            //Ftimer = ThreadPoolTimer.CreatePeriodicTimer(Flash_Timer_Tick, TimeSpan.FromMilliseconds(5000));
            //Vtimer = ThreadPoolTimer.CreatePeriodicTimer(Vibrator_Timer_Tick, TimeSpan.FromMilliseconds(7000));
            //Ctimer = ThreadPoolTimer.CreatePeriodicTimer(Camera_Timer_Tick, TimeSpan.FromMilliseconds(4000));
            //Gtimer = ThreadPoolTimer.CreatePeriodicTimer(Gyrometer_Timer_Tick, TimeSpan.FromMilliseconds(2000));
            //Htimer = ThreadPoolTimer.CreatePeriodicTimer(AS7000HRM_Timer_Tick, TimeSpan.FromMilliseconds(2000));
            //Atimer = ThreadPoolTimer.CreatePeriodicTimer(Accelerometer_Timer_Tick, TimeSpan.FromMilliseconds(2000));
            //InitBioSensor(TestMaxCount);


            isRecording = false;
            isPreviewing = false;
            isCapturing = false;

            this.ManipulationMode = ManipulationModes.TranslateY;            //设置这个页面的手势模式为豎向滑动  
            this.ManipulationCompleted += The_ManipulationCompleted;         //订阅手势滑动结束后的事件   
            this.ManipulationDelta += The_ManipulationDelta;
        }

        //手势滑动中  
        private void The_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            x += e.Delta.Translation.Y;     //将滑动的值赋给x   
        }


        //手势滑动结束  
        private void The_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (x > 100)                    //判断滑动的距离  
                MySplit.IsPaneOpen = true;    //打开汉堡菜单  
            if (x < -100)
                MySplit.IsPaneOpen = false;   //关闭汉堡菜单  
            x = 0;  //清零x，不然x会累加  
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Cleanup();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private async void InitCamera()
        {
            try
            {
                if (mediaCapture != null)
                {
                    // Cleanup MediaCapture object
                    if (isPreviewing)
                    {
                        await mediaCapture.StopPreviewAsync();
                        isPreviewing = false;
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                }


                // Use default initialization
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                // Start Preview                
                previewElement.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;



            }
            catch (Exception ex)
            {

            }
        }

        private void Initsensor()
        {
            //Accelerometer 相關
            _accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();

            //Gyromete 相關
            _gyrometer = Gyrometer.GetDefault();

            //Create log file
           Alog = new TestLog("TestAccelerometer.txt");
           Glog = new TestLog("TestGyrometer.txt");
           Hlog = new TestLog("TestAS7000HRM.txt");

        }

        private async void InitVibrator()
        {
            if (VibrationDevice == null)
            {
                await GetVbrationDevice();
            }

            Vlog = new TestLog("TestVibrator.txt");
        }

        private void InitGPIO112()
        {

            //Debug.WriteLine("Main: InitGPIO112"+value);
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {

                Debug.WriteLine("Flash: There is no GPIO controller on this device.");
                return;
            }

            try
            {
                gpio.TryOpenPin(Flash_PIN, GpioSharingMode.Exclusive, out flashPin112, out openStatus);
                
                if (openStatus == 0)
                {
                    Debug.WriteLine("Flash: GPIO pins 112 value1" + flashPin112.Read().ToString() + flashPin112.GetDriveMode().ToString());
                    flashPin112.SetDriveMode(GpioPinDriveMode.Output);
                    if (gpioValue)
                    {
                        flashPin112.Write(GpioPinValue.High);

                    }
                    else
                    {

                        flashPin112.Write(GpioPinValue.Low);

                    }
                  
                    Debug.WriteLine("Flash: GPIO pins 112 value2" + flashPin112.Read().ToString() + flashPin112.GetDriveMode().ToString());
                    //status.Text = "GPIO pins initialized correctly. OPEN GPIO 112 successful\n";
                    Debug.WriteLine("Flash: GPIO pins 112 initialized correctly");
                    //InitGPIO();
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
     
       
        private  async void InitBioSensor(int count)
        {

            byte[] i2CReadBuffer;
            byte[] i2CWriteBuffer;     
            string sMessage = "";


            var i2cSettings = new I2cConnectionSettings(PORT_EXPANDER_I2C_ADDRESS);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            //var controller = await I2cController.GetDefaultAsync();
            string deviceSelector = I2cDevice.GetDeviceSelector("I2C6");
            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            i2cPortExpander = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);

            Debug.WriteLine("HRM_info:i2cDeviceControllers = " + i2cDeviceControllers.Count);
            if (i2cPortExpander == null) Debug.WriteLine("i2cPortExpander Null ");
            else
            {
                try
                {

                    // initialize local copies of the IODIR, GPIO, and OLAT registers
                    i2CReadBuffer = new byte[1];

                    // read in each register value on register at a time (could do this all at once but
                    // for example clarity purposes we do it this way)
                    i2cPortExpander.WriteRead(new byte[] { 0x00 }, i2CReadBuffer);
                    Debug.WriteLine("HRM_info:iodirRegister" + i2CReadBuffer[0]);
                    if (i2CReadBuffer[0] == 2) _AS7000 &= true;
                    i2cPortExpander.WriteRead(new byte[] { 0x01 }, i2CReadBuffer);
                    Debug.WriteLine("HRM_info:iodirRegister" + i2CReadBuffer[0]);
                    if (i2CReadBuffer[0] == 0) _AS7000 &= true;
                    i2cPortExpander.WriteRead(new byte[] { 0x02 }, i2CReadBuffer);
                    Debug.WriteLine("HRM_info:iodirRegister" + i2CReadBuffer[0]);
                    if (i2CReadBuffer[0] == 77) _AS7000 &= true;
                    i2cPortExpander.WriteRead(new byte[] { 0x03 }, i2CReadBuffer);
                    Debug.WriteLine("HRM_info:iodirRegister" + i2CReadBuffer[0]);
                    if (i2CReadBuffer[0] == 22) _AS7000 &= true;


                    i2CWriteBuffer = new byte[] { 0x04, Convert.ToByte(2) };
                    i2cPortExpander.Write(i2CWriteBuffer);
                    await Task.Delay(10000);

                    for (int i=0;i<count;i++)
                    {
                        TestCount++;

                        i2cPortExpander.WriteRead(new byte[] { 0x04 }, i2CReadBuffer);
                        Debug.WriteLine("HRM_info:iodirRegister" + i2CReadBuffer[0]);
                        string temp;
                        int TIA = 0;

                        await Task.Delay(1000);
                        i2cPortExpander.WriteRead(new byte[] { 0x1e }, i2CReadBuffer);
                        temp = i2CReadBuffer[0].ToString() + "00";
                        Debug.WriteLine("HRM_info:iodirRegister" + i2CReadBuffer[0]);
                        i2cPortExpander.WriteRead(new byte[] { 0x1f }, i2CReadBuffer);
                        Debug.WriteLine(i2CReadBuffer[0]);
                        TIA = Convert.ToInt32(temp) + Convert.ToInt32(i2CReadBuffer[0]);

                        sMessage = "";
                        sMessage += "Round" + TestRound + "->" + TestCount + ":";
                        sMessage += "HRM" + TIA;
                        sMessage += "\r\n";

                        Hlog.WriteText(sMessage);

                    }
                    

                    i2CWriteBuffer = new byte[] { 0x04, Convert.ToByte(0) };
                    i2cPortExpander.Write(i2CWriteBuffer);
                    //i2cPortExpander.Read(i2CReadBuffer);
                }
                catch (Exception e)
                {
                    //ButtonStatusText.Text = "Failed to initialize I2C port expander: " + e.Message;
                    Debug.WriteLine("HRM_info:Failed to initialize I2C port expander");
                    _AS7000 = false;

                }
            }

            i2cPortExpander.Dispose();

            if (TestCount >= TestMaxCount)
            {
                StopGpio();
                TestCount = 0;               

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    previewElement.Visibility = Visibility.Visible;
                   InitCamera();
                });
                
                await Task.Delay(2000);
                Ctimer = ThreadPoolTimer.CreatePeriodicTimer(Camera_Timer_Tick, TimeSpan.FromMilliseconds(4000));
            }

        }
        

        private void StopGpio()
        {
            if (_GpioPin != null)
            {
                _GpioPin.Dispose();
                _GpioPin = null;
            }
        }

        private void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            TestCount++;

            AccelerometerReading reading = e.Reading;

            string sMessage = "";

            /*
            if (reading.AccelerationX > 0 && reading.AccelerationX < 0.1 && reading.AccelerationY > -0.02 && reading.AccelerationY < 0.05 && reading.AccelerationZ > -1.01 && reading.AccelerationZ < -0.93)
            {
                sMessage = "";
                sMessage += "Round" + TestRound + "->" + TestCount + ":";
                sMessage += string.Format(" Accelerometer X({0,5:0.00})", reading.AccelerationX);
                sMessage += string.Format(" Y({0,5:0.00})", reading.AccelerationY);
                sMessage += string.Format(" Z({0,5:0.00})", reading.AccelerationZ);
                sMessage += "\r\n";

                Alog.WriteText(sMessage);

            }
            else
            {
                sMessage = "";
                sMessage += "Round" + TestRound + "->" + TestCount + ":";
                sMessage += "Accelerometer Failed.";
                sMessage += "\r\n";

                Alog.WriteText(sMessage);
            }
            */

            sMessage += "Round" + TestRound + "->" + TestCount + ":";
            sMessage += string.Format(" Accelerometer X({0,5:0.00})", reading.AccelerationX);
            sMessage += string.Format(" Y({0,5:0.00})", reading.AccelerationY);
            sMessage += string.Format(" Z({0,5:0.00})", reading.AccelerationZ);
            sMessage += "\r\n";

            Alog.WriteText(sMessage);


            _accelerometer.ReportInterval = 0;
            _accelerometer.ReadingChanged -= new Windows.Foundation.TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

            if (TestCount >= TestMaxCount)
            {
                Atimer.Cancel();
                TestCount = 0;         
                Ftimer = ThreadPoolTimer.CreatePeriodicTimer(Flash_Timer_Tick, TimeSpan.FromMilliseconds(5000));
            }

        }

        private void ReadingChanged(object sender, GyrometerReadingChangedEventArgs e)
        {
            TestCount++;

            GyrometerReading reading = e.Reading;

            string sMessage = "";

            if (reading.AngularVelocityX > -311 && reading.AngularVelocityX < 575 && reading.AngularVelocityY > -621 && reading.AngularVelocityY < 615 && reading.AngularVelocityZ > -513 && reading.AngularVelocityZ < 494)
            {
                sMessage = "";
                sMessage += "Round" + TestRound + "->" + TestCount + ":";
                sMessage += string.Format(" Gyrometer X({0,5:0.00})", reading.AngularVelocityX);
                sMessage += string.Format(" Y({0,5:0.00})", reading.AngularVelocityY);
                sMessage += string.Format(" Z({0,5:0.00})", reading.AngularVelocityZ);
                sMessage += "\r\n";

                Glog.WriteText(sMessage);
            }
            else
            {
                sMessage = "";
                sMessage += "Round" + TestRound + "->" + TestCount + ":";
                sMessage += "Gyrometer Failed.";
                sMessage += "\r\n";

                Glog.WriteText(sMessage);
            }

               
            _gyrometer.ReportInterval = 0;
            _gyrometer.ReadingChanged -= ReadingChanged;

            if (TestCount >= TestMaxCount)
            {
                Gtimer.Cancel();
                TestCount = 0;
                Atimer = ThreadPoolTimer.CreatePeriodicTimer(Accelerometer_Timer_Tick, TimeSpan.FromMilliseconds(2000));
            }

            
            
        }

        private void ReadingChangedforVibrator(object sender, AccelerometerReadingChangedEventArgs e)
        {
            
            AccelerometerReading reading = e.Reading;

            string sMessage = "";
    

            if(Math.Abs(reading.AccelerationX-0.09) > 0.07 || Math.Abs(reading.AccelerationY-0.01) > 0.05 || Math.Abs(Math.Abs(reading.AccelerationZ)-0.96) > 0.4)
            {
                sMessage = "";
                sMessage += "Round" + TestRound + "->" + TestCount + ":VibrationDevice Success.";
                sMessage += "\r\n";
                Vlog.WriteText(sMessage);
            }
            else
            {
                sMessage = "";
                sMessage += "Round" + TestRound + "->" + TestCount + ":VibrationDevice Failed.";
                sMessage += "\r\n";
                Vlog.WriteText(sMessage);
            }


            _accelerometer.ReportInterval = 0;
            _accelerometer.ReadingChanged -= new Windows.Foundation.TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChangedforVibrator);



            if (TestCount >= TestMaxCount)
            {
                Vtimer.Cancel();
                TestCount = 0;
                Gtimer = ThreadPoolTimer.CreatePeriodicTimer(Gyrometer_Timer_Tick, TimeSpan.FromMilliseconds(2000));
            }


        }

        private void Accelerometer_Timer_Tick(ThreadPoolTimer timer)
        {

            //Accelerometer event
            uint minReportIntervalMsecs = _accelerometer.MinimumReportInterval;
            _accelerometer.ReportInterval = minReportIntervalMsecs > 16 ? minReportIntervalMsecs : 16;
            _accelerometer.ReadingChanged += new Windows.Foundation.TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
        }

        private void Gyrometer_Timer_Tick(ThreadPoolTimer timer)
        {
            //Gyrometer event
            _gyrometer.ReportInterval = Math.Max(_gyrometer.MinimumReportInterval, 16);
            _gyrometer.ReadingChanged += ReadingChanged;
        }

        private void Vibrator_Timer_Tick(ThreadPoolTimer timer)
        {
            TestCount++;
            string sMessage = "";

            if (VibrationDevice == null)
            {
                sMessage += "Round" + TestRound + "->" + TestCount + ":VibrationDevice is null.\r\n";
                Vlog.WriteText(sMessage);          
                return;
            }

            VibrationDevice.SimpleHapticsController.SendHapticFeedbackForDuration(BuzzFeedback, 1, TimeSpan.FromMilliseconds(4000));

            //Use Accelerometer value to check vibrator have vibrated or not 
            uint minReportIntervalMsecs = _accelerometer.MinimumReportInterval;
            _accelerometer.ReportInterval = minReportIntervalMsecs > 16 ? minReportIntervalMsecs : 16;
            _accelerometer.ReadingChanged += new Windows.Foundation.TypedEventHandler<Windows.Devices.Sensors.Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChangedforVibrator);

        }

        private  async void Camera_Timer_Tick(ThreadPoolTimer timer)
        {
            TestCount++;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                takePhoto();          
            });

            //App.ViewModel.readResultFile("TestCamera");


        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Vtimer = ThreadPoolTimer.CreatePeriodicTimer(Vibrator_Timer_Tick, TimeSpan.FromMilliseconds(7000));
            PhotoButton.IsEnabled = true;
            combox.IsEnabled = false;
            combox2.IsEnabled = false;
            MySplit.IsPaneOpen = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {     

            if (Atimer != null)
            {
                Atimer.Cancel();
            }
            if (Gtimer != null)
            {
                Gtimer.Cancel();
            }
            if (Vtimer != null)
            {
                Vtimer.Cancel();
            }
            if (Ctimer != null)
            {
                Ctimer.Cancel();
            }
            if (Ftimer != null)
            {
                Ftimer.Cancel();
            }
            if (Htimer != null)
            {
                Htimer.Cancel();
            }

            combox.IsEnabled = true;
            combox2.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
              
        }

        private void combox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combox == null) return;
            var combo = (ComboBox)sender;
            var item = (ComboBoxItem)combo.SelectedItem;
            switch (item.Content.ToString())
            {

                case "1":
                    TestMaxCount = 1;
                    break;
                case "2":
                    TestMaxCount = 2;
                    break;
                case "3":
                    TestMaxCount = 3;
                    break;
                case "5":
                    TestMaxCount = 5;
                    break;
                case "10":
                    TestMaxCount = 10;
                    break;
                case "20":
                    TestMaxCount = 20;
                    break;
                case "30":
                    TestMaxCount = 30;
                    break;

            }

        }

        private void combox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combox2 == null) return;
            var combo = (ComboBox)sender;
            var item = (ComboBoxItem)combo.SelectedItem;
            switch (item.Content.ToString())
            {

                case "1":
                    TestMaxRound = 1;
                    break;
                case "2":
                    TestMaxRound = 2;
                    break;
                case "3":
                    TestMaxRound = 3;
                    break;
                case "5":
                    TestMaxRound = 5;
                    break;
                case "10":
                    TestMaxRound = 10;
                    break;
                case "20":
                    TestMaxRound = 20;
                    break;
                case "30":
                    TestMaxRound = 30;
                    break;

            }

        }

        private void SPTap(object sender, TappedRoutedEventArgs e)
        {
            CompleteMessage.IsOpen = false;
        }

        private async void Flash_Timer_Tick(ThreadPoolTimer timer)
        {
            TestCount++;

            flashPin112.Write(GpioPinValue.High);
            await Task.Delay(3000);
            flashPin112.Write(GpioPinValue.Low);

            if (TestCount >= TestMaxCount)
            {
                Ftimer.Cancel();
                TestCount = 0;
                Htimer = ThreadPoolTimer.CreatePeriodicTimer(AS7000HRM_Timer_Tick, TimeSpan.FromMilliseconds(2000));
            }


        }

        private void AS7000HRM_Timer_Tick(ThreadPoolTimer timer)
        {
            Htimer.Cancel();
            InitBioSensor(TestMaxCount);
        }



        private async void Cleanup()
        {
            if (mediaCapture != null)
            {
                // Cleanup MediaCapture object
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                    captureImage.Source = null;        
                    //playbackElement.Source = null;
                    isPreviewing = false;
                }

                mediaCapture.Dispose();
                mediaCapture = null;
            }

        }



        private async void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    //status.Text = "MediaCaptureFailed: " + currentFailure.Message;

                    if (isRecording)
                    {
                        await mediaCapture.StopRecordAsync();
                        //status.Text += "\n Recording Stopped";
                    }
                }
                catch (Exception)
                {
                }
                finally
                {

                    //status.Text += "\nCheck if camera is diconnected. Try re-launching the app";
                }
            });
        }

        private async Task GetVbrationDevice()
        {
            if (await VibrationDevice.RequestAccessAsync() != VibrationAccessStatus.Allowed)
            {
                Vlog.WriteText("VibrationAccessStatus failed.");
                return;
            }

            VibrationDevice = await VibrationDevice.GetDefaultAsync();

            if (VibrationDevice != null)
            {
                BuzzFeedback = FindFeedback();
            }
        }

        private SimpleHapticsControllerFeedback FindFeedback()
        {
            foreach (var f in VibrationDevice.SimpleHapticsController.SupportedFeedback)
            {
                if (f.Waveform == KnownSimpleHapticsControllerWaveforms.BuzzContinuous)
                    return f;
            }
            return null;
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
          
            if (this.Frame.CanGoBack)
            {
                if (Atimer != null)
                {
                    Atimer.Cancel();
                }
                if (Gtimer != null)
                {
                    Gtimer.Cancel();
                }
                if (Vtimer != null)
                {
                    Vtimer.Cancel();
                }
                if (Ctimer != null)
                {
                    Ctimer.Cancel();
                }
                if (Ftimer != null)
                {
                    Ftimer.Cancel();
                }
                if (Htimer != null)
                {
                    Htimer.Cancel();
                }

                this.Frame.GoBack();
                
            }
        }

        private void PhotoButton_Click(object sender, RoutedEventArgs e)
        {
            if(mediaCapture != null)
            {
                takePhoto();
            }
                
        }

        private async void sendresultlog()
        {
            App.ViewModel.readResultFile("TestVibrator");
            await Task.Delay(3000);
            App.ViewModel.readResultFile("TestAccelerometer");
            await Task.Delay(3000);
            App.ViewModel.readResultFile("TestGyrometer");
            await Task.Delay(3000);
            App.ViewModel.readResultFile("TestAS7000HRM");
            await Task.Delay(3000);
            CompleteMessage.IsOpen = true;
            PhotoButton.IsEnabled = false;
        }

        private async void takePhoto()
        {

            this.isCapturing = true;

            this.PhotoButton.IsEnabled = false;
            var stream = new InMemoryRandomAccessStream();

            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            string[] separators = { ".", ":" };

            string[] time = DateTime.Now.TimeOfDay.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            //string PHOTO_FILE_NAME = "Photo-" + DateTime.Now.ToString("yyyy-MM-dd-") + time[0] + time[1] + time[2] + ".jpg";
             string PHOTO_FILE_NAME = "Round" + TestRound + "_Photo_" + TestCount +".jpg";

            try
            {
                captureImage.Source = null;

                photoFile = await KnownFolders.VideosLibrary.CreateFileAsync(
                    PHOTO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                //ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8);
                await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

                App.ViewModel.readResultFile(PHOTO_FILE_NAME);
                await Task.Delay(2000);

                //照片旋轉
                //await ReencodeAndSavePhotoAsync(stream, photoFile, PhotoOrientation.Rotate270);


                IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(photoStream);
                captureImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                Cleanup();
            }
            finally
            {

            }
            this.isCapturing = false;
            Debug.WriteLine("isCapturing" + isCapturing);
            this.PhotoButton.IsEnabled = true;




            if (TestCount >= TestMaxCount)
            {
                Ctimer.Cancel();

                if (TestRound != TestMaxRound)
                {
                    TestRound++;
                    TestCount = 0;

                    await mediaCapture.StopPreviewAsync();
                    mediaCapture.Dispose();
                    mediaCapture = null;
                    Vtimer = ThreadPoolTimer.CreatePeriodicTimer(Vibrator_Timer_Tick, TimeSpan.FromMilliseconds(7000));
                }
                else
                {
                    TestCount = 0;
                    TestRound = 1;
                    await mediaCapture.StopPreviewAsync();
                    mediaCapture.Dispose();
                    mediaCapture = null;
                    previewElement.Visibility =Visibility.Collapsed ;
                    btnStart.IsEnabled = true;
                    btnStop.IsEnabled = false;
                    combox.IsEnabled = true;
                    combox2.IsEnabled = true;
                    sendresultlog();
                }

            }
        }




    }
}
