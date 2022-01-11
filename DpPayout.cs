using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace PriceCheckWebAPI
{
    public static class DpPayout
    {
        [FunctionName("DpPayout")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("DPPayout log begin");

            string sid = req.Form["sid"];
            string card_type = req.Form["card_type"];
            string tx_action = req.Form["tx_action"];
            string firstname = req.Form["firstname"];
            string lastname = req.Form["lastname"];
            string city = req.Form["city"];
            string email = req.Form["email"];
            string amount = req.Form["amount"];
            string currency = req.Form["currency"];
            string bank_code = req.Form["bank_code"];
            string account_name = req.Form["account_name"];
            string account_number = req.Form["account_number"];
            string bank_province = req.Form["bank_province"];
            string bank_city = req.Form["bank_city"];
            string postback_url = req.Form["postback_url"];
            var client = new RestClient(GetEnvironmentVariable("BitakeApiKey").ToString());
            //var client = new RestClient("https://qa.secure.awepay.com/txHandler.php");
            var request = new RestRequest("default", Method.Post);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "sid="+sid+"&firstname="+firstname+"&lastname="+
                lastname+"&payby="+card_type+"&tx_action="+ tx_action + "&currency="+
                currency+"&no_ship_address=1&hide_descriptor=1&postbackurl="+postback_url+"&city="+city+ "&email="+
                email + "&amount="+amount+"&bank_code="+bank_code+ "&account_name="+ account_name + "&account_number="+
                account_number + "&bank_province="+ bank_province + "&bank_city="+ bank_city, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            
            return new OkObjectResult(response);
        }

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
