using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusQuery {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var result = Go("1", "2839861952").Result;
            result = Go("2", "3249678976").Result;
            Console.WriteLine("End!");

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
            if (!response.IsSuccessStatusCode) {
                result.Success = false;
                result.Message = "Error logging in: " + response.StatusCode;
                return result;
            }
            var content = await client.GetStringAsync("http://app.setuf.com.br/cardCidadao/showBalance");
            var price = Regex.Match(content, @"R\$ [0-9,]+").Value;
            var date = Regex.Match(content, @"[0-9]+\\/[0-9]+\\/[0-9]+").Value.Replace("\\/", "-");
            result.Success = true;
            result.Price = price;
            result.Date = date;
            return result; //$"{{balance: {price}, date: {date}}}";
        }

        public class Result {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Price { get; set; }
            public string Date { get; set; }
        }
    }
}