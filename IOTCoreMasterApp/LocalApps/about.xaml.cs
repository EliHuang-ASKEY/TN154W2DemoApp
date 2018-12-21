using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//linda
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class about : Page
    {
        public about()
        {
            this.InitializeComponent();
            this.InitializeComponentValue();
            
        }


        private async void InitializeComponentValue()
        {
            this.m_APPVersion.Text =string.Format("APPVer: {0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);


            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var majorVersion = (version & 0xFFFF000000000000L) >> 48;
            var minorVersion = (version & 0x0000FFFF00000000L) >> 32;
            var buildVersion = (version & 0x00000000FFFF0000L) >> 16;
            var revisionVersion = (version & 0x000000000000FFFFL);


            EasClientDeviceInformation clientDeviceInformation = new EasClientDeviceInformation();
            this.m_OperatingSystem.Text = clientDeviceInformation.OperatingSystem;
            this.m_SystemVersion.Text = $"{majorVersion}.{minorVersion}.{buildVersion}.{revisionVersion}"; ;
            //this.m_SystemHardwareVersion.Text = clientDeviceInformation.SystemHardwareVersion;
            //this.m_SystemFirmwareVersion.Text = clientDeviceInformation.SystemFirmwareVersion;

            string s = "PhoneBootLoaderVersion";
            await RunProcess("c:\\windows\\system32\\GetRegValue.exe", s,1);
            s = "PhoneFirmwareRevision";
            await RunProcess("c:\\windows\\system32\\GetRegValue.exe", s, 2);
            s = "PhoneHardwareRevision";
            await RunProcess("c:\\windows\\system32\\GetRegValue.exe", s, 3);
            s = "PhoneManufacturer";
            await RunProcess("c:\\windows\\system32\\GetRegValue.exe", s, 4);
            s = "PhoneManufacturerModelName";
            await RunProcess("c:\\windows\\system32\\GetRegValue.exe", s, 5);            
            //s = "OEMImageVersion";
           // await RunProcess("GetRegValue.exe", s,6);
            //s = "OEMBuildVersion";
            //await RunProcess("GetRegValue.exe", s, 7);
            s = "c:\\DPP\\OEM\\factory.ini General ro.serialno -G";
            this.RunProcess("c:\\windows\\system32\\factory_ini_set_v2.exe", s, 8);
            s = "c:\\DPP\\OEM\\factory.ini General ro.ConfigPN -G";
            this.RunProcess("c:\\windows\\system32\\factory_ini_set_v2.exe", s, 9);
            //linda
            ReadImg();
          

        }
        public async void ReadImg()
        {
            //StorageFile sourceFile = await KnownFolders.PicturesLibrary.GetFileAsync("test.jpg");
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync("c:\\DPP\\OEM\\");// Windows.Storage.StorageFolder storageFolder            
            try
            {
                StorageFile tryFile = await storageFolder.GetFileAsync("e-label.bmp");
            }
            catch
            {
               // WriteMessageText("\nFile does not exits! Check again");
               
                return;
            }
            StorageFile sourceFile = await storageFolder.GetFileAsync("e-label.bmp");

            IRandomAccessStream photoStream = await sourceFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(photoStream);
            imgShow.Source = bitmap;

        }
        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }



        public async Task RunProcess(string cmd, string args, int caseNum)
        {

            var options = new ProcessLauncherOptions();
            var standardOutput = new InMemoryRandomAccessStream();
            var standardError = new InMemoryRandomAccessStream();
            options.StandardOutput = standardOutput;
            options.StandardError = standardError;

            await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    var result = await Windows.System.ProcessLauncher.RunToCompletionAsync(cmd, args == null ? string.Empty : args, options);

                    //ProcessExitCode.Text += "Process Exit Code: " + result.ExitCode;

                    using (var outStreamRedirect = standardOutput.GetInputStreamAt(0))
                    {
                        var size = standardOutput.Size;
                        using (var dataReader = new DataReader(outStreamRedirect))
                        {
                            var bytesLoaded = await dataReader.LoadAsync((uint)size);
                            var stringRead = dataReader.ReadString(bytesLoaded);
                            //StdOutputText.Text += stringRead;
                            switch (caseNum)
                            {
                                case 1:
                                    this.m_BootLoaderVersion.Text = stringRead;
                                    break;
                                case 2:
                                    this.m_FirmwareRevision.Text = stringRead;
                                    break;
                                case 3:
                                    this.m_HardwareRevision.Text ="HWver:"+ stringRead;
                                    break;
                                case 4:
                                    this.m_Manufacturer.Text = stringRead;
                                    break;
                                case 5:
                                    this.m_ManufacturerModelName.Text = stringRead;
                                    break;
                                case 6:                        
                                    //this.m_OEMImageVersion.Text = stringRead;
                                    break;
                                case 7:
                                    //this.m_OEMBuildVersion.Text = stringRead;
                                    break;
                                case 8:
                                    this.m_SN.Text = "SN:" + ParseParams(stringRead);
                                    break;
                                case 9:
                                    this.m_PN.Text = "PN:" + ParseParams(stringRead);
                                    break;

                                default:
                                    //StdOutputText.Text.Text += stringRead;
                                    break;
                            }
                        }
                    }

                    using (var errStreamRedirect = standardError.GetInputStreamAt(0))
                    {
                        using (var dataReader = new DataReader(errStreamRedirect))
                        {
                            var size = standardError.Size;
                            var bytesLoaded = await dataReader.LoadAsync((uint)size);
                            var stringRead = dataReader.ReadString(bytesLoaded);
                            //StdErrorText.Text += stringRead;
                        }
                    }

                }
                catch (UnauthorizedAccessException uex)
                {
                }
                catch (Exception ex)
                {
                }
            });
        }



        private string ParseParams(string Params)
        {

            var ParamsArry = Regex.Split(Params, " = ", RegexOptions.IgnoreCase);
            //ParamsArry = Regex.Split(ParamsArry[1], "\r", RegexOptions.IgnoreCase);
            ParamsArry = Regex.Split(ParamsArry[1], "\\r", RegexOptions.IgnoreCase);
            //ParamsArry = Regex.Split(ParamsArry[0], "\n", RegexOptions.IgnoreCase);
            //ParamsArry = Regex.Split(ParamsArry[0], "\\n", RegexOptions.IgnoreCase);

            // Debug.WriteLine("SN:" + ParamsArry[0] + "@@");
            //this.m_SN.Text = "SN:" + ParamsArry[0];
            //Params = "SN:" + ParamsArry[0];
            return ParamsArry[0];
            /*
            for (int i = 0; i < ParamsArry.Length; i++)
            {
                var item = Regex.Split(ParamsArry[i], "=", RegexOptions.IgnoreCase);
                switch (item[0])
                {
                    case "DefaultRecordTime":
                        this.recordTimeParams = Convert.ToDouble(item[1]);
                        break;

                }
            }
            */

        }


    }
}
