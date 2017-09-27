using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Radios;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System.Profile;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Graphics.Display;


// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class Setting : Page
    {

        private App.RadioCtl m_RaidoController = new App.RadioCtl();
        private bool SoundEffectState = false;

        private BrightnessOverride bo;


        public Setting()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            PopupFailMessage.IsOpen = false;

            checkOSVersion();
            checkSoundEffectState();
            /*
            if (await RequestRadioController() == true)
            {
                mes.Text += "1";
                this.UpdateWifiRadioStatus();
                this.UpdateBluetoothRadioStatus();

                m_RaidoController.Wifi.StateChanged += WifiRadio_StateChanged;
                m_RaidoController.Bluetooth.StateChanged += BluetoothRadio_StateChanged;
            }
            */
            if (await RequestRadioController() == true)
            {
                mes.Text += "1";                
                this.UpdateBluetoothRadioStatus();
                
                m_RaidoController.Bluetooth.StateChanged += BluetoothRadio_StateChanged;
            }
            
            if (await RequestRadioControllerWifi() == true)
            {
                mes.Text += "5";
                this.UpdateWifiRadioStatus();
              
                m_RaidoController.Wifi.StateChanged += WifiRadio_StateChanged;
                
            }
        }
        /*
        public async Task<bool> RequestRadioController()
        {
            bool raido_wifi = false;
            bool raido_bluetooth = false;

            var accessLevel = await Radio.RequestAccessAsync();
            if (accessLevel != RadioAccessStatus.Allowed)
            {
                return false;
            }
            else
            {
                var radios = await Radio.GetRadiosAsync();
                mes.Text += "2";
                foreach (var radio in radios)
                {
                    switch (radio.Kind)
                    {
                        case RadioKind.WiFi:
                            mes.Text += "4";
                            this.m_RaidoController.Wifi = radio;
                            raido_wifi = true;
                            break;
                        case RadioKind.Bluetooth:
                            mes.Text += "3";
                            this.m_RaidoController.Bluetooth = radio;
                            raido_bluetooth = true;
                            break;
                    }
                }
            }

            if (raido_wifi && raido_bluetooth)
                return true;
            mes.Text += raido_wifi+"_"+raido_bluetooth;
            return false;
        }
        */
        public async Task<bool> RequestRadioController()
        {
            bool raido_wifi = false;
            bool raido_bluetooth = false;
            int i = 0;
            var accessLevel = await Radio.RequestAccessAsync();
            if (accessLevel != RadioAccessStatus.Allowed)
            {
                return false;
            }
            else
            {
                var radios = await Radio.GetRadiosAsync();
                
                
                foreach (var radio in radios)
                {
                    i++;
                    switch (radio.Kind)
                    {
                        case RadioKind.WiFi:
                           
                            this.m_RaidoController.Wifi = radio;
                            raido_wifi = true;
                            break;
                        case RadioKind.Bluetooth:
                            
                            this.m_RaidoController.Bluetooth = radio;
                            raido_bluetooth = true;
                            break;
                    }
                }

                /*
                var bluetoothRadio = radios.FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth);
                return bluetoothRadio != null && bluetoothRadio.State == RadioState.On;
                */
            }

            
            if (raido_bluetooth)
                return true;
            else
                return false;
        }

        public async Task<bool> RequestRadioControllerWifi()
        {
            bool raido_wifi = false;
            bool raido_bluetooth = false;

            var accessLevel = await Radio.RequestAccessAsync();
            if (accessLevel != RadioAccessStatus.Allowed)
            {
                return false;
            }
            else
            {
                var radios = await Radio.GetRadiosAsync();
                
                
                foreach (var radio in radios)
                {
                    switch (radio.Kind)
                    {
                        case RadioKind.WiFi:
                            
                            this.m_RaidoController.Wifi = radio;
                            raido_wifi = true;
                            break;
                        case RadioKind.Bluetooth:
                            
                            this.m_RaidoController.Bluetooth = radio;
                            raido_bluetooth = true;
                            break;
                    }
                }
            }
            
            if (raido_wifi )
                return true;
            else
                return false;
        }

        private async void UpdateWifiRadioStatus()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    m_WifiRadio.IsOn = (m_RaidoController.Wifi.State == RadioState.On);
                });
            }
            catch (Exception ex){
                PopupFailMessage.IsOpen = true;
                Message.Text += ex.ToString();
            }
        }
        private void WifiRadio_StateChanged(Radio sender, object args)
        {
            UpdateWifiRadioStatus();
        }
        private async void UpdateBluetoothRadioStatus()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    m_BluetoothRadio.IsOn = (m_RaidoController.Bluetooth.State == RadioState.On);
                });
            }
            catch (Exception ex)
            {
                PopupFailMessage.IsOpen = true;
                Message.Text += ex.ToString();
            }
        }
        private void BluetoothRadio_StateChanged(Radio sender, object args)
        {
            UpdateBluetoothRadioStatus();
        }
      
        private async void BluetoothPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            switch (this.m_RaidoController.Bluetooth.State)
            {
                case RadioState.On:
                    //this.Frame.Navigate(typeof(BluetoothDevice));
                    break;
                case RadioState.Off:
                    await this.m_RaidoController.Bluetooth.SetStateAsync(RadioState.On);
                    break;
                default:
                    break;
            }
        }
        private async void WifiPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            switch (this.m_RaidoController.Wifi.State)
            {
                case RadioState.On:
                    //this.Frame.Navigate(typeof(WifiScanPage));
                    break;
                case RadioState.Off:
                    await this.m_RaidoController.Wifi.SetStateAsync(RadioState.On);
                    break;
                default:
                    break;
            }
        }
        private async void Bluetooth_Toggled(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (this.m_BluetoothRadio.IsOn)
                {
                    if (this.m_RaidoController.Bluetooth.State == RadioState.On)
                    {
                            
                    }
                    else
                    {
                        await this.m_RaidoController.Bluetooth.SetStateAsync(RadioState.On);
                    }
                }
                else
                {
                    if (this.m_RaidoController.Bluetooth.State == RadioState.Off)
                    {

                    }
                    else
                    {   
                        await this.m_RaidoController.Bluetooth.SetStateAsync(RadioState.Off);
                    }
                }
            }
            catch(Exception ex)
            {
                PopupFailMessage.IsOpen = true;
                Message.Text = ex.ToString();
            }
        }
        private async void WifiRadio_Toggled(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (this.m_WifiRadio.IsOn)
                {
                    if (this.m_RaidoController.Wifi.State == RadioState.On)
                    {

                    }
                    else
                    {
                     await this.m_RaidoController.Wifi.SetStateAsync(RadioState.On);
                    }
                }
                else
                {
                    if (this.m_RaidoController.Wifi.State == RadioState.Off)
                    {

                    }
                    else
                    {
                        await this.m_RaidoController.Wifi.SetStateAsync(RadioState.Off);
                    }
                }
            }
            catch(Exception ex)
            {
                PopupFailMessage.IsOpen = true;
                Message.Text = ex.ToString();
            }
}

        private void checkOSVersion()
        {
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var majorVersion = (version & 0xFFFF000000000000L) >> 48;
            var minorVersion = (version & 0x0000FFFF00000000L) >> 32;
            var buildVersion = (version & 0x00000000FFFF0000L) >> 16;
            var revisionVersion = (version & 0x000000000000FFFFL);
            if(buildVersion > 14393) SPbluetooth.Visibility = Visibility.Visible;
            else if(buildVersion == 14393 & revisionVersion >= 448) SPbluetooth.Visibility = Visibility.Visible;



        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        private async void checkSoundEffectState()
        {

            string text;
            try
            {
                /*
                // 通过Uri获取本地文件
                StorageFile sf = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Assets/SoundEffects.txt"));
                // 打开文件获取文件的数据流
                IRandomAccessStream accessStream = await sf.OpenReadAsync();
                // 使用StreamReader读取文件的内容，需要将IRandomAccessStream对象转化为Stream对象来初始化StreamReader对象
                using (StreamReader streamReader = new StreamReader(accessStream.AsStreamForRead((int)accessStream.Size)))
                {
                    text = streamReader.ReadToEnd();
                }
                */
                //StorageFolder storageFolder = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
                //StorageFile sampleFile = await storageFolder.CreateFileAsync("soundeffects.txt", CreationCollisionOption.ReplaceExisting);
                

                text = await PathIO.ReadTextAsync("ms-appdata:///local/SoundEffects.txt");
                mes.Text = "PATHIO"+text+"_";
                
                if (text == "True")
                {
                    this.SoundEffectState = true;
                    this.m_soundeffect.IsOn = true;
                }
                else
                {
                    this.SoundEffectState = false;
                    this.m_soundeffect.IsOn = false;
                }

                Debug.WriteLine(" m_soundeffect.IsOn =" + text);
            }
            catch (Exception exce)
            {
                text = "SoundEffect文件讀取錯誤：" + exce.Message;
            }
            //UpdateSoundEffectStatus();
        }
        private async void UpdateSoundEffectStatus()
        {
            try
            {
                //create file in public folder
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await storageFolder.CreateFileAsync("soundeffects.txt", CreationCollisionOption.ReplaceExisting);
               
                //write sring to created file
                //StorageFolder storageFolder = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
               // StorageFile sampleFile = await storageFolder.CreateFileAsync("soundeffects.txt", CreationCollisionOption.ReplaceExisting);

                Debug.WriteLine("File Path: " + storageFolder.Path);

                await FileIO.WriteTextAsync(sampleFile, SoundEffectState.ToString());

                

            }
            catch (Exception ex)
            {
                //Debug.WriteLine("error: " + ex);

            }
        }

        private void SoundEffect_Toggled(object sender, RoutedEventArgs e)
        {
            
            if (this.m_soundeffect.IsOn)
            {
                SoundEffectState = true;
            }
            else
            {
                SoundEffectState = false;
            }

            UpdateSoundEffectStatus();

        }

        private void SPTap(object sender, TappedRoutedEventArgs e)
        {
            PopupFailMessage.IsOpen = false;
        }

        private void ResetSPTap(object sender, TappedRoutedEventArgs e)
        {
            PopupResetMessage.IsOpen = false;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
                //popup resetting message
                PopupResetMessage.IsOpen = true;


                //set BT and Wifi switch to off
                if (m_WifiRadio.IsOn || m_BluetoothRadio.IsOn)
                {
                    m_WifiRadio.IsOn = false;
                    m_BluetoothRadio.IsOn = false;
                }


                //set the Brightness to Max
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Graphics.Display.BrightnessOverride"))
                {

                    bo = BrightnessOverride.GetDefaultForSystem();
                    bo.StartOverride();
                    double value = 1.0;//1.0=>Brightness=100,0.8=>>Brightness=80
                    bo.SetBrightnessLevel(value, DisplayBrightnessOverrideOptions.None);
                    await Task.Delay(1000);
                    bo.StopOverride();
                    await BrightnessOverride.SaveForSystemAsync(bo);
                }


                //delete all files in Video floder
                var Videofiles = await KnownFolders.VideosLibrary.GetFilesAsync();

                foreach (var file in Videofiles)
                {
                    await file.DeleteAsync(StorageDeleteOption.Default);
                }


                //delete all files in Picture floder
                var Picturefiles = await KnownFolders.PicturesLibrary.GetFilesAsync();

                foreach (var file in Picturefiles)
                {
                    await file.DeleteAsync(StorageDeleteOption.Default);
                }


                //delete all files in Music floder
                var Musicfiles = await KnownFolders.MusicLibrary.GetFilesAsync();

                foreach (var file in Musicfiles)
                {
                    await file.DeleteAsync(StorageDeleteOption.Default);
                }

                //close resetting message
                PopupFailMessage.IsOpen = false;

                await Task.Delay(2000);

                //go back to mainpage when reset finished
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }


        }
            
    }
}
