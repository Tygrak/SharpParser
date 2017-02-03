using System;
//using System.IO;
//using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApplication{
    public class Program{
        public static void Main(string[] args){
            SharpParser.page idnes = new SharpParser.page("http://gyrosmajales.wz.cz/test.php");
            Console.WriteLine(idnes.html);
        }
    }
}

namespace SharpParser{
    public class page{
        public string url;
        public string html;

        public page(string url){
            this.url = url;
            Task<string> loader = loadHTML(url);
            loader.Wait();
            this.html = loader.Result;
        }

        public async Task<string> loadHTML(string url){
            using(HttpClient client = new HttpClient())
            using(HttpResponseMessage response = await client.GetAsync(url))
            using(HttpContent content = response.Content){
                string html = await content.ReadAsStringAsync();
                return html;
            }
        }

        public string findById(string tag, string id){
            int pos = html.IndexOf(tag);
            return "";
        }
    }
}
