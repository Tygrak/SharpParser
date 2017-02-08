using System;
//using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SharpParser;

namespace ConsoleApplication{
    public class Program{
        public static void Main(string[] args){
            //Example
            page pag = new page("http://diktator.wz.cz/test.php");
            Console.WriteLine(pag.content);
            pag = new page("http://diktator.wz.cz");
            //Console.WriteLine(pag.content);
            string[] strings = pag.findAllTags("a");
            for (int i = 0; i < strings.Length; i++){
                Console.WriteLine(strings[i]);
            }
            strings = pag.findAllTagsByProperty("a","id","odpoved0");
            for (int i = 0; i < strings.Length; i++){
                Console.WriteLine(strings[i]);
            }
            /*section[] sections = pag.findAllSections("a");
            for (int i = 0; i < sections.Length; i++){
                //Console.WriteLine(sections[i].content);
                Console.WriteLine(sections[i].content);
            }
            Console.WriteLine(pag.findSection("style").source);
            Console.WriteLine(pag.findSectionByProperty("div", "class", "createGameText").source);*/
            page utPage = new page("https://www.youtube.com/watch?v=L_jWHffIx5E");
            //Console.WriteLine(utPage.content);
            section[] sections = utPage.findAllSectionsByProperty("div", "class", "watch-view-count");
            for (int i = 0; i < sections.Length; i++){
                Console.WriteLine("Smash Mouth - All Star: "+sections[i].content);
            }
        }
    }
}

namespace SharpParser{
    public class page{
        public string url;
        public string html;
        public string content;

        public page(string url){
            this.url = url;
            Task<string> loader = loadHTML(url);
            loader.Wait();
            this.html = loader.Result;
            this.content = removeTags(loader.Result, true);
        }

        public page(string url, string html){
            this.url = url;
            this.html = html;
            this.content = removeTags(html, true);
        }

        public async Task<string> loadHTML(string url){
            using(HttpClient client = new HttpClient())
            using(HttpResponseMessage response = await client.GetAsync(url))
            using(HttpContent content = response.Content){
                string html = await content.ReadAsStringAsync();
                return html;
            }
        }

        public string findTag(string tagType, int offset = 0){
            //Finds nth tag where n is offset
            int posCurr = 0;
            int found = 0;
            string tagFinder = getTagFinder(tagType);
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
                if(posTag == -1){
                    return null;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType)){
                    posCurr = posTag+1;
                    continue;
                }
                if(offset == found){
                    return tag;
                }
                found += 1;
            }
        }

        public string[] findAllTags(string tagType, int numberToFind = -1){
            List<string> tags = new List<string>();
            int posCurr = 0;
            int found = 0;
            string tagFinder = getTagFinder(tagType);
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
                if(posTag == -1){
                    break;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType)){
                    posCurr = posTag+1;
                    continue;
                }
                tags.Add(tag);
                found += 1;
                if(numberToFind == found){
                    break;
                }
            }
            return tags.ToArray();
        }

        public int findTagPosition(string tagType, int offset = 0){
            //Returns position of the "<" of the tag.
            string tagFinder = getTagFinder(tagType);
            int posCurr = 0;
            int found = 0;
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
                if(posTag == -1){
                    return -1;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType) || posStart == -1){
                    posCurr = posTag+1;
                    continue;
                }
                if(offset == found){
                    return posStart;
                }
                found += 1;
            }
        }

        public int[] findAllTagPositions(string tagType){
            List<int> positions = new List<int>();
            string tagFinder = getTagFinder(tagType);
            int posCurr = 0;
            int found = 0;
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
                if(posTag == -1){
                    break;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType) || posStart == -1){
                    posCurr = posTag+1;
                    continue;
                }
                positions.Add(posStart);
                found += 1;
            }
            return positions.ToArray();
        }

        public section findSection(string tagType, int offset = 0){
            int posCurr = findTagPosition(tagType, offset);
            string tagFinder = getTagFinder(tagType);
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
                if(posTag>posEnd){
                        posCurr = posTag+1;
                        continue;
                }
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                if(tag.Contains("</"+tagType)){
                    if(depth == 0){
                        return new section(html.Substring(sectionStart, posCurr-sectionStart), posStart);
                    } else{
                        depth -= 1;
                    }
                } else if(tag.Contains(tagFinder)){
                    depth += 1;
                } else{
                    posCurr = posTag+1;
                    continue;
                }
            }
        }

        public section[] findAllSections(string tagType){
            List<section> sections = new List<section>();
            string tagFinder = getTagFinder(tagType);
            int[] positions = findAllTagPositions(tagType);
            for (int i = 0; i < positions.Length; i++){
                int posCurr = positions[i];
                int sectionStart = posCurr;
                posCurr = html.IndexOf(">", posCurr);
                int depth = 0;
                while (true){
                    int posTag = html.IndexOf(tagType, posCurr);
                    if(posTag == -1){
                        break;
                    }
                    int posStart = html.LastIndexOf("<", posTag);
                    int posEnd = html.IndexOf(">", posStart);
                    if(posTag>posEnd){
                        posCurr = posTag+1;
                        continue;
                    }
                    posCurr = posEnd+1;
                    string tag = html.Substring(posStart, posEnd-posStart+1);
                    if(tag.Contains("</"+tagType)){
                        if(depth == 0){
                            sections.Add(new section(html.Substring(sectionStart, posCurr-sectionStart), posStart));
                            break;
                        } else{
                            depth -= 1;
                        }
                    } else if(tag.Contains(tagFinder)){
                        depth += 1;
                    } else{
                        posCurr = posTag+1;
                        continue;
                    }
                }
            }
            return sections.ToArray();
        }

        public string findTagByProperty(string tagType, string property, string propertyValue){
            int posCurr = 0;
            string tagFinder = getTagFinder(tagType);
            property = property.Insert(0," ");
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
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
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType)){
                    posCurr = posTag+1;
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

        public string[] findAllTagsByProperty(string tagType, string property, string propertyValue, int numberToFind = -1){
            List<string> tags = new List<string>();
            int posCurr = 0;
            int found = 0;
            property = property.Insert(0," ");
            string tagFinder = getTagFinder(tagType);
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
                if(posTag == -1){
                    break;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                posStart = tag.IndexOf(property);
                if(posStart == -1){
                    continue;
                }
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType)){
                    posCurr = posTag+1;
                    continue;
                }
                posStart = tag.IndexOf("\"",posStart);
                posEnd = tag.IndexOf("\"",posStart+1);
                string content = tag.Substring(posStart+1, posEnd-posStart-1); 
                if(propertyValue == content){
                    tags.Add(tag);
                    found += 1;
                }
                if(numberToFind == found){
                    break;
                }
            }
            return tags.ToArray();
        }

        public int findTagPositionByProperty(string tagType, string property, string propertyValue){
            int posCurr = 0;
            string tagFinder = getTagFinder(tagType);
            property = property.Insert(0," ");
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
                if(posTag == -1){
                    return -1;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posStart;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                posStart = tag.IndexOf(property);
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType) || posStart == -1){
                    posCurr = posTag+1;
                    continue;
                }
                posStart = tag.IndexOf("\"",posStart);
                posEnd = tag.IndexOf("\"",posStart+1);
                string content = tag.Substring(posStart+1, posEnd-posStart-1);
                if(propertyValue == content){
                    return posCurr;
                }
                posCurr = posTag+1;
            }
        }

        public int[] findAllTagPositionsByProperty(string tagType, string property, string propertyValue){
            List<int> positions = new List<int>();
            string tagFinder = getTagFinder(tagType);
            property = property.Insert(0," ");
            int posCurr = 0;
            int posFound = 0;
            while (true){
                int posTag = html.IndexOf(tagFinder, posCurr);
                if(posTag == -1){
                    break;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                posCurr = posEnd;
                posFound = posStart;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                posStart = tag.IndexOf(property);
                if(tag.Contains("/"+tagType) || !tag.Contains(tagType) || posStart == -1){
                    posCurr = posTag+1;
                    continue;
                }
                posStart = tag.IndexOf("\"",posStart);
                posEnd = tag.IndexOf("\"",posStart+1);
                string content = tag.Substring(posStart+1, posEnd-posStart-1);
                if(propertyValue == content){
                    positions.Add(posFound);
                }
                posCurr = posTag+1;
            }
            return positions.ToArray();
        }

        public section findSectionByProperty(string tagType, string property, string propertyValue){
            int posCurr = findTagPositionByProperty(tagType, property, propertyValue);
            string tagFinder = getTagFinder(tagType);
            int sectionStart = posCurr;
            property = property.Insert(0," ");
            posCurr = html.IndexOf(">", posCurr);
            int depth = 0;
            while (true){
                int posTag = html.IndexOf(tagType, posCurr);
                if(posTag == -1){
                    return null;
                }
                int posStart = html.LastIndexOf("<", posTag);
                int posEnd = html.IndexOf(">", posStart);
                if(posTag>posEnd){
                        posCurr = posTag+1;
                        continue;
                }
                posCurr = posEnd+1;
                string tag = html.Substring(posStart, posEnd-posStart+1);
                if(tag.Contains("</"+tagType)){
                    if(depth == 0){
                        return new section(html.Substring(sectionStart, posCurr-sectionStart), posStart);
                    } else{
                        depth -= 1;
                    }
                } else if(tag.Contains(tagFinder)){
                    depth += 1;
                } else{
                    posCurr = posTag+1;
                    continue;
                }
            }
        }

        public section[] findAllSectionsByProperty(string tagType, string property, string propertyValue){
            List<section> sections = new List<section>();
            string tagFinder = getTagFinder(tagType);
            int[] positions = findAllTagPositionsByProperty(tagType, property, propertyValue);
            property = property.Insert(0," ");
            for (int i = 0; i < positions.Length; i++){
                int posCurr = positions[i];
                int sectionStart = posCurr;
                posCurr = html.IndexOf(">", posCurr);
                int depth = 0;
                while (true){
                    int posTag = html.IndexOf(tagType, posCurr);
                    if(posTag == -1){
                        break;
                    }
                    int posStart = html.LastIndexOf("<", posTag);
                    int posEnd = html.IndexOf(">", posStart);
                    if(posTag>posEnd){
                        posCurr = posTag+1;
                        continue;
                    }
                    posCurr = posEnd+1;
                    string tag = html.Substring(posStart, posEnd-posStart+1);
                    if(tag.Contains("</"+tagType)){
                        if(depth == 0){
                            sections.Add(new section(html.Substring(sectionStart, posCurr-sectionStart), posStart));
                            break;
                        } else{
                            depth -= 1;
                        }
                    } else if(tag.Contains(tagFinder)){
                        depth += 1;
                    } else{
                        posCurr = posTag+1;
                        continue;
                    }
                }
            }
            return sections.ToArray();
        }

        public static string getTagFinder(string toFix){
            //toFix += " ";
            toFix = toFix.Insert(0,"<");
            return toFix;
        }

        public static string removeTags(string toClean, bool clearSpecial = false){
            int posCurr = 0;
            if(clearSpecial){
                while (true){
                    int posStart = toClean.IndexOf("<style");
                    if(posStart == -1){
                        break;
                    }
                    int posEnd = toClean.IndexOf("</style", posStart);
                    posEnd = toClean.IndexOf(">", posEnd);
                    if(posEnd == -1){
                        break;
                    }
                    toClean = toClean.Remove(posStart, posEnd-posStart+1);
                }
                while (true){
                    int posStart = toClean.IndexOf("<script");
                    if(posStart == -1){
                        break;
                    }
                    int posEnd = toClean.IndexOf("</script", posStart);
                    posEnd = toClean.IndexOf(">", posEnd);
                    if(posEnd == -1){
                        break;
                    }
                    toClean = toClean.Remove(posStart, posEnd-posStart+1);
                }
            }
            posCurr = 0;
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
        public int tagPos;

        public section(string source, int tagPos = -1){
            this.source = source;
            this.content = page.removeTags(source);
            this.tagPos = tagPos;
        }

        public string getProperty(string property){
            int posStart = source.IndexOf(property);
            int posEnd = source.IndexOf(">");
            if(posStart == -1 || posEnd<posStart){
                return null;
            }
            posStart = source.IndexOf("\"",posStart);
            posEnd = source.IndexOf("\"",posStart+1);
            string content = source.Substring(posStart+1, posEnd-posStart-1); 
            return content;
        }
    }
}
