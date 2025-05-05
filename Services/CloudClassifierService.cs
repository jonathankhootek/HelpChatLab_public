using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using ABI.System;

namespace HelpChat.Services
{
    public class CloudClassifierService : IClassifierService
    {
        string URL = $"https://{App.Current.Resources["MLEndpoint"]}/score";
        string KEY = App.Current.Resources["MLKey"].ToString();

        public async Task<string> GetClassifiedLabel(string input, string[] labels)
        {
            //TODO Insert 3.2 below (delete placeholder command)
            //return "";

            var data = new
            {
                text = input,
                labels = labels
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", KEY);

                try
                {
                    var response = await client.PostAsync(URL, content);
                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"The request failed with status code: {response.StatusCode}");
                        Console.WriteLine(result);
                    }
                    else
                    {
                        //Get the actual json string from the response
                        string innerJson = JsonConvert.DeserializeObject<string>(result);

                        Log.Information($"🏷️ Cloud classifier says {innerJson}");

                        //Deserialize the json
                        var deserialized = JsonConvert.DeserializeObject<List<List<object>>>(innerJson);

                        //Return the name of the most likely label
                        return deserialized.First()[0].ToString();
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                }


                return "";
            }

            //Insert above
        }
    }
}
