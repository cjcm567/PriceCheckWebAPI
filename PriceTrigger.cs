using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PriceCheckWebAPI
{
    public static class PriceTrigger
    {
        [FunctionName("PriceTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string verifyCode = data.verifyCode;
            dynamic result = GetNewJson(GetValueFromMT.GetValue());
            return AuthorizeKey(verifyCode) != false
                ? (ActionResult)new OkObjectResult(result)
                : new UnauthorizedResult();
        }
        public static dynamic GetNewJson(string result)
        {
            var newJson = new JObject();
            JObject jobjResult = JObject.Parse(result);
            string jstring = "";
            newJson.Add("EURUSD", jobjResult["EURUSD"]);
            newJson.Add("USDJPY", jobjResult["USDJPY"]);
            newJson.Add("GBPUSD", jobjResult["GBPUSD"]);
            newJson.Add("EURJPY", jobjResult["EURJPY"]);
            newJson.Add("AUDUSD", jobjResult["AUDUSD"]);
            newJson.Add("SPX500", jobjResult["SPX500"]);
            newJson.Add("US30", jobjResult["US30"]);
            newJson.Add("HKG33", jobjResult["HKG33"]);
            newJson.Add("NAS100", jobjResult["NAS100"]);
            newJson.Add("A50", jobjResult["A50"]);
            newJson.Add("XAUUSD", jobjResult["XAUUSD"]);
            newJson.Add("XAGUSD", jobjResult["XAGUSD"]);
            newJson.Add("USOil", jobjResult["USOil"]);
            newJson.Add("TSLA", jobjResult["TSLA"]);
            newJson.Add("AMZN", jobjResult["AMZN"]);
            newJson.Add("AAPL", jobjResult["AAPL"]);
            newJson.Add("BA", jobjResult["BA"]);
            newJson.Add("MSFT", jobjResult["MSFT"]);
            jstring = newJson.ToString();
            jstring = jstring.Replace("bid","Bid");
            jstring = jstring.Replace("ask","Ask");
            jstring = jstring.Replace("tickTime","Last");
            newJson = JObject.Parse(jstring);
            return newJson;
        }
        /// <summary>
        /// This method is running for get the SHA256
        /// </summary>
        /// <param name="message">Encoding message</param>
        /// <param name="key">Encoding secure</param>
        /// <returns>sha256</returns>
        static string ComputeHMACSHA256(string message, string key)
        {
            string sha256 = null;
            if (!String.IsNullOrEmpty(message) && !String.IsNullOrEmpty(key))
            {
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                byte[] keyByte = encoding.GetBytes(key);
                HMACSHA256 hmacsha256 = new HMACSHA256(keyByte);
                byte[] messageBytes = encoding.GetBytes(message);
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                sha256 = ByteToString(hashmessage);
                return sha256;
            }
            return sha256 = "";
                      
            
        }
        public static bool AuthorizeKey(string verifyCode)
        {
            string key = GetEnvironmentVariable("Key");
            string message = GetEnvironmentVariable("Message");
            bool isAuthorized = false;
            return verifyCode.ToUpper() == ComputeHMACSHA256(message, key).ToUpper() ? isAuthorized = true : isAuthorized;
        }
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }
    }
    public static class GetValueFromMT
    {
        public static dynamic GetValue()
        {
            string content = "";
            dynamic json = "";
            content = getCjcApiValueAsync().ToString();
            if (String.IsNullOrEmpty(content))
            {
                content = GetWebContext();
                json = GetJson(content);
            } else
            {
                json = content;
            }            
            return json;
        }
        public static dynamic getCjcApiValueAsync()
        {
            string cjcApi = PriceTrigger.GetEnvironmentVariable("CJCAPI");
            var result = "";
            // Create a request for the URL.   
            WebRequest request = WebRequest.Create(cjcApi);
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server. 
            // The using block ensures the stream is automatically closed. 
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                if (responseFromServer != "")
                {
                    result = GetJson(responseFromServer);
                }
            }
            // Close the response.  
            response.Close();



            return result;
        }
        /// <summary>
        /// This function is running for getting the price from Fxcm.
        /// </summary>
        /// <returns>string XML</returns>
        public static string GetWebContext()
        {
            string pageContext = "";
            try
            {
                WebClient MyWebClient = new WebClient();
                Byte[] pageData = null;
                
                string fxApi = PriceTrigger.GetEnvironmentVariable("FXAPI");
                pageData = MyWebClient.DownloadData(fxApi);
                pageContext = Encoding.Default.GetString(pageData);
                pageContext = ConvertJson(pageContext);
            }
            catch (Exception ex)
            {
                pageContext = ex.Message.ToString();
            }
            return pageContext;
        }
        public static string ConvertJson(string webContext)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(webContext);
            var jObject = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
            return jObject;
        }
        /// <summary>
        /// This function is running for convert the XML to JSON
        /// </summary>
        /// <param name="webContext"></param>
        /// <returns>string JSON</returns>
        public static string GetJson(string webContext)
        {
            JObject jobj = JObject.Parse(webContext);
            string json = Fun(jobj);
            if (!String.IsNullOrEmpty(json)) json = "{" + json + "}";
            return json;
        }
        /// <summary>
        /// This method is running for updating the JSON format through track the JSON string
        /// </summary>
        /// <param name="obj">JSON object</param>
        /// <returns>JSON as we expected</returns>
        public static string Fun(JObject obj)
        {
            string result = null;

            foreach (var item in obj)
            {
                if (typeof(JObject) == item.Value.GetType())
                {
                    JObject child = (JObject)item.Value;
                    string tmp = Fun(child);
                    result += tmp;
                    string target = "@version=1.0,@encoding=UTF-8,";
                    if (result == target)
                    {
                        result = "";
                    }
                }
                else if (typeof(JArray) == item.Value.GetType())
                {
                    JArray _jarray = (JArray)item.Value;
                    foreach (var jitem in _jarray)
                    {
                        JObject jchild = (JObject)jitem;
                        string value = jchild.First.ToString();
                        value = value.Substring(9);
                        string tmp = value + ":" + jchild + ",";                        
                        result += tmp;

                    }
                    result = result.Substring(0, result.Length - 1);
                }
            }

            return result;
        }
    }
}