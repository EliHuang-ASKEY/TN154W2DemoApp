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
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Core;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class AS7000HRM : Page
    {

        // Chose Acc value resource
//linda
        const int Acc_resource = 1;   //0 = Dummy , 1 = BMC156 , 2 = BHI160 

        // For PCA6800 EV
        const int AS7000_GPIO_POWER_PIN = 8;
        const int AS7000_GPIO_INT_PIN = 9;
        const string AS7000_I2C_PORT = "I2C6";
        const int AS7000_I2C_ADDRESS = 0x30;
        const int AS7000_GPIO_LEDEN_PIN = 93;      // EV3 New

        //For BMC156
        const string BMC156_I2C_PORT = "I2C1";
        const int BMC156_I2C_ADDRESS = 0x10;

        //For BHI160
        const string BHI160_I2C_PORT = "I2C1";
        const int BHI160_I2C_ADDRESS = 0x28;


        //For AS7000
        const int AS7000_REG_PROTOCOL_VERSION_MAJOR = 0x00;
        const int AS7000_REG_PROTOCOL_VERSION_MINOR = 0x01;
        const byte AS7000_REG_SW_VERSION_MAJOR = 0x02;
        const byte AS7000_REG_SW_VERSION_MINOR = 0x03;
        const byte AS7000_REG_APPLICATION_ID = 0x04;
        const byte AS7000_REG_HW_VERSION = 0x05;
        const byte AS7000_REG_RESERVED0 = 0x06;
        const byte AS7000_REG_RESERVED1 = 0x07;

        const byte AS7000_REG_ACC_SAMPLE_FREQUENCY_MSB = 8;
        const byte AS7000_REG_ACC_SAMPLE_FREQUENCY_LSB = 9;

        const byte AS7000_REG_HOST_ONE_SECOND_TIME_MSB = 10;
        const byte AS7000_REG_HOST_ONE_SECOND_TIME_LSB = 11;

        const byte AS7000_REG_ACC_SAMPLES = 14;

        const byte AS7000_REG_ACC_DATA_INDEX = 20;
        const byte AS7000_REG_ACC_DATA_X_MSB = 21;
        const byte AS7000_REG_ACC_DATA_X_LSB = 22;
        const byte AS7000_REG_ACC_DATA_Y_MSB = 23;
        const byte AS7000_REG_ACC_DATA_Y_LSB = 24;
        const byte AS7000_REG_ACC_DATA_Z_MSB = 25;
        const byte AS7000_REG_ACC_DATA_Z_LSB = 26;

        const byte AS7000_REG_HRM_STATUS = 54;
        const byte AS7000_REG_HRM_HEARTRATE = 55;
        const byte AS7000_REG_HRM_SQI = 56;
        const byte AS7000_REG_HRM_SYNC = 57;

        // AS7000_REG_APPLICATION_ID values
        const byte AS7000_APPID_IDLE = 0x00;
        const byte AS7000_APPID_LOADER = 0x01;
        const byte AS7000_APPID_HRM = 0x02;

        //For BMC156
        const byte BMC156_REG_CHIP_ID = 0x00;
        const byte BMC156_REG_FIFO_STATUS = 0x0E;
        const byte BMC156_REG_RANGE_SEL = 0x0F;
        const byte BMC156_REG_BW_SEL = 0x10;
        const byte BMC156_REG_FIFO_CONFIG_1 = 0x3E;
        const byte BMC156_REG_FIFO_DATA = 0x3F;

        // BMC156_REG_RANGE_SEL values
        const byte BMC156_RANGE_SEL_2G = 0x03;
        const byte BMC156_RANGE_SEL_4G = 0x05;
        const byte BMC156_RANGE_SEL_8G = 0x08;
        const byte BMC156_RANGE_SEL_16G = 0x0C;

        // BMC156_REG_BW_SEL values
        const byte BMC156_BW_SEL_7_81Hz = 0x08;
        const byte BMC156_BW_SEL_15_63Hz = 0x09;
        const byte BMC156_BW_SEL_31_25Hz = 0x0A;
        const byte BMC156_BW_SEL_62_50Hz = 0x0B;
        const byte BMC156_BW_SEL_125Hz = 0x0C;
        const byte BMC156_BW_SEL_250Hz = 0x0D;
        const byte BMC156_BW_SEL_500Hz = 0x0E;
        const byte BMC156_BW_SEL_1000Hz = 0x0F;

        // BMC156_REG_FIFO_CONFIG_1 values
        const byte BMC156_FIFO_CONFIG_1_MODE_BYPASS = 0x00;
        const byte BMC156_FIFO_CONFIG_1_MODE_FIFO = 0x40;
        const byte BMC156_FIFO_CONFIG_1_MODE_STREAM = 0x80;

        const byte BMC156_FIFO_CONFIG_1_DATA_SELECT_XYZ = 0x00;
        const byte BMC156_FIFO_CONFIG_1_DATA_SELECT_XONLY = 0x01;
        const byte BMC156_FIFO_CONFIG_1_DATA_SELECT_YONLY = 0x10;
        const byte BMC156_FIFO_CONFIG_1_DATA_SELECT_ZONLY = 0x11;

        //For BHI160


        const byte BHI160_REG_CHIP_ID = 0x90;

        const byte BHI160_REG_PARAMETER_WRITE_BUFFER = 0x5C;
        const byte BHY160_REG_PARAMETER_PAGE_SELECT = 0x54;
        const byte BHI160_REG_PARAMETER_REQUEST = 0x64;
        const byte BHI160_REG_PARAMETER_ACKNOWLEDGE = 0x3A;

        const byte BHI160_REG_BYTES_REMAINING_LSB = 0x38;
        const byte BHI160_REG_BYTES_REMAINING_MSB = 0x39;

        const byte BHI160_REG_BUFFER_FIFO = 0x00;    // 0x00 ~ 0x31 total 50 byte

        const byte BHI160_PARAMETER_PAGE_SELECT_SENSORS = 0x03;
        const byte BHI160_PARAMETER_REQUEST_WRITE_ACCELEROMETER = 0xE1;

        GpioPin PwrPin = null;
        GpioPin IntPin = null;
        GpioPin LedEnPin = null;
        I2cDevice HrmSensor = null, BMCSensor = null, BHISensor = null;

        bool flgStart = false;

        int protocolVersion;
        int softwareVersion;
        byte applicationId;
        byte hwRevision;

        public AS7000HRM()
        {
            this.InitializeComponent();
            RateSensorInit();
//linda 2018.0803
            appBarButton.IsEnabled = true;
        }

        int AS7000_PWR_ON()
        {
            PwrPin.Write(GpioPinValue.Low);

            // EV3 New
            LedEnPin.Write(GpioPinValue.High);

            return 0;
        }


        int AS7000_PWR_OFF()
        {
            PwrPin.Write(GpioPinValue.High);

            // EV3 New
            LedEnPin.Write(GpioPinValue.Low);

            return 0;
        }

        int AS7000_GPIO_Init()
        {
            Debug.WriteLine("AS7000_GPIO_Init start");
            var gpio = GpioController.GetDefault();
            Debug.WriteLine("gpio=" + gpio);

            if (gpio == null)
            {
                return -1;
            }

            try
            {
                GpioOpenStatus openStatus = 0;
                gpio.TryOpenPin(AS7000_GPIO_POWER_PIN, GpioSharingMode.Exclusive, out PwrPin, out openStatus);
                PwrPin.SetDriveMode(GpioPinDriveMode.Output);

                gpio.TryOpenPin(AS7000_GPIO_INT_PIN, GpioSharingMode.Exclusive, out IntPin, out openStatus);
                IntPin.SetDriveMode(GpioPinDriveMode.Input);

                //// EV3 New
                gpio.TryOpenPin(AS7000_GPIO_LEDEN_PIN, GpioSharingMode.Exclusive, out LedEnPin, out openStatus);
                LedEnPin.SetDriveMode(GpioPinDriveMode.Output);
            }
            catch (Exception ex)
            {
                return -1;
            }

            return 1;
        }


        async Task<bool> AS7000_I2C_Init()
        {
            var i2cSettings = new I2cConnectionSettings(AS7000_I2C_ADDRESS); // connect to default address; 
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            //string deviceSelector = I2cDevice.GetDeviceSelector("I2C5");
            string deviceSelector = I2cDevice.GetDeviceSelector(AS7000_I2C_PORT);
            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            Debug.WriteLine("i2cDeviceControllers:" + i2cDeviceControllers);
            HrmSensor = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
            Debug.WriteLine("HrmSensor:" + HrmSensor);
            return false;
        }

        async Task<bool> BMC156_I2C_Init()
        {
            var i2cSettings = new I2cConnectionSettings(BMC156_I2C_ADDRESS); // connect to default address; 
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            //string deviceSelector = I2cDevice.GetDeviceSelector("I2C5");
            string deviceSelector = I2cDevice.GetDeviceSelector(BMC156_I2C_PORT);
            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            Debug.WriteLine("i2cDeviceControllers:" + i2cDeviceControllers);
            BMCSensor = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
            Debug.WriteLine("BMCSensor:" + BMCSensor);
            return false;
        }

        async Task<bool> BHI160_I2C_Init()
        {
            var i2cSettings = new I2cConnectionSettings(BHI160_I2C_ADDRESS); // connect to default address; 
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            //string deviceSelector = I2cDevice.GetDeviceSelector("I2C5");
            string deviceSelector = I2cDevice.GetDeviceSelector(BHI160_I2C_PORT);
            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            Debug.WriteLine("i2cDeviceControllers:" + i2cDeviceControllers);
            BHISensor = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
            Debug.WriteLine("BHISensor:" + BHISensor);
            return false;
        }

        int AS7000_INT_ON()
        {
            IntPin.ValueChanged += IntPin_ValueChanged;
            return 0;
        }

        int AS7000_INT_OFF()
        {
            IntPin.ValueChanged -= IntPin_ValueChanged;

            return 0;
        }


        // AS7000_REG_HRM_STATUS - Reg Value
        //
        // 0   OK
        // 1   Illegal parameter handed to a function (e.g. a 0-pointer)
        // 2   Data was lost (e.g. execution of a function did take too long and ADC data was not read fast enough)
        // 4   Accelerometer not accessible
        //

        // AS7000_REG_HRM_HEARTRATE - Reg Value
        //
        // // The heartrate result – units = bpm
        //

        // AS7000_REG_HRM_SQI - Reg Value
        //
        // 0..2    Q++    Excellent signal quality
        // 3..5    Q+     Good signal quality
        // 6..8    Q0     Acceptable signal quality
        // 9..10   Q-     Poor signal quality
        // 11..20  Q--    Worst signal quality
        // 21..253       Reserved - currently not used
        // 254     No-P  No presence detected (no object in proximity)
        // 255     ???   No signal quality calculated yet
        //

        // AS7000_REG_HRM_SYNC - Reg Value
        //
        // [1...255] incrementing each new HRM-result (roll-over to 1)
        // 0 = no HRM-result
        //

        byte hrmStatus = 0;
        byte hrmHeartRate = 0;
        byte hrmSqi = 0;
        byte hrmSync = 0;

        bool blockI2C = false;

        unsafe private void IntPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {

            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                if (blockI2C)
                {
                    Debug.Write("*B*");
                    return;
                }
                Debug.Write("\nINT:   ");
                if (flgStart == false)
                {
                    return;
                }
                blockI2C = true;

                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    byte hrmStatus;
                    byte hrmHeartRate;
                    byte hrmSqi;
                    byte hrmSync;

                    //ACC_BMC156
                    byte AccSamples;
                    byte[] BMC156bAccData = new byte[120]; // 20*6

                    //ACC_BHI160
                    byte[] bRegDataSize = new byte[2];
                    UInt16 wRegDataSize;
                    int wRegDataSize_quo;
                    int wRegDataSize_Rem;
                    byte[] bFifoData = new byte[500];
                    UInt16 wFifoDataSize;
                    byte[] bTemp = new byte[50];
                    byte[] BHI160bAccData = new byte[120];
                    int iAccDataIndex;
                    int i;


                    Debug.WriteLine("1");
                    // Host one-second time - 0=not-determined
                    I2C_WriteRegData(HrmSensor, AS7000_REG_HOST_ONE_SECOND_TIME_MSB, 0x00);
                    Task.Delay(1);
                    I2C_WriteRegData(HrmSensor, AS7000_REG_HOST_ONE_SECOND_TIME_LSB, 0x00);
                    Task.Delay(1);

                    if (Acc_resource == 0)
                    {
                        //Acc Samples as 20
                        I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLES, 20);

                        //Write Dummy Acc Data to AS7000
                        for (i = 0; i < 20; i++)
                        {
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_INDEX, (byte)(i + 1));
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_MSB, 0x00);
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_LSB, (byte)(0x07 + i));
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_MSB, 0x00);
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_LSB, (byte)(0x11 + i));
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_MSB, 0x07);
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_LSB, 0xD0 + 1);
                            Task.Delay(1);
                        }
                    }
                    else if (Acc_resource == 1)
                    {
                        //get BMC156 FIFO 
                        I2C_ReadRegData(BMCSensor, BMC156_REG_FIFO_STATUS, &AccSamples);

                        //read BMC156 Acc data
                        I2C_ReadRegMultiData(BMCSensor, BMC156_REG_FIFO_DATA, BMC156bAccData, 120);

                        Debug.WriteLine("BMC156bAccData:" + BitConverter.ToString(BMC156bAccData));
                        //clear/reset Acc BMC156 fifo
                        I2C_WriteRegData(BMCSensor, BMC156_REG_FIFO_CONFIG_1, (BMC156_FIFO_CONFIG_1_MODE_FIFO | BMC156_FIFO_CONFIG_1_DATA_SELECT_XYZ));

                        //Acc Samples as 20
                        I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLES, 20);

                        //Write Acc BMC156 Data to AS7000
                        for (i = 0; i < 20; i++)
                        {
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_INDEX, (byte)(i + 1));
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_MSB, BMC156bAccData[1 + (i * 6)]);
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_LSB, (byte)(BMC156bAccData[0 + (i * 6)] & 0xF0));
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_MSB, BMC156bAccData[3 + (i * 6)]);
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_LSB, (byte)(BMC156bAccData[2 + (i * 6)] & 0xF0));
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_MSB, BMC156bAccData[5 + (i * 6)]);
                            Task.Delay(1);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_LSB, (byte)(BMC156bAccData[4 + (i * 6)] & 0xF0));
                            Task.Delay(1);

                        }
                    }
                    else if (Acc_resource == 2)
                    {
                        I2C_ReadRegMultiData(BHISensor, BHI160_REG_BYTES_REMAINING_LSB, bRegDataSize, 2);
                        //TraceLog("bRegDataSize[0] = 0x%02x \r\n", bRegDataSize[0]);
                        //TraceLog("bRegDataSize[1] = 0x%02x \r\n", bRegDataSize[1]);
                        wRegDataSize = (UInt16)((bRegDataSize[1] << 8) | bRegDataSize[0]);
                        Debug.WriteLine("wRegDataSize:" + wRegDataSize);
                        //TraceLog(L"wRegDataSize = %d \r\n", wRegDataSize);
                        wRegDataSize_quo = (int)wRegDataSize / 50;
                        Debug.WriteLine("wRegDataSize_quo:" + wRegDataSize_quo);
                        wRegDataSize_Rem = (int)wRegDataSize % 50;
                        Debug.WriteLine("wRegDataSize_Rem:" + wRegDataSize_Rem);
                        //TraceLog(L"wRegDataSize_quo = %d wRegDataSize_Rem = %d \r\n", wRegDataSize_quo, wRegDataSize_Rem);

                        if (wRegDataSize <= 500)
                        {
                            wFifoDataSize = wRegDataSize;
                        }
                        else
                        {
                            wFifoDataSize = 500;
                        }

                        Array.Clear(bFifoData, 0, 500);

                        Debug.WriteLine("wFifoDataSize:" + wFifoDataSize);

                        for (i = 0; i < wRegDataSize_quo; i++)
                        {
                            if (i < 10)
                            {

                                I2C_ReadRegMultiData_3(BHISensor, BHI160_REG_BUFFER_FIFO, bFifoData, 50, i);
                            }
                            else
                            {
                                I2C_ReadRegMultiData(BHISensor, BHI160_REG_BUFFER_FIFO, bTemp, 50);
                            }
                        }
                        if (wRegDataSize_Rem > 0)
                        {
                            if (i < 10)
                            {

                                I2C_ReadRegMultiData_3(BHISensor, BHI160_REG_BUFFER_FIFO, bFifoData, wRegDataSize_Rem, i);
                            }
                            else
                            {
                                I2C_ReadRegMultiData(BHISensor, BHI160_REG_BUFFER_FIFO, bTemp, wRegDataSize_Rem);
                            }
                        }
                        //TraceLog(L"wFifoDataSize = %d \r\n", wFifoDataSize);
                        Debug.WriteLine("bFifoData:" + BitConverter.ToString(bFifoData));
                        // Parser Fifo Data
                        iAccDataIndex = 0;
                        Array.Clear(BHI160bAccData, 0, 120);
                        for (i = 0; i < (wFifoDataSize - 10); i++)
                        {
                            //Debug.WriteLine("bFifoData[i]:"+bFifoData[i]);
                            //Debug.WriteLine("bFifoData[i + 3]:"+ bFifoData[i+3]);
                            if (bFifoData[i] == 0xf6 && bFifoData[i + 3] == 0x21)
                            {
                                if (iAccDataIndex >= 20)
                                {
                                    //TraceLog(L"iAccDataIndex >= 20 Skip \r\n");
                                    break;
                                }

                                Array.Copy(bFifoData, i + 4, BHI160bAccData, iAccDataIndex * 6, 6 * sizeof(byte));
                                iAccDataIndex++;
                                i = i + 4 + 6;
                            }
                        }

                        //Acc Samples as 20
                        I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLES, 20);

                        Int16 x_raw = 0;
                        Int16 y_raw = 0;
                        Int16 z_raw = 0;
                        float x_data = 0;
                        float y_data = 0;
                        float z_data = 0;

                        // Write Acc BHI160 Data to AS7000
                        for (i = 0; i < 20; i++)
                        {
                            x_raw = (Int16)(((UInt16)BHI160bAccData[0 + (i * 6)]) | ((UInt16)(BHI160bAccData[1 + (i * 6)] << 8)));
                            y_raw = (Int16)(((UInt16)BHI160bAccData[2 + (i * 6)]) | ((UInt16)(BHI160bAccData[3 + (i * 6)] << 8)));
                            z_raw = (Int16)(((UInt16)BHI160bAccData[4 + (i * 6)]) | ((UInt16)(BHI160bAccData[5 + (i * 6)] << 8)));
                            x_data = (float)x_raw / 32768.0f * 4.0f;
                            y_data = (float)y_raw / 32768.0f * 4.0f;
                            z_data = (float)z_raw / 32768.0f * 4.0f;
                            Debug.WriteLine("X:" + x_data + "Y:" + y_data + "Z:" + z_data);

                            //TraceLog(L"X[%d] = M 0x%02x L 0x%02x Lmk 0x%02x \r\n", i, bAccData[1 + (i * 6)], bAccData[0 + (i * 6)], (bAccData[0 + (i * 6)] & 0xF0));
                            //TraceLog(L"Y[%d] = M 0x%02x L 0x%02x Lmk 0x%02x \r\n", i, bAccData[3 + (i * 6)], bAccData[2 + (i * 6)], (bAccData[2 + (i * 6)] & 0xF0));
                            //TraceLog(L"Z[%d] = M 0x%02x L 0x%02x Lmk 0x%02x \r\n", i, bAccData[5 + (i * 6)], bAccData[4 + (i * 6)], (bAccData[4 + (i * 6)] & 0xF0));

                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_INDEX, (byte)(i + 1));
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_MSB, BHI160bAccData[1 + (i * 6)]);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_LSB, (byte)(BHI160bAccData[0 + (i * 6)] & 0xF0));
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_MSB, BHI160bAccData[3 + (i * 6)]);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_LSB, (byte)(BHI160bAccData[2 + (i * 6)] & 0xF0));
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_MSB, BHI160bAccData[5 + (i * 6)]);
                            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_LSB, (byte)(BHI160bAccData[4 + (i * 6)] & 0xF0));
                        }

                    }


                    hrmStatus = I2C_ReadRegData_2(HrmSensor, AS7000_REG_HRM_STATUS);
                    Task.Delay(1);
                    hrmHeartRate = I2C_ReadRegData_2(HrmSensor, AS7000_REG_HRM_HEARTRATE);
                    Task.Delay(1);
                    hrmSqi = I2C_ReadRegData_2(HrmSensor, AS7000_REG_HRM_SQI);
                    Task.Delay(1);
                    hrmSync = I2C_ReadRegData_2(HrmSensor, AS7000_REG_HRM_SYNC);
                    Task.Delay(1);
                    Debug.Write("\nHRM: Status=" + hrmStatus.ToString() + " Rate=" + hrmHeartRate.ToString() + " Sqi=" + hrmSqi.ToString() + " sync=" + hrmSync.ToString());
                    blockI2C = false;

                    if (hrmSync != 0)
                    {
                        textBox_HRM_Value.Text = hrmHeartRate.ToString();
                        textBox_SQI_Value.Text = hrmSqi.ToString();
                        if (hrmSqi == 254)
                        {
                            ProgBar_SQI.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 0, 0));
                            ProgBar_SQI.Value = 0;
                        }
                        else
                        {
                            ProgBar_SQI.Background = new SolidColorBrush(ColorHelper.FromArgb(30, 255, 255, 255));
                            if (hrmSqi <= 2)
                                ProgBar_SQI.Value = 5;
                            else if (hrmSqi <= 5)
                                ProgBar_SQI.Value = 4;
                            else if (hrmSqi <= 8)
                                ProgBar_SQI.Value = 3;
                            else if (hrmSqi <= 10)
                                ProgBar_SQI.Value = 2;
                            else if (hrmSqi <= 20)
                                ProgBar_SQI.Value = 1;
                            else
                                ProgBar_SQI.Value = 0;
                        }

                    }
                });



            }
        }


        public string GetHeartRate()
        {
            return hrmHeartRate.ToString();
        }



        int AS7000_Get_Protocol_Version()
        {
            byte pbMajor = I2C_ReadRegData_2(HrmSensor, AS7000_REG_PROTOCOL_VERSION_MAJOR);
            byte pbMinor = I2C_ReadRegData_2(HrmSensor, AS7000_REG_PROTOCOL_VERSION_MINOR);
            protocolVersion = (int)pbMajor * 256 + (int)pbMinor;
            return 0;
        }

        int AS7000_Get_SW_Version()
        {
            byte pbMajor = I2C_ReadRegData_2(HrmSensor, AS7000_REG_SW_VERSION_MAJOR);
            byte pbMinor = I2C_ReadRegData_2(HrmSensor, AS7000_REG_SW_VERSION_MINOR);
            Debug.WriteLine("pbMajor:" + pbMajor);
            Debug.WriteLine("pbMinor:" + pbMinor);
            softwareVersion = (int)pbMajor * 256 + (int)pbMinor;

            if (Acc_resource == 0)
            {
                textBox_SW_Ver_Value.Text = pbMajor.ToString() + "." + pbMinor.ToString();
            }
            else if (Acc_resource == 1)
            {
                textBox_SW_Ver_Value.Text = pbMajor.ToString() + "." + pbMinor.ToString() + ".BMC156";
            }
            else if (Acc_resource == 2)
            {
                textBox_SW_Ver_Value.Text = pbMajor.ToString() + "." + pbMinor.ToString() + ".BHI160";
            }

            return 0;
        }

        int AS7000_Get_HW_Version()
        {
            hwRevision = I2C_ReadRegData_2(HrmSensor, AS7000_REG_HW_VERSION);
            return 0;
        }

        int AS7000_Get_App_ID()
        {
            applicationId = I2C_ReadRegData_2(HrmSensor, AS7000_REG_APPLICATION_ID);
            return 0;
        }



        public async void RateSensorInit()
        {
            
            Debug.WriteLine("RateSensorInit start");
//linda 2018.0803
if (Acc_resource == 1)
            {
                await BMC156_I2C_Init();
                await Task.Delay(300);
                BMC156_Init();
            }
            else if (Acc_resource == 2)
            {
                await BHI160_I2C_Init();
                await Task.Delay(300);
                BHI160_Init();
            }
            AS7000_GPIO_Init();
            AS7000_PWR_ON();

            await Task.Delay(1000); // If we do not sleep, we will have the FallingEdge event.
            //AS7000_INT_ON();

            await AS7000_I2C_Init();
            await Task.Delay(300);
            AS7000_Get_Protocol_Version();
            AS7000_Get_SW_Version();
            AS7000_Get_HW_Version();
            AS7000_Get_App_ID();

            

            return;
        }

        private void BMC156_Init()
        {
            // Set ACC BMC156 as +-16G with bandwith 15.63Hz
            I2C_WriteRegData(BMCSensor, BMC156_REG_RANGE_SEL, BMC156_RANGE_SEL_16G);
            I2C_WriteRegData(BMCSensor, BMC156_REG_BW_SEL, BMC156_BW_SEL_15_63Hz);
            // Set ACC BMC156 as FIFO mode with xyz data
            I2C_WriteRegData(BMCSensor, BMC156_REG_FIFO_CONFIG_1, (BMC156_FIFO_CONFIG_1_MODE_FIFO | BMC156_FIFO_CONFIG_1_DATA_SELECT_XYZ));
        }

        unsafe private void BHI160_Init()
        {
            byte bAck;
            byte[] bConfigDataBuf = { 0x14, 0x00, 0xe8, 0x03, 0x00, 0x00, 0x00, 0x00 };  // bit[0:1] sample rate , bit[2:3] max report latency

            Debug.WriteLine("bConfigDataBuf:" + BitConverter.ToString(bConfigDataBuf));
            I2C_WriteRegMultiData(BHISensor, BHI160_REG_PARAMETER_WRITE_BUFFER, bConfigDataBuf, 8);
            I2C_WriteRegData(BHISensor, BHY160_REG_PARAMETER_PAGE_SELECT, BHI160_PARAMETER_PAGE_SELECT_SENSORS);
            I2C_WriteRegData(BHISensor, BHI160_REG_PARAMETER_REQUEST, BHI160_PARAMETER_REQUEST_WRITE_ACCELEROMETER);
            I2C_ReadRegData(BHISensor, BHI160_REG_PARAMETER_ACKNOWLEDGE, &bAck);
        }


        public async void RateMonitorON()
        {
            Debug.WriteLine("RateMonitorON start");
            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLE_FREQUENCY_MSB, 0x4e);
            I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLE_FREQUENCY_LSB, 0x20);
            AS7000_INT_ON();
            await Task.Delay(1000);
            I2C_WriteRegData(HrmSensor, AS7000_REG_APPLICATION_ID, AS7000_APPID_HRM);
            await Task.Delay(300);
            AS7000_Get_App_ID();
            flgStart = true;
            Debug.WriteLine("RateMonitorON flgStart=" + flgStart);
        }

        public async void RateMonitorOff()
        {
            flgStart = false;
            AS7000_INT_OFF();
            await Task.Delay(1000);
            I2C_WriteRegData(HrmSensor, AS7000_REG_APPLICATION_ID, AS7000_APPID_IDLE);
            await Task.Delay(300);
            AS7000_Get_App_ID();
        }


        unsafe private void I2C_ReadRegData(I2cDevice device, byte bRegister, byte* bData)
        {
            I2cTransferResult result;
            byte[] wBuf = new byte[1];
            wBuf[0] = bRegister;
            byte[] rBuf = new byte[1];
            result = device.WriteReadPartial(wBuf, rBuf);

            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                *bData = 0;
            }

            *bData = rBuf[0];
        }

        unsafe private void I2C_ReadRegMultiData(I2cDevice device, byte bRegister, byte[] bBuf, int iLen)
        {
            I2cTransferResult result;
            byte[] wBuf = new byte[1];
            wBuf[0] = bRegister;
            byte[] rBuf = new byte[iLen];
            result = device.WriteReadPartial(wBuf, rBuf);


            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                bBuf = null;
            }

            Array.Copy(rBuf, 0, bBuf, 0, iLen * sizeof(byte));

            string ok = BitConverter.ToString(rBuf);
            //Debug.WriteLine("ok:"+ok);

        }

        unsafe private void I2C_ReadRegMultiData_3(I2cDevice device, byte bRegister, byte[] bBuf, int iLen, int i)
        {
            I2cTransferResult result;
            byte[] wBuf = new byte[1];
            wBuf[0] = bRegister;
            byte[] rBuf = new byte[iLen];
            result = device.WriteReadPartial(wBuf, rBuf);


            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                bBuf = null;
            }

            Array.Copy(rBuf, 0, bBuf, 0 + (i * 50), iLen * sizeof(byte));

            string ok = BitConverter.ToString(rBuf);
            Debug.WriteLine("ok:" + ok);

        }


        byte I2C_ReadRegData_2(I2cDevice device, byte reg)
        {
            try
            {
                if (device == null) return 0;
                device.Write(new byte[] { reg });
                byte[] i2cBuffer = new byte[1];
                device.Read(i2cBuffer);
                //UInt16 r = (UInt16)((i2cBuffer[0] << 8) | (i2cBuffer[1]));
                return i2cBuffer[0];
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        void I2C_WriteRegData(I2cDevice device, byte reg, byte value)
        {
            try
            {
                if (device == null) return;
                device.Write(new byte[] { reg, value });
            }
            catch (Exception ex)
            {
            }
        }

        unsafe private void I2C_WriteRegMultiData(I2cDevice device, byte bRegister, byte[] bBuf, int iLen)
        {
            I2cTransferResult result;
            int iwBufLen = iLen + 1;
            byte[] wBuf = new byte[iwBufLen];
            wBuf[0] = bRegister;

            Array.Copy(bBuf, 0, wBuf, 1, iLen * sizeof(byte));

            Debug.WriteLine("writemulti:" + BitConverter.ToString(wBuf));

            result = device.WritePartial(wBuf);

            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                return;
            }

        }

        private void toggleSwitch_HRM_Toggled(object sender, RoutedEventArgs e)
        {
            bool flgOn;

            flgOn = toggleSwitch_HRM.IsOn;

            if (flgOn)
            {
                RateMonitorON();
            }
            else
            {
                RateMonitorOff();
                textBox_HRM_Value.Text = "";
                textBox_SQI_Value.Text = "";
                ProgBar_SQI.Background = new SolidColorBrush(ColorHelper.FromArgb(30, 255, 255, 255));
                ProgBar_SQI.Value = 0;
            }

            return;

        }

        unsafe private void BHI160_DeInit()
        {
            byte bAck;
            byte[] bConfigDataBuf = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            I2C_WriteRegMultiData(BHISensor, BHI160_REG_PARAMETER_WRITE_BUFFER, bConfigDataBuf, 8);
            I2C_WriteRegData(BHISensor, BHY160_REG_PARAMETER_PAGE_SELECT, BHI160_PARAMETER_PAGE_SELECT_SENSORS);
            I2C_WriteRegData(BHISensor, BHI160_REG_PARAMETER_REQUEST, BHI160_PARAMETER_REQUEST_WRITE_ACCELEROMETER);
            I2C_ReadRegData(BHISensor, BHI160_REG_PARAMETER_ACKNOWLEDGE, &bAck);

            BHISensor.Dispose();
        }

        private async void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            toggleSwitch_HRM.IsOn = false;
            toggleSwitch_HRM.IsEnabled = false;
            appBarButton.IsEnabled = false;
            //stop monitor
            RateMonitorOff();
            textBox_HRM_Value.Text = "";
            textBox_SQI_Value.Text = "";
            ProgBar_SQI.Background = new SolidColorBrush(ColorHelper.FromArgb(30, 255, 255, 255));
            ProgBar_SQI.Value = 0;


            //Free the BMC156 Resources
            if (Acc_resource == 1)
            {
                BMCSensor.Dispose();
            }
            //Free the BHI160 Resources
            else if (Acc_resource == 2)
            {
                BHI160_DeInit();
            }


            //this.Frame.Navigate(typeof(MainPage));


            await Task.Delay(1500);


            if (this.Frame.CanGoBack)
            {
                //Free the AS7000 Resources
//null linda2018.0803
                PwrPin.Dispose();
                PwrPin = null;
                IntPin.Dispose();
                IntPin = null;
                LedEnPin.Dispose();
                LedEnPin = null;
                HrmSensor.Dispose();
                HrmSensor = null;

                this.Frame.GoBack();
            }

        }

    }
}
