//
// MainPage.xaml.cpp
// MainPage 類別的實作。
//

#include "pch.h"
#include "MainPage.xaml.h"

//BHI160Test_S
#include <ppltasks.h>
#include <collection.h>
#include <string>
#include <vector>
#include <sstream>
#include <iostream>
#include <cwctype>

#include <windows.h>
#include <strsafe.h>
#include <wrl.h>
#include <windows.foundation.h>
#include <windows.devices.gpio.h>
#include <conio.h>
//BHI160Test_E


using namespace BHI160Test;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

//BHI160Test_S
using namespace Windows::Devices::I2c;
using namespace ABI::Windows::Devices::Gpio;
using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Wrappers;
using namespace Windows::System::Threading;
using namespace Windows::Storage;
using namespace Windows::Storage::Streams;
using namespace Windows::Networking;
using namespace Windows::Networking::Sockets;
using namespace Windows::UI::Xaml::Automation::Peers;
using namespace Windows::UI::Xaml::Automation::Provider;
//BHI160Test_E

#if 1  // BHI160 Test
extern "C"
{
#include "bhy_uc_driver.h"

	extern int accelerometer_remapping_example_main(void);
	extern int gesture_recognition_example_main(void);
	extern int rotation_vector_example_main(void);
	extern int fifo_watermark_example_main(void);
	extern int activity_recognition_example_main(void);
	extern int debug_string_example_main(void);
	extern int fw_download_example_main(void);
	extern eric_value_return get_acc_value(void);
	extern eric_value_return get_gyro_value(void);
#define SKIP_RESET_CMD
	//#define DEBUG_BHI160_I2C_DATA

#define BHI160_I2C_PORT       "I2C1"
#define BHI160_GPIO_INT_PIN     96

	Windows::Devices::I2c::I2cDevice ^gpBHI160_i2c_device;
	ComPtr<IGpioPin> gBHI160_gpio_device;
	IGpioPin *gpBHI160_gpio_pin;
	HANDLE gthread_handle;
}
#endif

float Min_X = 0, Min_Y = 0, Min_Z = 0, Max_X = 0, Max_Y = 0, Max_Z = 0;

float gyro_Min_X = 0, gyro_Min_Y = 0, gyro_Min_Z = 0, gyro_Max_X = 0, gyro_Max_Y = 0, gyro_Max_Z = 0;

ThreadPoolTimer^  DelayTimer;


void TraceLog(LPCWSTR pszFormat, ...)
{
	WCHAR szTextBuf[256];
	va_list args;
	va_start(args, pszFormat);

	StringCchVPrintf(szTextBuf, _countof(szTextBuf), pszFormat, args);

	OutputDebugString(szTextBuf);
}



void ListI2cControllers()
{
	using namespace Windows::Devices::Enumeration;
	using namespace Platform::Collections;

	String^ friendlyNameProperty =
		L"System.DeviceInterface.Spb.ControllerFriendlyName";
	auto properties = ref new Vector<String^>();
	properties->Append(friendlyNameProperty);
	auto dis = concurrency::create_task(DeviceInformation::FindAllAsync(
		I2cDevice::GetDeviceSelector(),
		properties)).get();
	if (dis->Size < 1) {
		std::wcout << L"There are no I2C controllers on this system.\n";
		return;
	}

	wprintf(L"  FriendlyName DeviceId\n");
	for (const auto& di : dis) {
		wprintf(
			L"  %12s %s\n",
			((String^)di->Properties->Lookup(friendlyNameProperty))->Data(),
			di->Id->Data());
	}
}

I2cDevice^ MakeDevice(int slaveAddress, _In_opt_ String^ friendlyName)
{
	using namespace Windows::Devices::Enumeration;

	String^ aqs;
	if (friendlyName)
		aqs = I2cDevice::GetDeviceSelector(friendlyName);
	else
		aqs = I2cDevice::GetDeviceSelector();

	auto dis = concurrency::create_task(DeviceInformation::FindAllAsync(aqs)).get();
	if (dis->Size < 1) {
		//throw wexception(L"I2C controller not found");
	}

	String^ id = dis->GetAt(0)->Id;
#if 0 // Ammore Modify - for 400kHz
	auto device = concurrency::create_task(I2cDevice::FromIdAsync(
		id,
		ref new I2cConnectionSettings(slaveAddress))).get();
#else
	auto i2cSettings = ref new I2cConnectionSettings(slaveAddress);
	i2cSettings->BusSpeed = I2cBusSpeed::FastMode;
	auto device = concurrency::create_task(I2cDevice::FromIdAsync(
		id,
		i2cSettings)).get();
#endif

	if (!device) {
		std::wostringstream msg;
		msg << L"Slave address 0x" << std::hex << slaveAddress << L" on bus " << id->Data() <<
			L" is in use. Please ensure that no other applications are using I2C.";
		//throw wexception(msg.str());
	}

	return device;
}

ComPtr<IGpioController> MakeDefaultController()
{
	ComPtr<IGpioController> controller;

	// get the activation factory
	ComPtr<IGpioControllerStatics> controllerStatics;
	HRESULT hr = GetActivationFactory(
		HStringReference(RuntimeClass_Windows_Devices_Gpio_GpioController).Get(),
		&controllerStatics);
	

	hr = controllerStatics->GetDefault(controller.GetAddressOf());
	

	if (!controller) {
		//throw wexception(L"GPIO is not available on this system");
	}

	return controller;
}

ComPtr<IGpioPin> MakePin(int PinNumber)
{
	auto controller = MakeDefaultController();

	ComPtr<IGpioPin> pin;
	HRESULT hr = controller->OpenPin(PinNumber, pin.GetAddressOf());

	return pin;
}


#if 1  // BHI160 Test
void BHI160_Init()
{
	gpBHI160_i2c_device = MakeDevice(0x28, BHI160_I2C_PORT);

	//gpBHI160_i2c_device->ConnectionSettings->BusSpeed = I2cBusSpeed::FastMode;
	gBHI160_gpio_device = MakePin(BHI160_GPIO_INT_PIN);
	gpBHI160_gpio_pin = gBHI160_gpio_device.Get();

	HRESULT hr = gpBHI160_gpio_pin->SetDriveMode(GpioPinDriveMode_Input);
	if (FAILED(hr))
		TraceLog(L" Failed to set drive mode. (hr = 0x%x) \r\n ", hr);

	TraceLog(L" BHI160_Init \r\n");

	return;
}

void OEM_mdelay(uint32_t ul_dly_ticks)
{
	Sleep(ul_dly_ticks);

	return;
}

int OEM_IsQuit()
{
	char cIn;



	OEM_mdelay(2);

	return 0;
}


int OEM_get_pin_level()
{
	HRESULT hr;
	GpioPinValue pinValue;

	hr = gpBHI160_gpio_pin->Read(&pinValue);
	if (FAILED(hr))
	{
		TraceLog(L" Failed to set read pin. (hr = 0x%x) \r\n", hr);
		return -1;
	}

	return pinValue;
}

int8_t OEM_i2c_read_internal(uint8_t dev_addr, uint8_t reg_addr, uint8_t *rx_data, uint16_t length)
{
	I2cTransferResult result;
	Array<unsigned char> ^wBuf = { reg_addr };
	Array<unsigned char> ^rBuf = ref new Array<BYTE>(length);

	result = gpBHI160_i2c_device->WriteReadPartial(wBuf, rBuf);
	if (result.Status != I2cTransferStatus::FullTransfer)
	{
		TraceLog(L"OEM_i2c_read_internal-wr fail \r\n");
		return -1;
	}

	memcpy(rx_data, rBuf->Data, length);

#ifdef DEBUG_BHI160_I2C_DATA
	int i;

	printf("[Rx] ~~ reg_addr = 0x%x length = 0x%x ~~ \r\n", reg_addr, length);
	for (i = 0; i < length; i++)
	{
		printf(" 0x%x ", rx_data[i]);
	}
	printf("\r\n\r\n");
#endif

	return 0;
}

int8_t OEM_i2c_write_internal(uint8_t dev_addr, uint8_t reg_addr, uint8_t *reg_data, uint16_t length)
{
	I2cTransferResult result;
	Array<unsigned char> ^wBuf = ref new Array<BYTE>((length + 1));

#ifdef SKIP_RESET_CMD
	if (reg_addr == 0x9b)
	{
		printf(" Skip reset command \r\n");
		return 0;
	}
#endif

	wBuf->Data[0] = reg_addr;
	memcpy(&wBuf->Data[1], reg_data, length);

#ifdef DEBUG_BHI160_I2C_DATA
	int i;

	printf("[Wx] ~~ reg_addr = 0x%x length = 0x%x ~~ \r\n", reg_addr, length);
	for (i = 0; i <= length; i++)
	{
		printf(" 0x%x ", wBuf->Data[i]);
	}
	printf("\r\n\r\n");
#endif

	result = gpBHI160_i2c_device->WritePartial(wBuf);
	if (result.Status != I2cTransferStatus::FullTransfer)
	{
		printf(" OEM_i2c_write_internal fail \r\n");
		return -1;
	}

	return 0;
}

void MainPage::BHI160_GetVersion()
{
	BYTE bPID;
	u16  RomVersion;
	u16  RamVersion;
	u8   RevisionID;

	BHI160_Init();

	

	bhy_initialize_support();
	bhy_get_product_id(&bPID);
	TraceLog(L"PID = 0x%x \r\n", bPID);
	//ggtest->Text = bPID.ToString();

	bhy_get_rom_version(&RomVersion);
	TraceLog(L"RomVersion = 0x%x \r\n", RomVersion);

	bhy_get_ram_version(&RamVersion);
	TraceLog(L"RamVersion = 0x%x \r\n", RamVersion);

	bhy_get_revision_id(&RevisionID);
	TraceLog(L"RevisionID = 0x%x \r\n", RevisionID);



	return;
}


void MainPage::BHI160_Test_accelerometer_gyro()
{
	BYTE bPID;

	BHI160_Init();

	bhy_initialize_support();
	bhy_get_product_id(&bPID);

	TraceLog(L"PID = 0x%x \r\n", bPID);


	gthread_handle = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)accelerometer_remapping_example_main, NULL, 0, NULL);

	return;
}


void MainPage::BHI160_Test_gesture()
{
	BYTE bPID;

	BHI160_Init();

	bhy_initialize_support();
	bhy_get_product_id(&bPID);
	printf("PID = 0x%x \r\n", bPID);

	gesture_recognition_example_main();

	return;
}

void BHI160_Test_rotation()
{
	BYTE bPID;

	BHI160_Init();

	bhy_initialize_support();
	bhy_get_product_id(&bPID);
	printf("PID = 0x%x \r\n", bPID);

	rotation_vector_example_main();

	return;
}

void BHI160_Test_fifo_watermark()
{
	BYTE bPID;

	BHI160_Init();

	bhy_initialize_support();
	bhy_get_product_id(&bPID);
	printf("PID = 0x%x \r\n", bPID);

	fifo_watermark_example_main();

	return;
}

void BHI160_Test_activity_recognition()
{
	BYTE bPID;

	BHI160_Init();

	bhy_initialize_support();
	bhy_get_product_id(&bPID);
	printf("PID = 0x%x \r\n", bPID);

	activity_recognition_example_main();

	return;
}

void BHI160_Test_debug_string()
{
	BYTE bPID;

	BHI160_Init();

	bhy_initialize_support();
	bhy_get_product_id(&bPID);
	printf("PID = 0x%x \r\n", bPID);

	debug_string_example_main();

	return;
}

void BHI160_fw_downlaod()
{
	clock_t start_time, end_time;
	float total_time = 0;

	BHI160_Init();

	start_time = clock();
	fw_download_example_main();
	end_time = clock();
	total_time = (float)(end_time - start_time) / CLOCKS_PER_SEC;

	printf("total_time = %f  \r\n", total_time);

	return;
}
#endif


void MainPage::Send_Logfile() 
{
	StorageFolder^ storageFolder = ApplicationData::Current->LocalFolder;

	concurrency::create_task(storageFolder->GetFileAsync("TestSensor.txt")).then([](StorageFile^ sampleFile)
	{
		return FileIO::ReadBufferAsync(sampleFile);

	}).then([](Streams::IBuffer^ buffer)
	{
	
		StreamSocket^ socket = ref new StreamSocket();

		HostName^ hostName = ref new HostName("192.168.50.2");

		Concurrency::task<void> connectTask(socket->ConnectAsync(hostName, "2200"));

		connectTask.then([=](Concurrency::task<void> prevTask)
		{
			DataWriter^ writer = ref new DataWriter(socket->OutputStream);

			//Send message lenght and message text
			writer->WriteUInt32(buffer->Length);
			TraceLog(L"len1:%d\r\n", buffer->Length);
			writer->WriteBuffer(buffer);

			//Send Log file name
			String^ test = "TestSensor.txt";
			writer->WriteUInt32(writer->MeasureString(test));
			writer->WriteString(test);
			
			//Push message in Stream
			Concurrency::create_task(writer->StoreAsync()).then([socket, buffer, test](Concurrency::task<unsigned int> writeTask)
			{

			});

		});	

	});

}


MainPage::MainPage()
{
	InitializeComponent();
	BHI160_Test_accelerometer_gyro();
	
	//Set Timer delay
	TimeSpan delay;
	delay.Duration = 2500000; // 10,000,000 = 1 Second


	//Create a Timer Back to Main Menu after 15 sec 
	Back_timer = ref new Windows::UI::Xaml::DispatcherTimer;
	Back_timer->Tick += ref new Windows::Foundation::EventHandler<Platform::Object^>(this, &MainPage::Back);
	TimeSpan t;
	t.Duration = 15 * 10000000; //10000000= 1 sec
	Back_timer->Interval = t;
	Back_timer->Start();

	
	//Use Timer to polling g-sensor
	DelayTimer = ThreadPoolTimer::CreatePeriodicTimer(
		ref new TimerElapsedHandler([=](ThreadPoolTimer^ source)
	{

		//get g-sensor value
		eric_value_return result = get_acc_value();

		//get gyro-sensor value
		eric_value_return result_gyro = get_gyro_value();

		//compare g-sensor value
		if(result.x > Max_X)
		{
			Max_X = result.x;
		}
		else if (result.x < Min_X)
		{
			Min_X = result.x;
		}

		if (result.y > Max_Y)
		{
			Max_Y = result.y;
		}
		else if (result.y < Min_Y)
		{
			Min_Y = result.y;
		}

		if (result.z > Max_Z)
		{
			Max_Z = result.z;
		}
		else if (result.z < Min_Z)
		{
			Min_Z = result.z;
		}


		//compare gyro-sensor value
		if (result_gyro.x > gyro_Max_X)
		{
			gyro_Max_X = result_gyro.x;
		}
		else if (result_gyro.x < gyro_Min_X)
		{
			gyro_Min_X = result_gyro.x;
		}

		if (result_gyro.y > gyro_Max_Y)
		{
			gyro_Max_Y = result_gyro.y;
		}
		else if (result_gyro.y < gyro_Min_Y)
		{
			gyro_Min_Y = result_gyro.y;
		}

		if (result_gyro.z > gyro_Max_Z)
		{
			gyro_Max_Z = result_gyro.z;
		}
		else if (result_gyro.z < gyro_Min_Z)
		{
			gyro_Min_Z = result_gyro.z;
		}


		//update to UI
		Dispatcher->RunAsync(Windows::UI::Core::CoreDispatcherPriority::Normal,
			ref new Windows::UI::Core::DispatchedHandler([=]()
		{
			//show g-sensor value
			Acc_X->Text = result.x.ToString() + "Min:" + Min_X + "Max:" + Max_X;
			Acc_Y->Text = result.y.ToString() + "Min:" + Min_Y + "Max:" + Max_Y;
			Acc_Z->Text = result.z.ToString() + "Min:" + Min_Z + "Max:" + Max_Z;

			//show gyro-sensor value
			Gyro_X->Text = result_gyro.x.ToString() + "Min:" + gyro_Min_X + "Max:" + gyro_Max_X;
			Gyro_Y->Text = result_gyro.y.ToString() + "Min:" + gyro_Min_Y + "Max:" + gyro_Max_Y;
			Gyro_Z->Text = result_gyro.z.ToString() + "Min:" + gyro_Min_Z + "Max:" + gyro_Max_Z;
		}));


	}), delay);


}

void MainPage::Back(Platform::Object^ sender, Platform::Object ^args)
{
	//Close two Timer
	DelayTimer->Cancel();
	Back_timer->Stop();
	Sleep(1000);

	//create log text file
	StorageFolder^ storageFolder = ApplicationData::Current->LocalFolder;
	concurrency::create_task(storageFolder->CreateFileAsync("TestSensor.txt", CreationCollisionOption::ReplaceExisting));


	//write value in the file
	concurrency::create_task(storageFolder->GetFileAsync("TestSensor.txt")).then([](StorageFile^ sampleFile)
	{
		String^ acc;
		String^ gyro;
		String^ message;

		acc = "Accelerometer X( " + Min_X + " , " + Max_X + " ) , Y( " + Min_Y + " , " + Max_Y + " ) , Z( " + Min_Z + " , " + Max_Z + " )\r\n";

		gyro = "Gyrometer X( " + gyro_Min_X + " , " + gyro_Max_X + " ) , Y( " + gyro_Min_Y + " , " + gyro_Max_Y + " ) , Z( " + gyro_Min_Z + " , " + gyro_Max_Z + " )\r\n";

		message = acc + gyro;

		FileIO::WriteTextAsync(sampleFile, message);
	});

	Sleep(2000);

	//Send log to PC server
	Send_Logfile();

	Sleep(2000);
	Application::Current->Exit();

}

void MainPage::GoBack_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	//Close two Timer
	DelayTimer->Cancel();
	Back_timer->Stop();
	Sleep(1000);
	
	Application::Current->Exit();
}



