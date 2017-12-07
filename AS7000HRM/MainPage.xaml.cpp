// Copyright (c) Microsoft. All rights reserved.

//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"

using namespace AS7000HRM;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::UI::Core;
using namespace Windows::Devices::Enumeration;
using namespace Windows::Devices::Gpio;
using namespace Windows::Devices::I2c;
using namespace concurrency;

#if 0
// For PCA6800 ES
#define AS7000_GPIO_POWER_PIN    24   // ES
#define AS7000_GPIO_INT_PIN      96   // ES
#define AS7000_I2C_PORT       "I2C2"  // ES
#define BMC156_I2C_PORT       "I2C1"  // ES
#else
// For PCA6800 EV
#define AS7000_GPIO_POWER_PIN     8   // EV
#define AS7000_GPIO_INT_PIN       9   // EV
#define AS7000_I2C_PORT       "I2C6"  // EV
#define BMC156_I2C_PORT       "I2C1"  // EV
#define AS7000_GPIO_LEDEN_PIN    93   // EV3 New
#endif
#define AS7000_I2C_ADDRESS    0x30
#define BMC156_I2C_ADDRESS    0x10

#define AS7000_REG_PROTOCOL_VERSION_MAJOR     0x00
#define AS7000_REG_PROTOCOL_VERSION_MINOR     0x01
#define AS7000_REG_SW_VERSION_MAJOR           0x02
#define AS7000_REG_SW_VERSION_MINOR           0x03
#define AS7000_REG_APPLICATION_ID             0x04
#define AS7000_REG_HW_VERSION                 0x05
#define AS7000_REG_RESERVED0                  0x06
#define AS7000_REG_RESERVED1                  0x07

#define AS7000_REG_ACC_SAMPLE_FREQUENCY_MSB      8
#define AS7000_REG_ACC_SAMPLE_FREQUENCY_LSB      9

#define AS7000_REG_HOST_ONE_SECOND_TIME_MSB     10
#define AS7000_REG_HOST_ONE_SECOND_TIME_LSB     11

#define AS7000_REG_ACC_SAMPLES                  14

#define AS7000_REG_ACC_DATA_INDEX               20
#define AS7000_REG_ACC_DATA_X_MSB               21
#define AS7000_REG_ACC_DATA_X_LSB               22
#define AS7000_REG_ACC_DATA_Y_MSB               23
#define AS7000_REG_ACC_DATA_Y_LSB               24
#define AS7000_REG_ACC_DATA_Z_MSB               25
#define AS7000_REG_ACC_DATA_Z_LSB               26

#define AS7000_REG_HRM_STATUS           54
#define AS7000_REG_HRM_HEARTRATE        55
#define AS7000_REG_HRM_SQI              56
#define AS7000_REG_HRM_SYNC             57

// AS7000_REG_APPLICATION_ID values
#define AS7000_APPID_IDLE       0x00
#define AS7000_APPID_LOADER     0x01
#define AS7000_APPID_HRM        0x02

#define BMC156_REG_CHIP_ID                    0x00
#define BMC156_REG_FIFO_STATUS                0x0E
#define BMC156_REG_RANGE_SEL                  0x0F
#define BMC156_REG_BW_SEL                     0x10
#define BMC156_REG_FIFO_CONFIG_1              0x3E
#define BMC156_REG_FIFO_DATA                  0x3F

// BMC156_REG_RANGE_SEL values
#define BMC156_RANGE_SEL_2G      0x03
#define BMC156_RANGE_SEL_4G      0x05
#define BMC156_RANGE_SEL_8G      0x08
#define BMC156_RANGE_SEL_16G     0x0C

// BMC156_REG_BW_SEL values
#define BMC156_BW_SEL_7_81Hz     0x08
#define BMC156_BW_SEL_15_63Hz    0x09
#define BMC156_BW_SEL_31_25Hz    0x0A
#define BMC156_BW_SEL_62_50Hz    0x0B
#define BMC156_BW_SEL_125Hz      0x0C
#define BMC156_BW_SEL_250Hz      0x0D
#define BMC156_BW_SEL_500Hz      0x0E
#define BMC156_BW_SEL_1000Hz     0x0F

// BMC156_REG_FIFO_CONFIG_1 values
#define BMC156_FIFO_CONFIG_1_MODE_BYPASS           0x00
#define BMC156_FIFO_CONFIG_1_MODE_FIFO             0x40
#define BMC156_FIFO_CONFIG_1_MODE_STREAM           0x80

#define BMC156_FIFO_CONFIG_1_DATA_SELECT_XYZ       0x00
#define BMC156_FIFO_CONFIG_1_DATA_SELECT_XONLY     0x01
#define BMC156_FIFO_CONFIG_1_DATA_SELECT_YONLY     0x10
#define BMC156_FIFO_CONFIG_1_DATA_SELECT_ZONLY     0x11

void TraceLog(LPCWSTR pszFormat, ...)
{
	WCHAR szTextBuf[256];
	va_list args;
	va_start(args, pszFormat);

	StringCchVPrintf(szTextBuf, _countof(szTextBuf), pszFormat, args);

	OutputDebugString(szTextBuf);
}

int MainPage::I2C_ReadRegData(I2cDevice ^device, BYTE bRegister, BYTE *bData)
{
	I2cTransferResult result;
	Array<unsigned char> ^wBuf = { bRegister };
	auto rBuf = ref new Array<BYTE>(1);

	result = device->WriteReadPartial(wBuf, rBuf);

	if (result.Status != I2cTransferStatus::FullTransfer)
	{
		TraceLog(L" I2C_ReadRegData fail \r\n");
		*bData = 0;
		return -1;
	}

	*bData = rBuf[0];

	return 0;
}

int MainPage::I2C_WriteRegData(I2cDevice ^device, BYTE bRegister, BYTE bData)
{
	I2cTransferResult result;
	Array<unsigned char> ^wBuf = { bRegister , bData };

	result = device->WritePartial(wBuf);

	if (result.Status != I2cTransferStatus::FullTransfer)
	{
		TraceLog(L" I2C_WriteRegData fail \r\n");
		return -1;
	}

	return 0;
}

int MainPage::I2C_ReadRegMultiData(I2cDevice^ device, BYTE bRegister, BYTE *bBuf, int iLen)
{
	I2cTransferResult result;
	Array<unsigned char> ^wBuf = { bRegister };
	auto rBuf = ref new Array<BYTE>(iLen);

	result = device->WriteReadPartial(wBuf, rBuf);

	if (result.Status != I2cTransferStatus::FullTransfer)
	{
		TraceLog(L" I2C_ReadRegMultiData fail \r\n");
		memcpy(bBuf, 0x00, iLen);
		return -1;
	}

	memcpy(bBuf, &rBuf[0], iLen);

	return 0;
}

int MainPage::I2C_WriteRegMultiData(I2cDevice^ device, BYTE bRegister, BYTE *bBuf, int iLen)
{
	I2cTransferResult result;
	int iwBufLen = iLen + 1;
	auto wBuf = ref new Array<BYTE>(iwBufLen);

	wBuf[0] = bRegister;
	memcpy(&wBuf[1], bBuf, iLen);

	result = device->WritePartial(wBuf);
	if (result.Status != I2cTransferStatus::FullTransfer)
	{
		TraceLog(L" I2C_WriteRegMultiData fail \r\n");
		return -1;
	}

	return 0;
}

int MainPage::BMC156_Get_CHIP_ID(BYTE *pbChipID)
{
	I2C_ReadRegData(AccSensor, BMC156_REG_CHIP_ID, pbChipID);

	return 0;
}

task<void> MainPage::BMC156_I2C_Init()
{
	String^ i2cDeviceSelector = I2cDevice::GetDeviceSelector(BMC156_I2C_PORT);

	return create_task(DeviceInformation::FindAllAsync(i2cDeviceSelector))
		.then([this](DeviceInformationCollection^ devices)
	{
		auto ACC_settings = ref new I2cConnectionSettings(BMC156_I2C_ADDRESS);

		// If this next line crashes with an OutOfBoundsException,
		// then the problem is that no I2C devices were found.
		//
		// If the next line crashes with Access Denied, then the problem is
		// that access to the I2C device (HTU21D) is denied.
		//
		// The call to FromIdAsync will also crash if the settings are invalid.
		//TraceLog(L" BMC156_I2C_Init ~ 1 \r\n");
		return I2cDevice::FromIdAsync(devices->GetAt(0)->Id, ACC_settings);
	}).then([this](I2cDevice^ i2cDevice)
	{
		// i2cDevice will be nullptr if there is a sharing violation on the device.
		// This will result in an access violation in Timer_Tick below.
		AccSensor = i2cDevice;
		//TraceLog(L" BMC156_I2C_Init ~ 2 \r\n");
	});
}

int MainPage::BMC156_I2C_DeInit()
{
	delete AccSensor;
	AccSensor = nullptr;

	return 0;
}


int MainPage::AS7000_Get_Protocol_Version(BYTE *pbMajor, BYTE *pbMinor)
{
	I2C_ReadRegData(HrmSensor, AS7000_REG_PROTOCOL_VERSION_MAJOR, pbMajor);
	I2C_ReadRegData(HrmSensor, AS7000_REG_PROTOCOL_VERSION_MINOR, pbMinor);

	return 0;
}

int MainPage::AS7000_Get_SW_Version(BYTE *pbMajor, BYTE *pbMinor)
{
	I2C_ReadRegData(HrmSensor, AS7000_REG_SW_VERSION_MAJOR, pbMajor);
	I2C_ReadRegData(HrmSensor, AS7000_REG_SW_VERSION_MINOR, pbMinor);

	return 0;
}

int MainPage::AS7000_Get_HW_Version(BYTE *pbHwVer)
{
	I2C_ReadRegData(HrmSensor, AS7000_REG_HW_VERSION, pbHwVer);

	return 0;
}

int MainPage::AS7000_Get_App_ID(BYTE *pbAppID)
{
	I2C_ReadRegData(HrmSensor, AS7000_REG_APPLICATION_ID, pbAppID);

	return 0;
}

task<void> MainPage::AS7000_I2C_Init()
{
	String^ i2cDeviceSelector = I2cDevice::GetDeviceSelector(AS7000_I2C_PORT);

	return create_task(DeviceInformation::FindAllAsync(i2cDeviceSelector))
		.then([this](DeviceInformationCollection^ devices)
	{
		auto HRM_settings = ref new I2cConnectionSettings(AS7000_I2C_ADDRESS);

		// If this next line crashes with an OutOfBoundsException,
		// then the problem is that no I2C devices were found.
		//
		// If the next line crashes with Access Denied, then the problem is
		// that access to the I2C device (HTU21D) is denied.
		//
		// The call to FromIdAsync will also crash if the settings are invalid.
		//TraceLog(L" AS7000_I2C_Init ~ 1 \r\n");
		return I2cDevice::FromIdAsync(devices->GetAt(0)->Id, HRM_settings);
	}).then([this](I2cDevice^ i2cDevice)
	{
		// i2cDevice will be nullptr if there is a sharing violation on the device.
		// This will result in an access violation in Timer_Tick below.
		HrmSensor = i2cDevice;
		//TraceLog(L" AS7000_I2C_Init ~ 2 \r\n");
	});
}

int MainPage::AS7000_I2C_DeInit()
{
	delete HrmSensor;
	HrmSensor = nullptr;

	return 0;
}

int MainPage::AS7000_PWR_ON()
{
	TraceLog(L" AS7000_PWR_ON ~ \r\n");

	PwrPin->Write(GpioPinValue::Low);

	// EV3 New
	LedEnPin->Write(GpioPinValue::High);

	return 0;
}

int MainPage::AS7000_PWR_OFF()
{
	TraceLog(L" AS7000_PWR_OFF ~ \r\n");

	PwrPin->Write(GpioPinValue::High);
	
	// EV3 New
	LedEnPin->Write(GpioPinValue::Low);

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

void MainPage::Pin_ValueChanged(GpioPin ^sender, GpioPinValueChangedEventArgs ^e)
{
	if (e->Edge == GpioPinEdge::FallingEdge)
	{
		//TraceLog(L" FallingEdge \r\n");
		if (flgStart == FALSE)
		{
			TraceLog(L" Skip first FallingEdge \r\n");
			return;
		}

		Dispatcher->RunAsync(
			CoreDispatcherPriority::Normal,
			ref new Windows::UI::Core::DispatchedHandler([this]()
		{
			BYTE hrmStatus;
			BYTE hrmHeartRate;
			BYTE hrmSqi;
			BYTE hrmSync;
			BYTE AccSamples;
			BYTE bAccData[120]; // 20*6
			int  i;

			// Host one-second time - 0=not-determined
			I2C_WriteRegData(HrmSensor, AS7000_REG_HOST_ONE_SECOND_TIME_MSB, 0x00);
			I2C_WriteRegData(HrmSensor, AS7000_REG_HOST_ONE_SECOND_TIME_LSB, 0x00);

			// get Acc BMC156 total fifo samples, 15.63Hz always have 29~32 Samples per second
			I2C_ReadRegData(AccSensor, BMC156_REG_FIFO_STATUS, &AccSamples);
			//TraceLog(L" Sampels 0x%02x %d \r\n", AccSamples, AccSamples);

			// read Acc Data from BMC156
			I2C_ReadRegMultiData(AccSensor, BMC156_REG_FIFO_DATA, bAccData, 120);

			//clear/reset Acc BMC156 fifo
			I2C_WriteRegData(AccSensor, BMC156_REG_FIFO_CONFIG_1, (BMC156_FIFO_CONFIG_1_MODE_FIFO | BMC156_FIFO_CONFIG_1_DATA_SELECT_XYZ));

			// Acc Samples as 20
			I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLES, 20);

#if 0
			// Using dummy Acc Data
			for (i = 0; i < 20; i++)
			{
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_INDEX, i+1);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_MSB, 0x00);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_LSB, 0x07+i);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_MSB, 0x00);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_LSB, 0x11+i);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_MSB, 0x07);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_LSB, 0xD0+1);
			}
#else
			// Using Acc BMC156 Data
			for (i = 0; i < 20; i++)
			{
				//TraceLog(L"X[%d] = M 0x%02x L 0x%02x Lmk 0x%02x \r\n", i, bAccData[1 + (i * 6)], bAccData[0 + (i * 6)], (bAccData[0 + (i * 6)] & 0xF0));
				//TraceLog(L"Y[%d] = M 0x%02x L 0x%02x Lmk 0x%02x \r\n", i, bAccData[3 + (i * 6)], bAccData[2 + (i * 6)], (bAccData[2 + (i * 6)] & 0xF0));
				//TraceLog(L"Z[%d] = M 0x%02x L 0x%02x Lmk 0x%02x \r\n", i, bAccData[5 + (i * 6)], bAccData[4 + (i * 6)], (bAccData[4 + (i * 6)] & 0xF0));

				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_INDEX, i + 1);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_MSB,  bAccData[1 + (i * 6)]);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_X_LSB, (bAccData[0 + (i * 6)] & 0xF0));
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_MSB,  bAccData[3 + (i * 6)]);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Y_LSB, (bAccData[2 + (i * 6)] & 0xF0));
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_MSB,  bAccData[5 + (i * 6)]);
				I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_DATA_Z_LSB, (bAccData[4 + (i * 6)] & 0xF0));
			}
#endif

			I2C_ReadRegData(HrmSensor, AS7000_REG_HRM_STATUS, &hrmStatus);
			I2C_ReadRegData(HrmSensor, AS7000_REG_HRM_HEARTRATE, &hrmHeartRate);
			I2C_ReadRegData(HrmSensor, AS7000_REG_HRM_SQI, &hrmSqi);
			I2C_ReadRegData(HrmSensor, AS7000_REG_HRM_SYNC, &hrmSync);

			//TraceLog(L" 1> %d \r\n", hrmStatus);
			//TraceLog(L" 2> %d \r\n", hrmHeartRate);
			//TraceLog(L" 3> %d \r\n", hrmSqi);
			//TraceLog(L" 4> %d \r\n", hrmSync);
			if (hrmSync != 0)
			{
				textBox_HRM_Value->Text = hrmHeartRate.ToString();
				textBox_SQI_Value->Text = hrmSqi.ToString();
				if (hrmSqi == 254)
				{
					ProgBar_SQI->Background = ref new SolidColorBrush(ColorHelper::FromArgb(255, 255, 0, 0));
					ProgBar_SQI->Value = 0;
				}
				else
				{
					ProgBar_SQI->Background = ref new SolidColorBrush(ColorHelper::FromArgb(33, 0, 0, 0));
					if (hrmSqi <= 2)
						ProgBar_SQI->Value = 5;
					else if (hrmSqi <= 5)
						ProgBar_SQI->Value = 4;
					else if (hrmSqi <= 8)
						ProgBar_SQI->Value = 3;
					else if (hrmSqi <= 10)
						ProgBar_SQI->Value = 2;
					else if (hrmSqi <= 20)
						ProgBar_SQI->Value = 1;
					else
						ProgBar_SQI->Value = 0;
				}
			}
			else
			{
				TraceLog(L" no HRM-result \r\n");
			}
		}));
	}
	else
	{
		//TraceLog(L" RisingEdge \r\n");
	}

	return;
}

int MainPage::AS7000_INT_ON()
{
	TraceLog(L" AS7000_INT_ON ~ \r\n");

	valueChangedToken = IntPin->ValueChanged += ref new TypedEventHandler<GpioPin^, GpioPinValueChangedEventArgs^>(this, &MainPage::Pin_ValueChanged);

	return 0;
}

int MainPage::AS7000_INT_OFF()
{
	TraceLog(L" AS7000_INT_OFF ~ \r\n");

	IntPin->ValueChanged -= valueChangedToken;

	return 0;
}

int MainPage::AS7000_GPIO_Init()
{
	auto gpio = GpioController::GetDefault();

	if (gpio == nullptr)
	{
		return -1;
	}

	PwrPin = gpio->OpenPin(AS7000_GPIO_POWER_PIN);
	PwrPin->SetDriveMode(GpioPinDriveMode::Output);

	IntPin = gpio->OpenPin(AS7000_GPIO_INT_PIN);
	IntPin->SetDriveMode(GpioPinDriveMode::Input);

	// EV3 New
	LedEnPin = gpio->OpenPin(AS7000_GPIO_LEDEN_PIN);
	LedEnPin->SetDriveMode(GpioPinDriveMode::Output);

	return 0;
}

int MainPage::AS7000_GPIO_DeInit()
{
	delete PwrPin;
	PwrPin = nullptr;

	delete IntPin;
	IntPin = nullptr;

	// EV3 New
	delete LedEnPin;
	LedEnPin = nullptr;

	return 0;
}

int MainPage::AS7000_Init()
{
	AS7000_GPIO_Init();

	AS7000_PWR_ON();
	//Sleep(100); // If we do not sleep, we will have the FallingEdge event.
	//AS7000_INT_ON();

	AS7000_I2C_Init().then([this]()
	{
		BYTE protocolRevMajor;
		BYTE protocolRevMinor;
		BYTE swRevMsb;
		BYTE swRevLsb;
		BYTE applicationId;
		BYTE hwRevision;

		TraceLog(L" AS7000_I2C_Init ~ \r\n");

		AS7000_Get_Protocol_Version(&protocolRevMajor, &protocolRevMinor);
		AS7000_Get_SW_Version(&swRevMsb, &swRevLsb);
		AS7000_Get_HW_Version(&hwRevision);
		AS7000_Get_App_ID(&applicationId);

		textBox_Pro_Ver_Value->Text = protocolRevMajor.ToString() + "." + protocolRevMinor.ToString() ;
		textBox_SW_Ver_Value->Text = ((swRevMsb & 0xc0) >> 6).ToString() + "." + (swRevMsb & 0x3f).ToString() + "." + swRevLsb.ToString();
		textBox_HW_Ver_Value->Text = hwRevision.ToString();
		textBox_APP_ID_Value->Text = applicationId.ToString();
	});

	return 0;
}

int MainPage::AS7000_DeInit()
{
	AS7000_I2C_DeInit();

	//AS7000_INT_OFF();

	AS7000_PWR_OFF();

	AS7000_GPIO_DeInit();

	return 0;
}

int MainPage::BMC156_Init()
{
	BMC156_I2C_Init().then([this]()
	{
		BYTE ChipId;

		TraceLog(L" BMC156_Init ~ \r\n");

		BMC156_Get_CHIP_ID(&ChipId);

		//TraceLog(L"BMC156 chip id = 0x%02x \r\n", ChipId);

		// Set ACC BMC156 as +-16G with bandwith 15.63Hz
		I2C_WriteRegData(AccSensor, BMC156_REG_RANGE_SEL, BMC156_RANGE_SEL_16G);
		I2C_WriteRegData(AccSensor, BMC156_REG_BW_SEL, BMC156_BW_SEL_15_63Hz);
		// Set ACC BMC156 as FIFO mode with xyz data
		I2C_WriteRegData(AccSensor, BMC156_REG_FIFO_CONFIG_1, (BMC156_FIFO_CONFIG_1_MODE_FIFO | BMC156_FIFO_CONFIG_1_DATA_SELECT_XYZ));
	});

	return 0;
}

int MainPage::BMC156_DeInit()
{
	BMC156_I2C_DeInit();

	return 0;
}

void AS7000HRM::MainPage::OnPageLoaded(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	//TraceLog(L" OnPageLoaded \r\n");
	
	AS7000_Init();

	BMC156_Init();

	return;
}

void AS7000HRM::MainPage::OnPageUnLoad(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	//TraceLog(L" OnPageUnLoad \r\n");

	//BMC156_DeInit();

	//AS7000_DeInit();

	return;
}

MainPage::MainPage()
{
    InitializeComponent();
}

void AS7000HRM::MainPage::toggleSwitch_HRM_Toggled(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	bool flgOn;
	BYTE applicationId;

	flgOn = toggleSwitch_HRM->IsOn;

	if (flgOn)
	{
		TraceLog(L" HRM Start \r\n");
		// Set Accelerometer Sample-Frequency as 20Hz=20000=0x4e20 (unit=0.001Hz) 
		I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLE_FREQUENCY_MSB, 0x4e);
		I2C_WriteRegData(HrmSensor, AS7000_REG_ACC_SAMPLE_FREQUENCY_LSB, 0x20);
		AS7000_INT_ON();
		I2C_WriteRegData(HrmSensor, AS7000_REG_APPLICATION_ID, AS7000_APPID_HRM);
		Sleep(100);
		AS7000_Get_App_ID(&applicationId);
		textBox_APP_ID_Value->Text = applicationId.ToString();
		flgStart = TRUE;
	}
	else
	{
		TraceLog(L" HRM Stop \r\n");
		flgStart = FALSE;
		AS7000_INT_OFF();
		Sleep(1000);
		I2C_WriteRegData(HrmSensor, AS7000_REG_APPLICATION_ID, AS7000_APPID_IDLE);
		Sleep(100);
		AS7000_Get_App_ID(&applicationId);
		textBox_APP_ID_Value->Text = applicationId.ToString();

		textBox_HRM_Value->Text = "";
		textBox_SQI_Value->Text = "";
		ProgBar_SQI->Background = ref new SolidColorBrush(ColorHelper::FromArgb(33, 0, 0, 0));
		ProgBar_SQI->Value = 0;
	}

	return;
}

void AS7000HRM::MainPage::button_Exit_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	BMC156_DeInit();

	AS7000_DeInit();

	terminate();

	return;
}
