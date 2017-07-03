using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BusQueryFunction {
    public static class GetBalance {
        [FunctionName("GetBalance")]

        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "balance")]HttpRequestMessage req, TraceWriter log) {

            // parse query parameter
            string type = GetQueryParam("type", req);
            string card = GetQueryParam("card", req);

            if (type == null || card == null) {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a type and a card on the query string");
            }
            var result = await Go(type, card);
            log.Info($"HTTP trigger function processed a request for: type: {type} and card: {card} = [{result.Price} {result.Date}]");
            return req.CreateResponse(HttpStatusCode.OK, result);
        }

        private static async Task<Result> Go(string typeSys, string idCard) {
            var formData = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("typeSys", typeSys),
                new KeyValuePair<string, string>("typeCard", "18"),
                new KeyValuePair<string, string>("idCard", idCard)
            };
            var result = new Result();

            var client = new HttpClient();
            var response = await client.PostAsync("http://app.setuf.com.br/index/requestAuthCid", new FormUrlEncodedContent(formData));
            var content = await client.GetStringAsync("http://app.setuf.com.br/cardCidadao/showBalance");
            var price = Regex.Match(content, @"R\$ [0-9,]+").Value;
            var date = Regex.Match(content, @"[0-9]+\\/[0-9]+\\/[0-9]+").Value.Replace("\\/", "-");
            result.Price = price;
            result.Date = date;
            return result; //$"{{balance: {price}, date: {date}}}";
        }

        private static string GetQueryParam(string name, HttpRequestMessage req) {
            return req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, name, true) == 0)
                .Value;
        }

        public class Result {
            public string Price { get; set; }
            public string Date { get; set; }
        }
    }
}