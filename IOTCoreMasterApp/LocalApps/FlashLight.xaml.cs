using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class FlashLight : Page
    {
        
        private const int Flash_PIN = 112;
        private GpioPin flashPin112;
        private GpioOpenStatus openStatus;
        private GpioController gpio;

        public FlashLight()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {

                flashPin112 = e.Parameter as GpioPin;
                InitGPIO();
            }
            else {
                Debug.WriteLine("Flash: Parameter Error");
            }
            //StopGpio();



        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //StopGpio();



        }


       



        private void InitGPIO()
        {
            
                Debug.WriteLine("Flash: GPIO pins 112 value"+ flashPin112.Read().ToString()+flashPin112.GetDriveMode().ToString());
            if (flashPin112.GetDriveMode().ToString() == "Output" & flashPin112.Read().ToString() == "High")
            {
                toggleSwitch_FLASH112.IsOn = true;
            }
            else
            {
                toggleSwitch_FLASH112.IsOn = false;
            }
            
                
                

          
        }
        
        
        private void StopGpio()
        {

            if (flashPin112 != null)
            {
                flashPin112.Write(GpioPinValue.Low);
                flashPin112.Dispose();
                flashPin112 = null;
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

        private void toggleSwitch_FLASH112_Toggled(object sender, RoutedEventArgs e)
        {
            if (toggleSwitch_FLASH112.IsOn)
            {
                flashPin112.Write(GpioPinValue.High);
                Debug.WriteLine("Flash112: ON \n" + flashPin112.Read().ToString());
                status.Text = "Flash112: ON ";
            }
            else
            {
                flashPin112.Write(GpioPinValue.Low);
                Debug.WriteLine("Flash112: OFF \n"+ flashPin112.Read().ToString());
                status.Text = "Flash112: OFF ";
            }

        }
    }
}
