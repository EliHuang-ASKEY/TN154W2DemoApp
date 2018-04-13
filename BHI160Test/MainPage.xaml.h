//
// MainPage.xaml.h
// MainPage 類別的宣告。
//

#pragma once

#include "MainPage.g.h"

namespace BHI160Test
{
	/// <summary>
	/// 可以在本身使用或巡覽至框架內的空白頁面。
	/// </summary>
	public ref class MainPage sealed
	{
	public:
		MainPage();
		
		
		


	private:
		void MainPage::BHI160_Test_accelerometer_gyro();
		void MainPage::Back(Platform::Object^ sender, Platform::Object ^args);
		void MainPage::BHI160_GetVersion();
		void MainPage::BHI160_Test_gesture();
		void MainPage::GoBack_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		void MainPage::Send_Logfile();
		Windows::UI::Xaml::DispatcherTimer ^Back_timer;
		


	};
}
