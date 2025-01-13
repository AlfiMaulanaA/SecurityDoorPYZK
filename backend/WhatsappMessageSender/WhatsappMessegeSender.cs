using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class WhatsappMessageSender
{
    public static async Task SendWhatsapp(string to, string text)
    {
        try
        {
            using (var client = new HttpClient())
            {
                // Set the request URL
                string requestUri = "https://graph.facebook.com/v15.0/106770632243346/messages";

                // Set the headers
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "EAAHS7iXyCXkBABeJBDzfVrIDzGeqy12ZBPGx8983DjZAAKndBA7TJ8GuSpVrUTVh3fvgFwgIdLF13hqScanrGuWgYFfm3JORkCL69ZCZCz2sB37L2WD877HZACUBNfPu02o87EEo0imwptVNc2MzONDqAbjKzZBhDLxYTuFL0XiNx3ixisZAQEr");

                // Create the request body
                var data = new
                {
                    messaging_product = "whatsapp",
                    to = to,
                    type = "template",
                    template = new
                    {
                        name = "initiate_gspe",
                        language = new
                        {
                            code = "en_US"
                        },
                        components = new[]
                        {
                                new
                                {
                                    type = "body",
                                    parameters = new[]
                                    {
                                        new { type = "text", text = text }
                                    }
                                }
                            }
                    }
                };

                string json = JsonSerializer.Serialize(data);
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(requestUri, content);

                    // Handle the response
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions here
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
