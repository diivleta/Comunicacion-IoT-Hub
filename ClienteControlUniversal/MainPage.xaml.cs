using ClienteControlUniversal.Helpers;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClienteControlUniversal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        static string eventHubEntity = "iothubpruebascloud";
        static string ConnectionString = $"Endpoint=sb://iothub-ns-iothubprue-345202-1a0d1c2a0b.servicebus.windows.net/;EntityPath={eventHubEntity};SharedAccessKeyName=iothubowner;SharedAccessKey=Uotv2Pzm/LE1VnC3xLQ+1oEwzWa757UdYadKsJnsJe0=";
        static string partitionId = "1";
        static EventHubClient eventHubClient;

        bool ledsPrendidos = false;

        public MainPage()
        {
            this.InitializeComponent();

            eventHubClient = EventHubClient.CreateFromConnectionString(ConnectionString);

            ReceiveMessagesFromDeviceAsync(partitionId);
        }

        private async void ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.CreateReceiver("sensores", partition, DateTime.UtcNow);
            while (true)
            {
                IEnumerable<EventData> messages = await eventHubReceiver.ReceiveAsync(10);
                if (messages == null) continue;
                foreach (var eventData in messages)
                {
                    var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    var sensorData = DeSerialize(data);

                    lblLight.Text = "Light level: " + sensorData["light"].ToString();

                    string[] luz = sensorData["light"].ToString().Replace(",", ".").Split(' ');

                    HandleLightStatus(luz[0], ref ledsPrendidos);
                }
            }
        }

        private JObject DeSerialize(string data)
        {
            return JObject.Parse(data);
        }

        private void HandleLightStatus(string light, ref bool handler)
        {

            double lightLevel = Convert.ToDouble(light);

            if (lightLevel < 70.00 && !handler)
            {
                FunctionHelper.SendDataToFunction("1");
                handler = !handler;
            }
            else if (lightLevel >= 70.00 && handler)
            {
                FunctionHelper.SendDataToFunction("0");
                handler = !handler;
            }
        }
    }
}
