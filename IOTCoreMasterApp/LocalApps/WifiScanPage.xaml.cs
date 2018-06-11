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
using Windows.Devices.WiFi;
using Windows.Networking;
using Windows.Networking.Connectivity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Security.Credentials;
using System.Diagnostics;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    /// 
    class WifiAvailableAP : INotifyPropertyChanged
    {
        public String WiFiImage { get; set; } = "";   

        public WiFiAvailableNetwork network;
        public WifiAvailableAP(WiFiAvailableNetwork availableNetwork)
        {
            this.network = availableNetwork;
            this.InitNetworkImage();
        }
        public String Ssid
        {
            get
            {
                return network.Ssid;
            }
        }
        public String Rssi
        {
            get
            {
                return string.Format("Rssi:{0}dBm", network.NetworkRssiInDecibelMilliwatts);
            }
        }
        public String AuthenticationType
        {
            get
            {
                return string.Format("secure:{0}", network.SecuritySettings.NetworkAuthenticationType.ToString());
            }
        }
        public void InitNetworkImage()
        {
            string imageFileNamePrefix = "secure";
            if (network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211)
            {
                imageFileNamePrefix = "open";
            }

            this.WiFiImage = string.Format("ms-appx:/Assets/{0}_{1}bar.png", imageFileNamePrefix, network.SignalBars);
            OnPropertyChanged("WiFiImage");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
    public class ConnectInfoFlyout
    {
        public Flyout flyout;
        public Button connectButton;
        public Button closeButton;
        public TextBlock textBlock;
        public StackPanel stackPanel;
        public PasswordBox passwordBox;
        public FrameworkElement element;
        public ConnectInfoFlyout()
        {
            flyout = new Flyout();
            connectButton = new Button();
            closeButton = new Button();
            textBlock = new TextBlock();
            stackPanel = new StackPanel();
            passwordBox = new PasswordBox();
        }
        public void SetConent()
        {
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(passwordBox);
            stackPanel.Children.Add(connectButton);
            stackPanel.Children.Add(closeButton);
            flyout.Content = stackPanel;
        }
        public void SetFrameworkElement(FrameworkElement parent)
        {
            this.element = parent;
        }
        public void Show()
        {
            flyout.ShowAt(this.element);
        }
    }
    public sealed partial class WifiScanPage : Page
    {
        private WiFiAdapter m_WifiAdapter;
        private ConnectInfoFlyout m_ConnectInfo;
        private ObservableCollection<WifiAvailableAP> m_WifiApCollection;
        public static string flag = "";

        public WifiScanPage()
        {
            this.InitializeComponent();
            this.m_ScanButton.IsEnabled = false;

            this.m_WifiApCollection = new ObservableCollection<WifiAvailableAP>();
            this.m_WifiApCollectionListView.ItemsSource = this.m_WifiApCollection;

            this.m_ConnectInfo = new ConnectInfoFlyout();

            m_ConnectInfo.flyout.Placement = FlyoutPlacementMode.Bottom;

            m_ConnectInfo.stackPanel.Orientation = Orientation.Horizontal;
            m_ConnectInfo.stackPanel.Width = 300;

            m_ConnectInfo.textBlock.Text = " Key";
            m_ConnectInfo.textBlock.Margin = new Thickness(5, 0, 5, 0);
            m_ConnectInfo.textBlock.VerticalAlignment = VerticalAlignment.Center;

            m_ConnectInfo.passwordBox.Width = 100;
            m_ConnectInfo.passwordBox.Margin = new Thickness(5, 0, 10, 0);
            //<Button Click="ConnectButton_Click" Margin="5,0,0,0">Connect</Button>
            m_ConnectInfo.connectButton.Margin = new Thickness(5, 0, 00, 0);
            m_ConnectInfo.connectButton.Content = "Connect ";
            m_ConnectInfo.connectButton.Click += ConnectButton_Click;
            m_ConnectInfo.closeButton.Margin = new Thickness(5, 0, 0, 0);
            m_ConnectInfo.closeButton.Content = "Close ";
            m_ConnectInfo.closeButton.Click += CloseButton_Click;

            m_ConnectInfo.SetConent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            PopupFailMessage.IsOpen = false;
            try
            {
                var access = await WiFiAdapter.RequestAccessAsync();
                if (access != WiFiAccessStatus.Allowed)
                {
                    //rootPage.NotifyUser("Access denied", NotifyType.ErrorMessage);
                }
                else
                {
                    var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                    if (result.Count >= 1)
                    {
                        m_WifiAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);

                        await m_WifiAdapter.ScanAsync();

                        DisplayNetworkReport(m_WifiAdapter.NetworkReport);

                        m_ScanButton.IsEnabled = true;

                    }
                    else
                    {
                        //rootPage.NotifyUser("No WiFi Adapters detected on this machine.", NotifyType.ErrorMessage);
                    }
                }
            }
            catch(Exception ex)
            {
                PopupFailMessage.IsOpen = true;
                //this.Frame.GoBack();
                Message.Text = ex.ToString();
            }
            
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            await m_WifiAdapter.ScanAsync();

            DisplayNetworkReport(m_WifiAdapter.NetworkReport);
        }

        private void DisplayNetworkReport(WiFiNetworkReport report)
        {
            //rootPage.NotifyUser(string.Format("Network Report Timestamp: {0}", report.Timestamp), NotifyType.StatusMessage);

            m_WifiApCollection.Clear();

            foreach (var network in report.AvailableNetworks)
            {
                m_WifiApCollection.Add(new WifiAvailableAP(network));
            }
        }

        private async void WifiApCollectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WifiAvailableAP selectedNetwork = e.AddedItems[0] as WifiAvailableAP;

            if (selectedNetwork == null || m_WifiAdapter == null)
            {
                //rootPage.NotifyUser("Network not selcted", NotifyType.ErrorMessage);
                return;
            }

            WiFiConnectionResult result;
            if (selectedNetwork.network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211)
            {
                this.ShowProgressRing();

                result = await m_WifiAdapter.ConnectAsync(selectedNetwork.network, WiFiReconnectionKind.Automatic);

                this.CloseProgressRing();

                if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                {
                    flag = "on";
                   this.Frame.GoBack();

                }
                   
            }
            else
            {
                m_ConnectInfo.Show();
            }
        }
        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            m_ConnectInfo.SetFrameworkElement(sender as FrameworkElement);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedNetwork = m_WifiApCollectionListView.SelectedItem as WifiAvailableAP;
                var credential = new PasswordCredential();

                this.m_ConnectInfo.flyout.Hide();

                credential.Password = m_ConnectInfo.passwordBox.Password;

                this.ShowProgressRing();

                WiFiConnectionResult result = await m_WifiAdapter.ConnectAsync(selectedNetwork.network, WiFiReconnectionKind.Automatic, credential);

                this.CloseProgressRing();

                if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                    this.Frame.GoBack();
                   
                //throw new NotImplementedException();
            }
            catch (Exception ex){ Debug.WriteLine(ex.ToString()); }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.m_ConnectInfo.flyout.Hide();
            //throw new NotImplementedException();
        }

        private void ShowProgressRing()
        {
            m_WifiApCollectionListView.IsEnabled = false;
            m_Popup.IsOpen = true;
        }
        private void CloseProgressRing()
        {
            m_Popup.IsOpen = false;
            m_WifiApCollectionListView.IsEnabled = true;
        }

        private void SPTap(object sender, TappedRoutedEventArgs e)
        {
            PopupFailMessage.IsOpen = false;
            //this.Frame.GoBack();
        }
    }
}
