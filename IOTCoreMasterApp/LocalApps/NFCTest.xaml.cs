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
using NdefLibrary.Ndef;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class NFCTest : Page
    {

        private Windows.Networking.Proximity.ProximityDevice proxDevice;
        public NFCTest()
        {
            this.InitializeComponent();
            proxDevice = ProximityDevice.GetDefault();
            if (proxDevice != null)
            {
                proxDevice.DeviceArrived += DeviceArrived;
                proxDevice.DeviceDeparted += DeviceDeparted;
                proxDevice.SubscribeForMessage("NDEF", messagedReceived);

            }
            else
            {
                WriteMessageText("No proximity device found\n");
                WriteMessageText("No proximity device found\n");
            }
        }

        private void messagedReceived(ProximityDevice device, ProximityMessage m)
        {
            uint x = m.Data.Length;
            byte[] b = new byte[x];
            b = m.Data.ToArray();

            PlaySound();

            // string s = Encoding.UTF8.GetString(b, 0, b.Length);
            // Debug.WriteLine(s);


            NdefMessage ndefMessage = NdefMessage.FromByteArray(b);

            foreach (NdefRecord record in ndefMessage)
            {
                WriteMessageText("\n----------------------------");
                WriteMessageText("\nType: " + Encoding.UTF8.GetString(record.Type, 0, record.Type.Length));
                if (record.CheckSpecializedType(false) == typeof(NdefUriRecord))
                {
                    var uriRecord = new NdefUriRecord(record);
                    WriteMessageText("\nURI: " + uriRecord.Uri);
                };
            }
            WriteMessageText("\n----------------------------\n");


        }

        public void DeviceArrived(ProximityDevice proximityDevice)
        {
            WriteMessageText("Proximate device arrived\n");
        }
        public void DeviceDeparted(ProximityDevice proximityDevice)
        {
            WriteMessageText("Proximate device departed\n");
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

        async private void PlaySound()
        {
            await messageDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                       () =>

                       {
                           beep.Play();
                       });
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void CleanMessage(object sender, RoutedEventArgs e)
        {
            textBlock.Text = "";
        }
    }
}
