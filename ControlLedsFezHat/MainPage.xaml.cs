using GHIElectronics.UWP.Shields;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace ControlLedsFezHat
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private I2cDevice bridgeDevice;
        static DeviceClient deviceClient;
        static string iotHubUri = "iothubpruebascloud.azure-devices.net";
        static string deviceKey = "IZip2TDe5US3Uc8W8SUJByD5JkjuIsPsihDmQRLWVmk=";
        static string deviceId = "devicetest";
        private FEZHAT hat;
        private DispatcherTimer timer;
        private string light;
        private int messageId = 1;

        public MainPage()
        {
            this.InitializeComponent();

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Amqp);

            Setup();
            ReceiveMessage();
        }

        private async void Setup()
        {
            this.hat = await FEZHAT.CreateAsync();

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(1000);
            this.timer.Tick += OnTick;
            this.timer.Start();
        }

        private void OnTick(object sender, object e)
        {
            light = hat.GetLightLevel().ToString("P2");

            txtLight.Text = "Luz: " + light;

            messageId++;

            SendDeviceToCloudMessagesAsync(light, messageId);
        }

        private static async void SendDeviceToCloudMessagesAsync(string lighSensor, int messageId)
        {
            var telemetryDataPoint = new
            {
                messageId = messageId++,
                deviceId = deviceId,
                light = lighSensor
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
        }

        private async void ReceiveMessage()
        {
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;
                

                HandleLeds(Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

        private void HandleLeds(string receivedMessage)
        {

            bool decision = Convert.ToBoolean(Convert.ToInt32(receivedMessage));

            if (decision)
            {
                hat.D2.Color = FEZHAT.Color.White;
                hat.D3.Color = FEZHAT.Color.White;
            }
            else
            {
                hat.D2.Color = FEZHAT.Color.Black;
                hat.D3.Color = FEZHAT.Color.Black;
            }
        }
    }
}
