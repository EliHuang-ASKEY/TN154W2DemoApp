# TN154W2DemoApp
TN154W2 defaule APP source code For Visual Studio 2017-- update by 2018/06/19

# Feature List
### Clock:
    IOTCoreMasterApp\LocalApps\AppClock.xaml
    Show Clock.

### Setting:
    IOTCoreMasterApp\LocalApps\Setting.xaml
    WiFi switch,Bluetooth switch,
    The audio switch when the app is started

### Wifi & BT:
    IOTCoreMasterApp\LocalApps\ConnectMainPackage.xaml
    WiFi & BT scan,connect, and show wlan mac & BT mac

### FlashLED
    IOTCoreMasterApp\LocalApps\FlashLight.xaml
    Flash light switch(GPIO112 control)

### Camera:
    IOTCoreMasterApp\LocalApps\camera2.xaml
    Camera preview, capture, Focus setting, Zoom 

### AudioRecord:
    IOTCoreMasterApp\LocalApps\AudioRecord.xaml
     Audio Record

### MediaPlayer:
    IOTCoreMasterApp\LocalApps\MediaPlayer.xaml
    Media Player

### Sensor: Accelerometer, Gyrometer
    IOTCoreMasterApp\LocalApps\Accelerometer.xaml
    IOTCoreMasterApp\LocalApps\SensorGyrometer.xaml
    Accelerometer & Gyrometer
    
### Vibrator
    IOTCoreMasterApp\LocalApps\Vibrator.xaml
    Vibration Device Class
    Namespace:Windows.Devices.Haptics
    Windows 10 requirements
    Device family	Windows 10 Creators Update (introduced v10.0.15063.0)
*[Link](https://docs.microsoft.com/en-us/uwp/api/windows.devices.haptics.vibrationdevice)
  
### NFC
    IOTCoreMasterApp\LocalApps\NFCTest.xaml
    NFC tag detect only
    
### Location
    IOTCoreMasterApp\LocalApps\Location2.xaml
    GPS positioning
    
### Brightness
    IOTCoreMasterApp\LocalApps\DeviceContrl.xaml
    Brightness Override Class
    Namespace:Windows.Graphics.Display
    Windows 10 requirements
    Device family	Windows 10 Creators Update (introduced v10.0.15063.0)
*[Link](https://docs.microsoft.com/en-us/uwp/api/windows.graphics.display.brightnessoverride)
    
### Message
    IOTCoreMasterApp\LocalApps\ShowMessage.xaml
    Show HID information from Keyboard, BarCodescan....
    
### Information
    IOTCoreMasterApp\LocalApps\About.xaml
    Show information about TN154W2. Get register value via GetRegValue.exe.
    please reference 
*[External Process Launcher](https://developer.microsoft.com/en-us/windows/iot/samples/externalprocesslauncher)
    
### Shutdown
    IOTCoreMasterApp\LocalApps\shutdown.xaml
    Shutdown & Reboot
    
