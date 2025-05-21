using Castle.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sayarah.Application.Helpers
{
    public class SMSHelper
    {
        public ILogger Logger { get; set; }
        public SMSHelper()
        {
            Logger = NullLogger.Instance;
        }

        public async Task<SendingResult> SendMessage(SendMessageInput input)
        {
            try
            {

               
                if (string.IsNullOrEmpty(input.PhoneNumbers))
                    return new SendingResult { Success = false, Message = "Phone is empty" };
                // using System.Net;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                input.PhoneNumbers = input.PhoneNumbers.Replace("-0", string.Empty);
                input.PhoneNumbers = input.PhoneNumbers.Replace("-", string.Empty);
                // Initialize HttpClient with base URL
                var httpClient = new HttpClient { BaseAddress = new Uri("https://www.msegat.com") };

                // Prepare the JSON payload
                var payload = new
                {
                    userName = input.UserName,
                    apiKey = input.ApiKey,
                    numbers = "966"+ input.PhoneNumbers,
                    userSender = input.Sender,
                    msg = input.MessageText,
                    msgEncoding = "UTF8"
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");



                // Send the POST request
                var response = await httpClient.PostAsync("/gw/sendsms.php", jsonContent);

                // Read response headers and data
                string responseHeaders = response.Headers.ToString();
                string responseData = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response
                    var jsonResponse = JObject.Parse(responseData);
                    var code = jsonResponse["code"]?.ToString();
                    var message = jsonResponse["message"]?.ToString();

                    // Interpret response content
                    switch (code)
                    {
                        case "1":
                        case "M0000":
                            return new SendingResult { Success = true, Message = "Message sent successfully" };

                        case "M0001":
                        case "1010":
                            return new SendingResult { Success = false, Message = "Variables missing" };

                        case "M0002":
                        case "1020":
                            return new SendingResult { Success = false, Message = "Invalid login info" };

                        case "1050":
                            return new SendingResult { Success = false, Message = "Message body is empty" };

                        case "1060":
                            return new SendingResult { Success = false, Message = "Balance is not enough" };

                        case "1064":
                            return new SendingResult { Success = false, Message = "Invalid message content for free OTP" };

                        case "1110":
                            return new SendingResult { Success = false, Message = "Sender name is missing or incorrect" };

                        case "1120":
                            return new SendingResult { Success = false, Message = "Mobile numbers are incorrect" };

                        case "1140":
                            return new SendingResult { Success = false, Message = "Message length is too long" };

                        case "M0008":
                            return new SendingResult { Success = false, Message = "Mobile number has an incorrect prefix" };

                        default:
                            Logger.Error($"Unhandled response code: {responseData.Trim()}");
                            return new SendingResult { Success = false, Message = $"Error: Unhandled response code {responseData.Trim()}" };
                    }
                }
                else
                {
                    Logger.Error($"Failed to send message. HTTP Status: {statusCode}, Error: {response.ReasonPhrase}");
                    return new SendingResult
                    {
                        Success = false,
                        Message = $"Failed to send message. HTTP Status: {statusCode}, Error: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new SendingResult { Success = false, Message = $"Exception: {ex.Message}" };
            }
        }

    }

    public class SendMessageBase
    {
        public string UserName { get; set; } = "syarah app";
        public string ApiKey { get; set; } = "451A52B6FADEBEDCD7C839E83DEC3569";
       // public string Sender { get; set; } = "syarahap-AD";
        public string Sender { get; set; } = "syarahap";
    }
    public class SendMessageInput : SendMessageBase
    {
        public string MessageText { set; get; }
        public string PhoneNumbers { set; get; }
    }
    public class SendingResult
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }




}
