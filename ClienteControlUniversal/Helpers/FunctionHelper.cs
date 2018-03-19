using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClienteControlUniversal.Helpers
{
    public class FunctionHelper
    {

        static string iotHub = "iothubpruebascloud";
        static string deviceId = "devicetest";
        static string api = "2016-02-03";

        public static async void SendDataToFunction(string handler)
        {
            HttpClient request = new HttpClient();
            var requestedLink = new Uri("https://functionsiotpruebas.azurewebsites.net/api/HttpTriggerCSharp1?code=VPLWYMqTnPMVyPkDYwCKjXY2BWaKEBhjBt0vZq/LdMuCYiiLKAk5Dw==");
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, requestedLink);

            var sendString = String.Format("{{\"DeviceId\":\"devicetest\",\"Message\":\"{0}\"}}", handler);

            requestMessage.Content = new StringContent(sendString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await request.SendAsync(requestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
        }
    }
}
