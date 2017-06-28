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
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using System.Net;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.System;
using Windows.Networking;
using Windows.Networking.Connectivity;
using IOTCoreMasterApp.Utils;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class ConnectMainPackage : Page
    {
        private DispatcherTimer m_DispatcherTimer = new DispatcherTimer();
        private List<AdapterInfo> m_AdapterInfo;
        public ConnectMainPackage()
        {
            this.InitializeComponent();

            m_DispatcherTimer.Tick += DispatcherTimerTick;
            m_DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10000);

            m_AdapterInfo = AdaptersHelper.GetAdapters();
        }
        void DispatcherTimerTick(object sender, object e)
        {
            this.InitializeComponentValue();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.InitializeComponentValue();
            m_DispatcherTimer.Start();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (m_DispatcherTimer.IsEnabled)
                m_DispatcherTimer.Stop();

            base.OnNavigatedFrom(e);
        }
        private void InitializeComponentValue()
        {
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var majorVersion = (version & 0xFFFF000000000000L) >> 48;
            var minorVersion = (version & 0x0000FFFF00000000L) >> 32;
            var buildVersion = (version & 0x00000000FFFF0000L) >> 16;
            var revisionVersion = (version & 0x000000000000FFFFL);

            EasClientDeviceInformation clientDeviceInformation = new EasClientDeviceInformation();
            //this.m_BoardName.Text = "Askey PCA6800";
            //this.m_DeviceName.Text = clientDeviceInformation.FriendlyName;
            this.m_DeviceName.Text = GetHostName();


            this.m_OsVersion.Text = $"{majorVersion}.{minorVersion}.{buildVersion}.{revisionVersion}"; ;

            this.m_AdapterName.Text = "Qualcomm Atheros Wireless LAN Adapter";// await GetWifiAdapterName();
            this.m_AdapterMac.Text = GetAdapterMAC("Qualcomm Atheros Wireless LAN Adapter");

            this.m_WifiNetworkName.Text = GetCurrentProfile();
            this.m_WifiNetworkAddr.Text = GetCurrentIpv4Address();

            this.m_BluetoothName.Text = "Qualcomm Bluetooth Device";
            this.m_BluetoothMac.Text = GetAdapterMAC("Bluetooth Device (Personal Area Network)");
        }
        public string GetAdapterMAC(string Name)
        {
            foreach (var adapter in m_AdapterInfo)
            {
                if (adapter.Description.CompareTo(Name) == 0)
                {
                    return adapter.MAC;
                }
            }

            return "--------";
        }
        public static string GetCurrentIpv4Address()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null
                  && icp.NetworkAdapter != null
                  && icp.NetworkAdapter.NetworkAdapterId != null)
            {
                var name = icp.ProfileName;

                var hostnames = NetworkInformation.GetHostNames();

                foreach (var hn in hostnames)
                {
                    if (hn.IPInformation != null
                        && hn.IPInformation.NetworkAdapter != null
                        && hn.IPInformation.NetworkAdapter.NetworkAdapterId != null
                        && hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId
                        && hn.Type == HostNameType.Ipv4)
                    {
                        return hn.CanonicalName;
                    }
                }
            }
            return "-------";
        }
        public static string GetCurrentProfile()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null
                  && icp.NetworkAdapter != null
                  && icp.NetworkAdapter.NetworkAdapterId != null)
            {
                return icp.ProfileName;
            }
            return "-------";
        }
        public static string GetHostName()
        {
            try
            {
                var hostname = NetworkInformation.GetHostNames()
                    .FirstOrDefault(x => x.Type == HostNameType.DomainName);
                if (hostname != null)
                {
                    return hostname.CanonicalName;
                }
            }
            catch (Exception)
            {
                // do nothing
                // in some (strange) cases NetworkInformation.GetHostNames() fails... maybe a bug in the API...
            }
            return "-------";
        }
        private void WifiStackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WifiScanPage));
        }
        private void BluetoothStackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BluetoothDevice));
        }

        private void Image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //this.m_MediaElement.Play();
        }
        private void Setting_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Application.Current.Exit();
            //m_Popup.IsOpen = true;
        }
        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0.5));
        }
        private void PowerOffButtonClick(object sender, RoutedEventArgs e)
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0.5));
        }
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            //this.m_Popup.IsOpen = false;
        }

        public void APPEXIT(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
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
