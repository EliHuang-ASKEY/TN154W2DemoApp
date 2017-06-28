using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Haptics;
using System.Threading.Tasks;
// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class Vibrator : Page
    {

        VibrationDevice VibrationDevice;
        SimpleHapticsControllerFeedback BuzzFeedback;

        private int TimeMillis = 10000;

        private bool checkedAlready = false;

        public Vibrator()
        {
            this.InitializeComponent();
        }

        private async Task GetVbrationDevice()
        {
            if (await VibrationDevice.RequestAccessAsync() != VibrationAccessStatus.Allowed)
            {
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
        private async void VibratorEnable(object sender, RoutedEventArgs e)
        {
            if (VibrationDevice != null)
            {
                VibrationDevice.SimpleHapticsController.SendHapticFeedbackForDuration(BuzzFeedback, 1, TimeSpan.FromMilliseconds(TimeMillis));
            }
            else
            {
                if (!checkedAlready)
                    await GetVbrationDevice();

                checkedAlready = true;

                if (VibrationDevice != null)
                {
                    VibrationDevice.SimpleHapticsController.SendHapticFeedbackForDuration(BuzzFeedback, 1, TimeSpan.FromMilliseconds(TimeMillis));

                }

            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (VibrationDevice != null)
            {
                VibrationDevice.SimpleHapticsController.StopFeedback();
            }
            else
            {
                if (!checkedAlready)
                    await GetVbrationDevice();

                if (VibrationDevice == null)
                {
                    checkedAlready = true;
                }
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
