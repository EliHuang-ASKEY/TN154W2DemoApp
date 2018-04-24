using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;


using Windows.System.Power;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Power;
using Windows.System.Threading;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.System;
using Windows.ApplicationModel.AppService;
using Windows.Devices.Haptics;
using Windows.Foundation.Collections;
using Windows.Storage.Search;
using Windows.Storage;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BackgroundAzureIotHub
{
    public sealed class StartupTask : IBackgroundTask
    {
        ThreadPoolTimer timer;
        ThreadPoolTimer fileReadTimer;
        BackgroundTaskDeferral _deferral;
        string deviceId = null;



        //public string deviceName = "MikroKopter_BT"; // Specify the device name to be selected; You can find the device name from the webb under bluetooth 

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
            _deferral = taskInstance.GetDeferral();

            PowerManager.BatteryStatusChanged += PowerManager_BatteryStatusChanged;
            PowerManager.PowerSupplyStatusChanged += PowerManager_PowerSupplyStatusChanged;
            PowerManager.RemainingChargePercentChanged += PowerManager_RemainingChargePercentChanged;

            deviceId = await AzureIoTHub.TestHubConnection(false, "");
            DateTime d = DateTime.UtcNow;

            //RateSensor bs = new RateSensor();
            //bs.RateSensorInit();

            //await Task.Delay(1000);

            //bs.RateMonitorON();

            //mt = new MiotyTX();
            //mt.Init();
            //await Task.Delay(1000);

            long x = d.ToFileTime();
            if (deviceId != null)
            {
                await AzureIoTHub.SendDeviceToCloudMessageAsync("{\"pkey\":\"" + deviceId + "\", \"rkey\":\"" + x.ToString() + "\",\"status\":\"Device Restarted\"}");

                bool result = await AzureIoTHub.SendDeviceToCloudMessageAsync("Device Restarted");
                //InitAzureIotReceiver();
            }

            AzureIoTHub.counter++;

            // request access to vibration device
            //if (await VibrationDevice.RequestAccessAsync() != VibrationAccessStatus.Allowed)
            //{
            //    Debug.WriteLine("access to vibration device denied!!!!!!");
            //}

            //enable this to start periodic message to iot Hub 
            this.timer = ThreadPoolTimer.CreateTimer(Timer_Tick, TimeSpan.FromSeconds(2));

            //this.fileReadTimer = ThreadPoolTimer.CreateTimer(FileReadTimer_Tick, TimeSpan.FromMilliseconds(900));

            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        string receivedMessage = null;

                        try
                        {
                            receivedMessage = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
                            AzureIoTHub.counter++;

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("******ERROR RECEIVER: " + ex.Message);
                        }


                        if (receivedMessage == null) continue;
                        Debug.WriteLine("Received message:" + receivedMessage);

                        JsonObject j = null;
                        try
                        {
                            j = JsonObject.Parse(receivedMessage);
                        }
                        catch
                        {
                            Debug.WriteLine(" error");
                            continue;
                        }

                        try
                        {
                            if (j.Keys.Contains("msg"))
                            {
                                string msg = j.GetNamedString("msg");
                                AzureIoTHub.counter++;
                                MessageListItem m = new MessageListItem();
                                m.message = msg;
                                AzureIoTHub.msgList.Add(m);

                                Windows.Storage.StorageFolder storageFolder = Windows.Storage.KnownFolders.PicturesLibrary;
                                Windows.Storage.StorageFile msgFile = await storageFolder.CreateFileAsync("message.txt", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                                await Windows.Storage.FileIO.AppendTextAsync(msgFile, msg);
                            }
                            if (j.Keys.Contains("cmd"))
                            {
                                string cmd = j.GetNamedString("cmd");
                                if (cmd == "info")
                                {
                                    await UpdateBatteryInfo();
                                }
                                if (cmd == "wake")
                                {
                                    if (ShutdownManager.IsPowerStateSupported(PowerState.ConnectedStandby))
                                    {
                                        ShutdownManager.EnterPowerState(PowerState.ConnectedStandby, TimeSpan.FromSeconds(1));
                                        //ShutdownManager.EnterPowerState(PowerState.SleepS3, TimeSpan.FromSeconds(15));
                                    }
                                }
                                if (cmd == "vibra")
                                {
                                    //try
                                    //{
                                    //    VibrationDevice VibrationDevice = await VibrationDevice.GetDefaultAsync();
                                    //    SimpleHapticsControllerFeedback BuzzFeedback = null;
                                    //    foreach (var f in VibrationDevice.SimpleHapticsController.SupportedFeedback)
                                    //    {
                                    //        if (f.Waveform == KnownSimpleHapticsControllerWaveforms.BuzzContinuous)
                                    //            BuzzFeedback = f;
                                    //    }
                                    //    if (BuzzFeedback != null)
                                    //    {
                                    //        VibrationDevice.SimpleHapticsController.SendHapticFeedbackForDuration(BuzzFeedback, 1, TimeSpan.FromMilliseconds(200));
                                    //    }
                                    //}
                                    //catch (Exception ex)
                                    //{
                                    //}
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                        }

                    }
                });
            }
            catch (Exception ex)
            {
            }


            //
            // Once the asynchronous method(s) are done, close the deferral.
            //
            //_deferral.Complete();
        }

        private async void Timer_Tick(ThreadPoolTimer timer)
        {
            Debug.WriteLine("tick" + Environment.TickCount);

            await UpdateBatteryInfo();
            this.timer = ThreadPoolTimer.CreateTimer(Timer_Tick, TimeSpan.FromMinutes(1));
        }

        private async void FileReadTimer_Tick(ThreadPoolTimer timer)
        {
            Debug.WriteLine("ReadFile");
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.KnownFolders.PicturesLibrary; //.ApplicationData.Current.LocalFolder;

            try
            {
                Windows.Storage.StorageFile dFile =
                await storageFolder.GetFileAsync("app2cloud.txt");

                var list = await Windows.Storage.FileIO.ReadLinesAsync(dFile);

                if (list.Count > 0)
                {
                    foreach (string s in list)
                    {
                        try
                        {
                            if (deviceId != null)
                            {
                                Debug.Write(".");
                                DateTime d = DateTime.UtcNow;
                                long x = d.ToFileTime();
                                await AzureIoTHub.SendDeviceToCloudMessageAsync("{\"pkey\":\"" + deviceId + "\", \"rkey\":\"" + x.ToString() + "\",\"status\":\"" + s + "\"}");


                            }
                        }
                        catch
                        {
                            Debug.Write("e");
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Debug.Write("E");
            }
            this.fileReadTimer = ThreadPoolTimer.CreateTimer(FileReadTimer_Tick, TimeSpan.FromMilliseconds(900));

        }

        private async Task UpdateBatteryInfo()
        {
            BatteryReport br = await this.GetBatteryStatus();
            DateTime d = DateTime.UtcNow;
            long x = d.ToFileTime();
            if (br == null)
            {
                await AzureIoTHub.SendDeviceToCloudMessageAsync("{\"pkey\":\"" + deviceId + "\", \"rkey\":\"" + x.ToString() + "\",\"status\":\"Idle\", \"rate\":\"0\", \"percent\":\"100\"}");
            }
            else
            {

                Debug.WriteLine(br.Status.ToString());
                Debug.WriteLine("Rate: " + br.ChargeRateInMilliwatts.Value.ToString() + "mW");
                Debug.WriteLine("Remaining: " + PowerManager.RemainingChargePercent.ToString() + "% " + PowerManager.RemainingDischargeTime.Hours.ToString() + "h " + PowerManager.RemainingDischargeTime.Minutes.ToString() + "min");
                //mt.SendData("R:"+ br.ChargeRateInMilliwatts.Value.ToString() + "P:" + PowerManager.RemainingChargePercent.ToString());

                await AzureIoTHub.SendDeviceToCloudMessageAsync("{\"pkey\":\"" + deviceId + "\", \"rkey\":\"" + x.ToString() + "\",\"status\":\"" + br.Status.ToString() + "\", \"rate\":\"" + br.ChargeRateInMilliwatts.Value.ToString() + "\", \"percent\":\"" + PowerManager.RemainingChargePercent.ToString() + "\"}");

            }//ScreenCapture screen = ScreenCapture.GetForCurrentView(); 
        }


        private async Task<BatteryReport> GetBatteryStatus()
        {
            string batteryStatus;
            int batteryPercent;

            var deviceInfo = await DeviceInformation.FindAllAsync(Battery.GetDeviceSelector());
            BatteryReport br = null;
            foreach (DeviceInformation device in deviceInfo)
            {
                try
                {
                    // Create battery object
                    var battery = await Battery.FromIdAsync(device.Id);

                    // Get report
                    var report = battery.GetReport();
                    br = report;
                    batteryStatus = report.Status.ToString();
                    if (batteryStatus == "Idle")
                    {
                        batteryStatus = PowerManager.RemainingChargePercent.ToString() + "%";
                    }

                }
                catch (Exception e)
                {
                    /* Add error handling, as applicable */
                }
            }

            batteryPercent = PowerManager.RemainingChargePercent;
            return br;
        }

        private async void PowerManager_RemainingChargePercentChanged(object sender, object e)
        {
            //await GetBatteryStatus();
        }

        private async void PowerManager_PowerSupplyStatusChanged(object sender, object e)
        {
            //await GetBatteryStatus();
        }

        private async void PowerManager_BatteryStatusChanged(object sender, object e)
        {
            //await GetBatteryStatus();
        }
    }
}
