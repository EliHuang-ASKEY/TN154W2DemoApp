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
using Windows.Networking.Proximity;
using System.Diagnostics;
using System.Text;
//using NdefLibrary.Ndef;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404
//linda wifi
using Windows.Security.Credentials;
using Windows.Devices.WiFi;
using Windows.Networking;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;
using Windows.Storage;//0511

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class WifiConnectFromFile : Page
    {
        //linda-start        
        public static string flagExit = "false";
        public static string ipget = "";
        private string savedProfileName = null;        
        string WifiAPName = "IOT Default";
        //string wifitype = "WPA2-Personal";
        string passwd = "p@ssw0rd";
              
        //private Windows.Networking.Proximity.ProximityDevice proxDevice;
         enum WifiConnectResult
         {
             WifiAccessDenied,
             NoWifiDevice,
             Success,
             CouldNotConnect,
             SsidNotFound,
         }
        //linda-end
       /* enum WifiConnectResult
        {
            UnspecifiedFailure = 0,    //連線因為這份清單中所列以外的原因而失敗。    
            Success = 1,     //連線成功。       
            AccessRevoked = 2,    //連線因為網路存取權已撤銷而失敗。  
            InvalidCredential = 3,  //連線因為提供了無效的認證而失敗。
            NetworkNotAvailable = 4,  //連線因為沒有可用網路而失敗。
            Timeout = 5,        //連線因為連線嘗試逾時而失敗。
            UnsupportedAuthenticationProtocol,
        }*/
        public WifiConnectFromFile()
        {
            flagExit = "false";
            this.InitializeComponent();         
            WriteMessageText("\nCurrentProfile: " + GetCurrentProfile());
            readWIFIInfoToConnect();           
        
        }
       
        private async Task<WifiConnectResult> ConnectWifi(string ssid, string password)
           {
               if (password == null)
                   password = "";

               var access = await WiFiAdapter.RequestAccessAsync();
               if (access != WiFiAccessStatus.Allowed)
               {
                   return WifiConnectResult.WifiAccessDenied;
               }
               else
               {
                   var allAdapters = await WiFiAdapter.FindAllAdaptersAsync();
                   if (allAdapters.Count >= 1)
                   {
                       var firstAdapter = allAdapters[0];
                       await firstAdapter.ScanAsync();
                       var network = firstAdapter.NetworkReport.AvailableNetworks.SingleOrDefault(n => n.Ssid == ssid);
                       if (network != null)
                       {
                           WiFiConnectionResult wifiConnectionResult;
                           if (network.SecuritySettings.NetworkAuthenticationType == Windows.Networking.Connectivity.NetworkAuthenticationType.Open80211)
                           {
                               wifiConnectionResult = await firstAdapter.ConnectAsync(network, WiFiReconnectionKind.Automatic);
                           }
                           else
                           {
                               // Only the password potion of the credential need to be supplied
                               var credential = new Windows.Security.Credentials.PasswordCredential();
                               credential.Password = password;

                               wifiConnectionResult = await firstAdapter.ConnectAsync(network, WiFiReconnectionKind.Automatic, credential);
                           }
                           //for get ip
                           if (firstAdapter.NetworkAdapter.GetConnectedProfileAsync() != null)
                           {
                               var connectedProfile = await firstAdapter.NetworkAdapter.GetConnectedProfileAsync();
                               //WriteMessageText("\nconnectedProfile:" + connectedProfile.ProfileName + "\n");
                               if (connectedProfile != null && connectedProfile.ProfileName.Equals(savedProfileName))
                               {
                                   foreach (HostName localHostName in NetworkInformation.GetHostNames())
                                   {
                                       if (localHostName.IPInformation != null)
                                       {
                                           if (localHostName.Type == HostNameType.Ipv4)
                                           {
                                               ipget = localHostName.ToString();
                                               break;
                                           }
                                       }
                                   }
                                   // WriteMessageText("\nIP=localHostName.ToString:" + ipget);
                               }//
                               else
                               {
                                   // WriteMessageText("\n !!!!< >" + Not connected to :"+connectedProfile.ProfileName+"\n");
                               }
                           }

                           if (wifiConnectionResult.ConnectionStatus == WiFiConnectionStatus.Success)
                           {
                               return WifiConnectResult.Success;
                           }
                           else
                           {
                               if (wifiConnectionResult.ConnectionStatus == WiFiConnectionStatus.UnspecifiedFailure)
                                   //return WifiConnectResult.UnspecifiedFailure;
                                   WriteMessageText("\nUnspecified Failure");
                               else if (wifiConnectionResult.ConnectionStatus == WiFiConnectionStatus.AccessRevoked)
                                   // return WifiConnectResult.AccessRevoked;
                                   WriteMessageText("\nAccess Revoked");
                               else if (wifiConnectionResult.ConnectionStatus == WiFiConnectionStatus.InvalidCredential)
                                   // return WifiConnectResult.InvalidCredential;
                                   WriteMessageText("\nInvalid Credential");
                               else if (wifiConnectionResult.ConnectionStatus == WiFiConnectionStatus.NetworkNotAvailable)
                                   //    return WifiConnectResult.NetworkNotAvailable;
                                   WriteMessageText("\nNetwork NotAvailable");
                               else if (wifiConnectionResult.ConnectionStatus == WiFiConnectionStatus.Timeout)
                                   //return WifiConnectResult.Timeout;
                                   WriteMessageText("\nTimeout");
                               else if (wifiConnectionResult.ConnectionStatus == WiFiConnectionStatus.UnsupportedAuthenticationProtocol)
                                   //return WifiConnectResult.UnsupportedAuthenticationProtocol; */
                                    WriteMessageText("\nUnsupported Authentication Protocol");
                            return WifiConnectResult.CouldNotConnect;
                         }
                      
                    }
                    else
                    {
                        return WifiConnectResult.SsidNotFound;
                    }
                }
                else
                {
                    return WifiConnectResult.NoWifiDevice;
                }
            }
        }
 public async void readWIFIInfoToConnect() //string TestCase
        {
            WriteMessageText("\nRead wifi-ssid-pd.txt....");                     
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync("c:\\Data\\users\\public\\");// Windows.Storage.StorageFolder storageFolder            
            try
            {
                StorageFile tryFile = await storageFolder.GetFileAsync("Wifi-ssid-pd.txt");             
            }
            catch
            {
                WriteMessageText("\nFile does not exits! Check again");
                flagExit = "true";
               return;
            }           
            StorageFile sampleFile =await storageFolder.GetFileAsync("Wifi-ssid-pd.txt"); ;// Windows.Storage.StorageFile sampleFile
            string s = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
            if (s.Length > 0)
            {
                int start = -1;
                int find = s.IndexOf("\\");
                if (find > 0)
                {
                    start = 0;

                    WifiAPName = s.Substring(start, find - start);
                    start = find + 1;
                    find = s.IndexOf("\\", start);
                    // wifitype = s.Substring(start, find - start);
                    //  start = find + 1;

                    passwd = s.Substring(start, s.Length - start);
                    WriteMessageText("\nWIFI AP= " + WifiAPName + "\nPasswd= " + passwd);
                }

                WriteMessageText("\nConnecting ...");
                m_Popup.IsOpen = true;
                
                savedProfileName = WifiAPName;
                WriteMessageText("\nprofile= " +  GetCurrentProfile());//+"flag " + flagExit
                WifiConnectResult result = await ConnectWifi(WifiAPName, passwd);
                m_Popup.IsOpen = false;
                if (result == WifiConnectResult.Success)
                {
                    WriteMessageText("\nConnect to " + WifiAPName + " Success\nWait to get IP...");
                    WriteMessageText("\nIP =" + ipget);
                    if ( String.Compare(ipget,0,"169.254",0,7)==0)//169.254.x.x
                       await Task.Delay(5000);

                    //flagExit = "true";
                }
                else if (result == WifiConnectResult.CouldNotConnect)
                    WriteMessageText("\nConnect to " + WifiAPName + " CouldNotConnect");
                else if (result == WifiConnectResult.NoWifiDevice)
                    WriteMessageText("\nConnect to " + WifiAPName + " Failed: NoWifiDevice");
                else if (result == WifiConnectResult.WifiAccessDenied)
                    WriteMessageText("\nConnect to " + WifiAPName + " Failed: WifiAccess Denied\nPlease check txt is correct");
                else if (result == WifiConnectResult.SsidNotFound)
                    WriteMessageText("\nConnect to " + WifiAPName + " Failed: Ssid NotFound");
                else
                    WriteMessageText("\nConnect to " + WifiAPName + " Failed: " + result);

               //await Task.Delay(5000);                
                if (ipget == "-------")//RS4:169.254  RS3:
                {
                    //flag = "NO";
                    //WriteMessageText("\nIP =" + ipget);
                   // flagExit = "true";
                }
                else
                {
                  //  WriteMessageText("\nIP =" + ipget);
                    //flag = "OK";
                }
            }
            else
            {
                flagExit = "true";
                WriteMessageText("\nNo Data in Wifi-ssid-pd.txt\nCheck the txt");
            }
            flagExit = "true";
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
    
        // Write a message to MessageBlock on the UI thread.

        private Windows.UI.Core.CoreDispatcher messageDispatcher = Window.Current.CoreWindow.Dispatcher;

        async private void WriteMessageText(string message, bool overwrite = false)
        {
            await messageDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                       () =>

                       {

                           if (overwrite)

                               textBlock.Text = message;

                           else
                               textBlock.Text += message;

                       });

        }
      

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                if (flagExit == "true")
                {
                   // flagExit = "false";
                    this.Frame.GoBack();
                }             
            }          
            //Application.Current.Exit();
        }

        private void CleanMessage(object sender, RoutedEventArgs e)
        {
            textBlock.Text = "";
        }
    }
}
