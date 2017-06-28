using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.Devices.Enumeration;
using System.ComponentModel;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    /// 

    public class BthDevice : INotifyPropertyChanged
    {
        private DeviceInformation deviceInfo;
        public BitmapImage BthImage { get; set; }
        public BthDevice(DeviceInformation deviceInfoIn)
        {
            deviceInfo = deviceInfoIn;
            UpdateGlyphBitmapImage();
        }
        public string Id
        {
            get
            {
                return deviceInfo.Id;
            }
        }
        public string Name
        {
            get
            {
                return deviceInfo.Name;
            }
        }
        public bool CanPair
        {
            get
            {
                return deviceInfo.Pairing.CanPair;
            }
        }
        public bool IsPaired
        {
            get
            {
                return deviceInfo.Pairing.IsPaired;
            }
        }
        public DeviceInformation DeviceInformation
        {
            get
            {
                return deviceInfo;
            }

            private set
            {
                deviceInfo = value;
            }
        }
        private async void UpdateGlyphBitmapImage()
        {
            DeviceThumbnail deviceThumbnail = await deviceInfo.GetGlyphThumbnailAsync();
            BitmapImage glyphBitmapImage = new BitmapImage();
            await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
            BthImage = glyphBitmapImage;
            OnPropertyChanged("BthImage");
        }
        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            deviceInfo.Update(deviceInfoUpdate);
            OnPropertyChanged("Id");
            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
            OnPropertyChanged("CanPair");
            OnPropertyChanged("IsPaired");
            UpdateGlyphBitmapImage();
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

    public sealed partial class BluetoothDevice : Page
    {
        private DeviceWatcher deviceWatcher = null;
        private TypedEventHandler<DeviceWatcher, DeviceInformation> handlerAdded = null;
        private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> handlerUpdated = null;
        private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> handlerRemoved = null;
        private TypedEventHandler<DeviceWatcher, Object> handlerEnumCompleted = null;
        private TypedEventHandler<DeviceWatcher, Object> handlerStopped = null;
        ObservableCollection<BthDevice> m_BthDeviceCollection;
        public BluetoothDevice()
        {
            this.InitializeComponent();
            this.m_BthDeviceCollection = new ObservableCollection<BthDevice>();
            this.m_BthCollectionListView.ItemsSource = m_BthDeviceCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StartWatcher();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopWatcher();
            base.OnNavigatedFrom(e);
        }
        private void StartWatcher()
        {
            DeviceInformationKind BluetoothDeviceKind = DeviceInformationKind.AssociationEndpoint;
            //string BluetoothDeviceFilter = "System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\"";
            string BluetoothDeviceFilter = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\" OR System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\"";
            
            deviceWatcher = DeviceInformation.CreateWatcher(BluetoothDeviceFilter,
                                                            null, // don't request additional properties for this sample
                                                            BluetoothDeviceKind);
                                                            

            m_BthDeviceCollection.Clear();

            handlerAdded = new TypedEventHandler<DeviceWatcher, DeviceInformation>(async (watcher, deviceInfo) =>
            {
                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    m_BthDeviceCollection.Add(new BthDevice(deviceInfo));
                });
            });

            deviceWatcher.Added += handlerAdded;
            handlerUpdated = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    // Find the corresponding updated DeviceInformation in the collection and pass the update object
                    // to the Update method of the existing DeviceInformation. This automatically updates the object
                    // for us.
                    foreach (BthDevice device in m_BthDeviceCollection)
                    {
                        if (device.Id == deviceInfoUpdate.Id)
                        {
                            device.Update(deviceInfoUpdate);

                            break;
                        }
                    }
                });
            });

            deviceWatcher.Updated += handlerUpdated;

            handlerRemoved = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    // Find the corresponding DeviceInformation in the collection and remove it
                    foreach (BthDevice device in m_BthDeviceCollection)
                    {
                        if (device.Id == deviceInfoUpdate.Id)
                        {
                            m_BthDeviceCollection.Remove(device);
                            break;
                        }
                    }
                });
            });
            deviceWatcher.Removed += handlerRemoved;

            handlerEnumCompleted = new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {

                });
            });
            deviceWatcher.EnumerationCompleted += handlerEnumCompleted;

            handlerStopped = new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {

                });
            });
            deviceWatcher.Stopped += handlerStopped;
            deviceWatcher.Start();
        }
        private void StopWatcher()
        {
            if (null != deviceWatcher)
            {
                // First unhook all event handlers except the stopped handler. This ensures our
                // event handlers don't get called after stop, as stop won't block for any "in flight" 
                // event handler calls.  We leave the stopped handler as it's guaranteed to only be called
                // once and we'll use it to know when the query is completely stopped. 
                deviceWatcher.Added -= handlerAdded;
                deviceWatcher.Updated -= handlerUpdated;
                deviceWatcher.Removed -= handlerRemoved;
                deviceWatcher.EnumerationCompleted -= handlerEnumCompleted;

                if (DeviceWatcherStatus.Started == deviceWatcher.Status ||
                    DeviceWatcherStatus.EnumerationCompleted == deviceWatcher.Status)
                {
                    deviceWatcher.Stop();
                }
            }
        }
        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            /*
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
            */
            //m_Popup.ShowAt(senderElement);


        }
        private async void BthDeviceCollectionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BthDevice bthDevice = m_BthCollectionListView.SelectedItem as BthDevice;

            DevicePairingResult dpr;
            DeviceUnpairingResult dupr;

            this.ShowProgressRing();

            if (bthDevice.IsPaired == false)
            {
                if (bthDevice.CanPair == true)
                {
                    dpr = await bthDevice.DeviceInformation.Pairing.PairAsync();
                }
            }
            else
            {
                dupr = await bthDevice.DeviceInformation.Pairing.UnpairAsync();
            }

            this.CloseProgressRing();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void ShowProgressRing()
        {
            m_BthCollectionListView.IsEnabled = false;
            m_Popup.IsOpen = true;
        }
        private void CloseProgressRing()
        {
            m_Popup.IsOpen = false;
            m_BthCollectionListView.IsEnabled = true;
        }
    }
}
