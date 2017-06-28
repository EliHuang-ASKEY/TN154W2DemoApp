using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AutoShutdown : Page
    {
        private DispatcherTimer timerProgress = new DispatcherTimer();
        private int MaxTime = 0;
        private int _Time = 5;

        public AutoShutdown()
        {
            this.InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.timerProgress.Interval = new TimeSpan(0, 0, 0, 1, 0);
            timerProgress.Tick += UpdateTime;
            this.timerProgress.Start();

        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (this.timerProgress.IsEnabled)
            {
                this.timerProgress.Stop();
                timerProgress.Tick -= UpdateTime;
            }

        }


        private void UpdateTime(object sender, object e)
        {

            _Time = _Time - 1;

            
            if (_Time > MaxTime) return;


            try
            {

                this.timerProgress.Stop();
                timerProgress.Tick -= UpdateTime;
                System.Diagnostics.Debug.WriteLine("shuntdown :" );
                ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0.5));

            }
            catch (Exception ex)
            {

                //Debug.WriteLine("UpdateTime Error :" + ex.Message);
            }


        }

    }
}
