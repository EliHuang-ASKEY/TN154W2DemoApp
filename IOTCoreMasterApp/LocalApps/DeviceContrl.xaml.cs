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
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.Graphics.Display;







// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    

    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class DeviceContrl : Page
    {
        

        private BrightnessOverride bo;
        


        public DeviceContrl()
        {
            this.InitializeComponent();

            

        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string sErrMessage = "";

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Graphics.Display.BrightnessOverride"))
            {

                try
                {
                    //Create BrightnessOverride object
                    bo = BrightnessOverride.GetDefaultForSystem();

                    bo.StartOverride();
                    
                    double value = bo.GetLevelForScenario(DisplayBrightnessScenario.DefaultBrightness) * 100;
                    BrignessSlider.Value = value;
                    BrignessSlider.Header = string.Format("Brigness：{0}", BrignessSlider.Value);
                    BrignessSlider.IsEnabled = true;
                    

                }
                catch (Exception ex)
                {
                    sErrMessage = ex.Message;
                }
            }

            if (bo == null)
            {
                //sMsg.Text = "This Devices not support BrightnessOverride" + "\n" + "\n" + sErrMessage;
            }


            /*
            var localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["Brightness"];
            if (value == null)
            {
                SetBrightness();
            }
            else {
                //假設有此資料
                textBox_Brightness_Level_Value.Text = Convert.ToString(value);
                slider_Brightness.Value = Convert.ToInt32(value);
                //將資料轉型成你要的型態

            }
            */

        }


        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            bo.StopOverride();
            await BrightnessOverride.SaveForSystemAsync(bo);
            
            //saveBrightness();
          


        }

        


        private void slider_Brightness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider _slider = (Slider)sender;

            _slider.IsEnabled = false;

            if (bo != null)
            {
                double value = _slider.Value / 100;

                //Set override brightness to full brightness even when battery is low

                //bo.SetBrightnessScenario(DisplayBrightnessScenario.FullBrightness, DisplayBrightnessOverrideOptions.None);
                bo.SetBrightnessLevel(value, DisplayBrightnessOverrideOptions.None);
                
                //Request to start the overriding process

            }

            _slider.Header = string.Format("Brigness：{0}", BrignessSlider.Value);
            _slider.IsEnabled = true;


           

        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {


            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }


        /*
        public async void SetBrightness()
        {
            string text;
            text = await PathIO.ReadTextAsync("ms-appdata:///local/BrightnessLevel.txt");
            textBox_Brightness_Level_Value.Text = text;
            slider_Brightness.Value = Convert.ToInt32(text);
           
        }
        
        public async void saveBrightness()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            //寫入資料到localSettings之中
            localSettings.Values["Brightness"] = textBox_Brightness_Level_Value.Text;
            //await FileIO.WriteTextAsync(sf, "19");
            try
            {
                //create file in public folder
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await storageFolder.CreateFileAsync("BrightnessLevel.txt", CreationCollisionOption.ReplaceExisting);

                //write sring to created file
                await FileIO.WriteTextAsync(sampleFile, textBox_Brightness_Level_Value.Text);
                //await RunProcess("SetRegValueCurrentUser.exe", textBox_Brightness_Level_Value.Text);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("error: " + ex);

            }
        }

       */
    }
}
