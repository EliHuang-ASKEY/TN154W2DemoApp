using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class SetTime : Page
    {
        public struct SYSTEMTIME
        {
            public ushort wYear, wMonth, wDayOfWeek, wDay,
               wHour, wMinute, wSecond, wMilliseconds;
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        public SetTime()
        {
            this.InitializeComponent();
        

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // App.ViewModel._setTime(23,23);


            Debug.WriteLine("TimeSetting");
            SYSTEMTIME st = new SYSTEMTIME();
            GetSystemTime(ref st);
            
            st.wHour = (ushort)(st.wHour + 1 % 24);
            if (SetSystemTime(ref st) == 0)
            
            st.wHour = (ushort)(st.wHour - 1 % 24);
            SetSystemTime(ref st);
            




        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {

            
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    

    
    }
}
