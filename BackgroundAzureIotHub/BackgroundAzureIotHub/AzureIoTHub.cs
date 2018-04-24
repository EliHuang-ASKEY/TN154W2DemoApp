// ---------------------------------------------------------------------------------
// Copyright(c) Microsoft Open Technologies, Inc.All rights reserved.
//  
// The MIT License(MIT)
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
//  
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software. 
//   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE. 
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Devices.Tpm;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics;

class MessageListItem
{
    public string message = null;
}


static class AzureIoTHub
{
    //
    // This sample assumes the device has been connected to Azure with the IoT Dashboard
    //
    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static int counter = 0;

    public static List<MessageListItem> msgList = new List<MessageListItem>(); 

    public static async Task<string> TestHubConnection(bool sendRestartMessage, string restartMessage)
    {
        string deviceid = null; 
        try
        {
            TpmDevice myDevice = new TpmDevice(0); // Use logical device 0 on the TPM
            string hubUri = myDevice.GetHostName();
            deviceid = myDevice.GetDeviceId();
            string sasToken = myDevice.GetSASToken();
            if ((hubUri.Length == 0) || (sasToken.Length == 0)) return null;
        }
        catch (Exception ex)
        {
            return null;
        }

        if (sendRestartMessage)
        {
            await SendDeviceToCloudMessageAsync(restartMessage);
            return deviceid;
        }
        return deviceid;
    }

    public static async Task<bool> SendDeviceToCloudMessageAsync(string str)
    {
        try
        {
            TpmDevice myDevice = new TpmDevice(0); // Use logical device 0 on the TPM
            string hubUri = myDevice.GetHostName();
            string deviceId = myDevice.GetDeviceId();
            string hwID = myDevice.GetHardwareDeviceId(); 
            string sasToken = myDevice.GetSASToken();

            DeviceClient deviceClient = DeviceClient.Create(
                hubUri,
                AuthenticationMethodFactory.
                    CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Mqtt_WebSocket_Only);


            //string m = "{\"pkey\":\"" + deviceId + "\", \"msg\":"+ str +"}";

            var message = new Message(Encoding.ASCII.GetBytes(str));

            await deviceClient.SendEventAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        try
        {
            while (true)
            {
                TpmDevice myDevice = new TpmDevice(0); // Use logical device 0 on the TPM by default
                string hubUri = myDevice.GetHostName();
                string deviceId = myDevice.GetDeviceId();
                string sasToken = myDevice.GetSASToken();

                var deviceClient = DeviceClient.Create(
                    hubUri,
                    AuthenticationMethodFactory.
                        CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Mqtt_WebSocket_Only);

                //            while (true)
                //            {
                Message receivedMessage = null;
                Debug.Write(".");
                try
                {
                    receivedMessage = await deviceClient.ReceiveAsync();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("----- ERROR awaiting ReceiveAsync");
                }

                if (receivedMessage != null)
                {
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    await deviceClient.CompleteAsync(receivedMessage);
                    deviceClient.Dispose();
                    return messageData;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("----ERROR Receive:" + ex.Message + " " + ex.InnerException.Message);
            return null;
        }
    }
}