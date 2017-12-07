// Copyright (c) Microsoft. All rights reserved.

//
// MainPage.xaml.h
// Declaration of the MainPage class.
//

#pragma once

#include "MainPage.g.h"

namespace AS7000HRM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public ref class MainPage sealed
    {
		public:
			MainPage();

		private:
			void OnPageLoaded(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			void OnPageUnLoad(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			void toggleSwitch_HRM_Toggled(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			void button_Exit_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);

		private:
			int  AS7000_Init(void);
			int  AS7000_DeInit(void);
			int  AS7000_GPIO_Init(void);
			int  AS7000_GPIO_DeInit(void);
			int  AS7000_PWR_ON(void);
			int  AS7000_PWR_OFF(void);
			int  AS7000_INT_ON(void);
			int  AS7000_INT_OFF(void);
			void Pin_ValueChanged(Windows::Devices::Gpio::GpioPin ^sender, Windows::Devices::Gpio::GpioPinValueChangedEventArgs ^e);

			int I2C_ReadRegData(Windows::Devices::I2c::I2cDevice ^device, BYTE bRegister, BYTE *bData);
			int I2C_WriteRegData(Windows::Devices::I2c::I2cDevice ^device, BYTE bRegister, BYTE bData);
			int I2C_ReadRegMultiData(Windows::Devices::I2c::I2cDevice^ device, BYTE bRegister, BYTE *bBuf, int iLen);
			int I2C_WriteRegMultiData(Windows::Devices::I2c::I2cDevice^ device, BYTE bRegister, BYTE *bBuf, int iLen);

			concurrency::task<void> AS7000_I2C_Init(void);
			int AS7000_I2C_DeInit(void);
			int AS7000_Get_Protocol_Version(BYTE *pbMajor, BYTE *pbMinor);
			int AS7000_Get_SW_Version(BYTE *pbMajor, BYTE *pbMinor);
			int AS7000_Get_HW_Version(BYTE *pbHwVer);
			int AS7000_Get_App_ID(BYTE *pbAppID);

			int BMC156_Init(void);
			int BMC156_DeInit(void);
			concurrency::task<void> BMC156_I2C_Init(void);
			int BMC156_I2C_DeInit(void);
			int BMC156_Get_CHIP_ID(BYTE *pbChipID);

		private:
			Windows::Devices::Gpio::GpioPin ^PwrPin;
			Windows::Devices::Gpio::GpioPin ^IntPin;
			Windows::Devices::Gpio::GpioPin ^LedEnPin;  // EV3 New
			Windows::Foundation::EventRegistrationToken valueChangedToken{};
			Windows::Devices::I2c::I2cDevice ^HrmSensor;
			Windows::Devices::I2c::I2cDevice ^AccSensor;
			BOOL flgStart = FALSE;
	};
}
