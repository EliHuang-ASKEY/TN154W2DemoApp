using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
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
    public sealed partial class Shutdown : Page
    {
        private DispatcherTimer timerProgress = new DispatcherTimer();
        private int MaxTime = 0;
        private int _Time = 20;
        private bool _shutdown = false;
        private bool _reboot = false;


        private BrightnessOverride shbo;
        private double brightnessLevel;

        public Shutdown()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SPstate.Visibility = Visibility.Collapsed;
            this.timerProgress.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timerProgress.Tick += UpdateTime;



            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Graphics.Display.BrightnessOverride"))
            {

                try
                {
                    //Create BrightnessOverride object
                    shbo = BrightnessOverride.GetDefaultForSystem();

                    shbo.StartOverride();

                    brightnessLevel = shbo.GetLevelForScenario(DisplayBrightnessScenario.DefaultBrightness) * 100;
                    


                }
                catch (Exception ex)
                {
                    //sErrMessage = ex.Message;
                }
            }


        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (this.timerProgress.IsEnabled)
            {
                this.timerProgress.Stop();
                timerProgress.Tick -= UpdateTime;
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

        private void btnShutdown_Click(object sender, RoutedEventArgs e)
        {
            _shutdown = true;
            state.Text = "Shutdown";
            SPstate.Visibility = Visibility.Visible;
            SPshutdown.Visibility = Visibility.Collapsed;
            
            //shbo.SetBrightnessLevel(_Time/100, DisplayBrightnessOverrideOptions.None);
            this.timerProgress.Start();
            
        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            _reboot = true;
            state.Text = "Rebooting";
            SPstate.Visibility = Visibility.Visible;
            SPshutdown.Visibility = Visibility.Collapsed;
            
            //shbo.SetBrightnessLevel(_Time / 100, DisplayBrightnessOverrideOptions.None);
            this.timerProgress.Start();
            
           
        }


        private void UpdateTime(object sender, object e)
        {

            _Time = _Time-2;
            state.Text += ".";
            
            //shbo.SetBrightnessLevel(_Time / 100, DisplayBrightnessOverrideOptions.None);
            if (_Time > MaxTime) return;


            try
            {

                this.timerProgress.Stop();
                timerProgress.Tick -= UpdateTime;
                
                //shbo.SetBrightnessLevel(0, DisplayBrightnessOverrideOptions.None);
                //shbo.StopOverride();
                if (_shutdown)
                    ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0.5));
                if(_reboot)
                    ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0.5));
            }
            catch (Exception ex)
            {

                Debug.WriteLine("UpdateTime Error :" + ex.Message);
            }


        }
    }
}
