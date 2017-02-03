using System;
//using System.IO;
//using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SharpParser;

namespace ConsoleApplication{
    public class Program{
        public static void Main(string[] args){
            page idnes = new SharpParser.page("http://gyrosmajales.wz.cz/test.php");
            Console.WriteLine(idnes.findSectionByProperty("div", "id", "pes").content);
            Console.WriteLine(idnes.findSectionByProperty("span", "class", "kocur").content);
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

        public page(string url, string html){
            this.url = url;
            this.html = html;
        }

        public async Task<string> loadHTML(string url){
            using(HttpClient client = new HttpClient())
            using(HttpResponseMessage response = await client.GetAsync(url))
            using(HttpContent content = response.Content){
                string html = await content.ReadAsStringAsync();
                return html;
            }
        }

        public string findTagByProperty(string tagType, string property, string propertyValue){
            int posCurr = 0;
            while (true){
                int posTag = html.IndexOf(tagType, posCurr);
                if(posTag == -1){
                    return null;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                posStart = tag.IndexOf(property);
                if(posStart == -1){
                    continue;
                }
                posStart = tag.IndexOf("\"",posStart);
                posEnd = tag.IndexOf("\"",posStart+1);
                string content = tag.Substring(posStart+1, posEnd-posStart-1); 
                if(propertyValue == content){
                    return tag;
                }
            }
        }

        public int findTagPositionByProperty(string tagType, string property, string propertyValue){
            //Returns position of the "<" of the tag.
            int posCurr = 0;
            while (true){
                int posTag = html.IndexOf(tagType, posCurr);
                if(posTag == -1){
                    return -1;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posStart;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                posStart = tag.IndexOf(property);
                if(posStart == -1){
                    continue;
                }
                posStart = tag.IndexOf("\"",posStart);
                posEnd = tag.IndexOf("\"",posStart+1);
                string content = tag.Substring(posStart+1, posEnd-posStart-1);
                if(propertyValue == content){
                    return posCurr;
                }
            }
        }

        public section findSectionByProperty(string tagType, string property, string propertyValue){
            int posCurr = findTagPositionByProperty(tagType, property, propertyValue);
            int sectionStart = posCurr;
            posCurr = html.IndexOf(">", posCurr);
            int depth = 0;
            while (true){
                int posTag = html.IndexOf(tagType, posCurr);
                if(posTag == -1){
                    return null;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                if(tag.Contains("/"+tagType)){
                    if(depth == 0){
                        return new section(html.Substring(sectionStart, posCurr-sectionStart));
                    } else{
                        depth -= 1;
                    }
                } else{
                    depth += 1;
                }
            }
        }

        public static string removeTags(string toClean){
            int posCurr = 0;
            while (true){
                int posStart = toClean.IndexOf("<", posCurr);
                if(posStart == -1){
                    return toClean;
                }
                int posTest = toClean.IndexOf("<", posStart+1);
                int posEnd = toClean.IndexOf(">", posStart);
                if(posTest != -1 && posTest < posEnd){
                    posCurr = posTest;
                    continue;
                }
                toClean = toClean.Remove(posStart, posEnd-posStart+1);
            }
        }
    }

    public class section{
        public string source;
        public string content;
        public section(string source){
            this.source = source;
            this.content = page.removeTags(source);
        }
    }
}
