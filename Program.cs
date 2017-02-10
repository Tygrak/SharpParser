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
            section[] sections = pag.findAllSectionsByContent("je");
            for (int i = 0; i < sections.Length; i++){
                Console.WriteLine(sections[i].content);
            }
            section ha = pag.findSectionByProperty("div", "class", "ha");
            Console.WriteLine(ha.content);
            Console.WriteLine(ha.tagPos);
            section pes = pag.findSectionParent(ha.tagPos);
            Console.WriteLine(pes.content);
            Console.WriteLine(pes.tagPos);
            Console.WriteLine(pag.findSectionChild(pes.tagPos).content);
            section teaspot = pag.findSectionByContent("ice cream");
            Console.WriteLine(teaspot.content);
            sections = pag.findAllSectionChildren(teaspot.tagPos);
            for (int i = 0; i < sections.Length; i++){
                Console.WriteLine(sections[i].content);
            }
            //Console.WriteLine(pag.content);
            pag = new page("http://diktator.wz.cz");
            //Console.WriteLine(pag.content);
            tag[] tags = pag.findAllTags("a");
            for (int i = 0; i < tags.Length; i++){
                Console.WriteLine(tags[i].getProperty("href"));
            }
            tags = pag.findAllTagsByProperty("a","id","odpoved0");
            for (int i = 0; i < tags.Length; i++){
                Console.WriteLine(tags[i].source);
            }
            /*section[] sections = pag.findAllSections("a");
            for (int i = 0; i < sections.Length; i++){
                //Console.WriteLine(sections[i].content);
                Console.WriteLine(sections[i].content);
            }
            Console.WriteLine(pag.findSection("style").source);
            Console.WriteLine(pag.findSectionByProperty("div", "class", "createGameText").source);*/
            //page utPage = new page("https://www.youtube.com/watch?v=L_jWHffIx5E");
            //Console.WriteLine("Smash Mouth - All Star: "+utPage.findSectionByContent("zhlédnutí").content);
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

        public tag findTag(string tagType, int offset = 0){
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType)){
                    posCurr = posTag+1;
                    continue;
                }
                if(offset == found){
                    return new tag(tagString, posStart);
                }
                found += 1;
            }
        }

        public tag[] findAllTags(string tagType, int numberToFind = -1){
            List<tag> tags = new List<tag>();
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType)){
                    posCurr = posTag+1;
                    continue;
                }
                tags.Add(new tag(tagString, posStart));
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType) || posStart == -1){
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType) || posStart == -1){
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                if(tagString.Contains("</"+tagType)){
                    if(depth == 0){
                        return new section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart);
                    } else{
                        depth -= 1;
                    }
                } else if(tagString.Contains(tagFinder)){
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
                    string tagString = html.Substring(posStart, posEnd-posStart+1);
                    if(tagString.Contains("</"+tagType)){
                        if(depth == 0){
                            sections.Add(new section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart));
                            break;
                        } else{
                            depth -= 1;
                        }
                    } else if(tagString.Contains(tagFinder)){
                        depth += 1;
                    } else{
                        posCurr = posTag+1;
                        continue;
                    }
                }
            }
            return sections.ToArray();
        }

        public tag findTagByProperty(string tagType, string property, string propertyValue){
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                posStart = tagString.IndexOf(property);
                if(posStart == -1){
                    continue;
                }
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType)){
                    posCurr = posTag+1;
                    continue;
                }
                posStart = tagString.IndexOf("\"",posStart);
                posEnd = tagString.IndexOf("\"",posStart+1);
                string content = tagString.Substring(posStart+1, posEnd-posStart-1); 
                if(propertyValue == content){
                    return new tag(tagString, posStart);
                }
            }
        }

        public tag[] findAllTagsByProperty(string tagType, string property, string propertyValue, int numberToFind = -1){
            List<tag> tags = new List<tag>();
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                posStart = tagString.IndexOf(property);
                if(posStart == -1){
                    continue;
                }
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType)){
                    posCurr = posTag+1;
                    continue;
                }
                posStart = tagString.IndexOf("\"",posStart);
                posEnd = tagString.IndexOf("\"",posStart+1);
                string content = tagString.Substring(posStart+1, posEnd-posStart-1); 
                if(propertyValue == content){
                    tags.Add(new tag(tagString, posStart));
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                posStart = tagString.IndexOf(property);
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType) || posStart == -1){
                    posCurr = posTag+1;
                    continue;
                }
                posStart = tagString.IndexOf("\"",posStart);
                posEnd = tagString.IndexOf("\"",posStart+1);
                string content = tagString.Substring(posStart+1, posEnd-posStart-1);
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                posStart = tagString.IndexOf(property);
                if(tagString.Contains("/"+tagType) || !tagString.Contains(tagType) || posStart == -1){
                    posCurr = posTag+1;
                    continue;
                }
                posStart = tagString.IndexOf("\"",posStart);
                posEnd = tagString.IndexOf("\"",posStart+1);
                string content = tagString.Substring(posStart+1, posEnd-posStart-1);
                if(propertyValue == content){
                    positions.Add(posFound);
                }
                posCurr = posTag+1;
            }
            return positions.ToArray();
        }
        
        public section findSectionParent(int position){
            position -= 1;
            int sectionStart, sectionEnd, posTest;
            while(true){
                sectionStart = html.LastIndexOf("<", position);
                posTest = html.IndexOf(">", sectionStart);
                if(sectionStart == -1 || posTest == -1){
                    return null;
                }
                if(posTest>position){
                    continue;
                }
                break;
            }
            string tagType = html.Substring(sectionStart,2);
            string tagEnd = tagType.Insert(1,"/");
            sectionEnd = html.IndexOf(tagEnd, sectionStart)-2;
            posTest = sectionEnd;
            while(true){
                sectionEnd = html.IndexOf(tagEnd, sectionEnd+1);
                posTest = html.LastIndexOf(tagType, posTest-1);
                if(sectionEnd == -1 || posTest == -1){
                    return null;
                }
                if(posTest>sectionStart){
                    continue;
                }
                break;
            }
            sectionEnd = html.IndexOf(">", sectionEnd);
            return new section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart);
        }

        public section findSectionChild(int position){
            int sectionStart, sectionEnd, posTest;
            while(true){
                sectionStart = html.IndexOf("<", position+1);
                posTest = html.IndexOf("<", sectionStart+1);
                int posTest2 = html.IndexOf(">", sectionStart);
                if(sectionStart == -1 || posTest2 == -1){
                    return null;
                }
                if(posTest<posTest2){
                    continue;
                }
                break;
            }
            string tagType = html.Substring(sectionStart,2);
            string tagEnd = tagType.Insert(1,"/");
            sectionEnd = html.IndexOf(tagEnd, sectionStart)-2;
            posTest = sectionEnd;
            while(true){
                sectionEnd = html.IndexOf(tagEnd, sectionEnd+1);
                posTest = html.LastIndexOf(tagType, posTest-1);
                if(sectionEnd == -1 || posTest == -1){
                    return null;
                }
                if(posTest>sectionStart){
                    continue;
                }
                break;
            }
            sectionEnd = html.IndexOf(">", sectionEnd);
            return new section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart);
        }

        public section[] findAllSectionChildren(int position){
            List<section> sections = new List<section>();
            int posCurr = position;
            while(true){
                int sectionStart, sectionEnd, posTest, posTest2;
                while(true){
                    sectionStart = html.IndexOf("<", posCurr+1);
                    posTest = html.IndexOf("<", sectionStart+1);
                    posTest2 = html.IndexOf(">", sectionStart);
                    if(sectionStart == -1 || posTest2 == -1){
                        break;
                    }
                    if(posTest<posTest2){
                        continue;
                    }
                    break;
                }
                if(sectionStart == -1 || posTest2 == -1){
                    break;
                }
                string tagType = html.Substring(sectionStart,2);
                if(tagType == "</"){
                    break;
                }
                string tagEnd = tagType.Insert(1,"/");
                sectionEnd = html.IndexOf(tagEnd, sectionStart)-2;
                posTest = sectionEnd;
                while(true){
                    sectionEnd = html.IndexOf(tagEnd, sectionEnd+1);
                    posTest = html.LastIndexOf(tagType, posTest-1);
                    if(sectionEnd == -1 || posTest == -1){
                        break;
                    }
                    if(posTest>sectionStart){
                        continue;
                    }
                    break;
                }
                if(sectionEnd == -1 || posTest == -1){
                    break;
                }
                sectionEnd = html.IndexOf(">", sectionEnd);
                sections.Add(new section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart));
                posCurr = sectionEnd;
            }
            return sections.ToArray();
        }

        public section findSectionByContent(string content){
            int posCont = html.IndexOf(content, 0);
            if(posCont == -1){
                return null;
            }
            int sectionStart, sectionEnd, posTest;
            while(true){
                sectionStart = html.LastIndexOf("<", posCont);
                posTest = html.IndexOf(">", sectionStart);
                if(sectionStart == -1 || posTest == -1){
                    return null;
                }
                if(posTest>posCont){
                    continue;
                }
                break;
            }
            string tagType = html.Substring(sectionStart,2);
            string tagEnd = tagType.Insert(1,"/");
            sectionEnd = html.IndexOf(tagEnd, sectionStart)-2;
            posTest = sectionEnd;
            while(true){
                sectionEnd = html.IndexOf(tagEnd, sectionEnd+1);
                posTest = html.LastIndexOf(tagType, posTest-1);
                if(sectionEnd == -1 || posTest == -1){
                    return null;
                }
                if(posTest>sectionStart){
                    continue;
                }
                break;
            }
            sectionEnd = html.IndexOf(">", sectionEnd);
            return new section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart);
        }

        public section[] findAllSectionsByContent(string content){
            List<section> sections = new List<section>();
            int posCurr = 0;
            while(true){
                int posCont = html.IndexOf(content, posCurr);
                if(posCont == -1){
                    break;
                }
                int sectionStart, sectionEnd, posTest;
                while(true){
                    sectionStart = html.LastIndexOf("<", posCont);
                    posTest = html.IndexOf(">", sectionStart);
                    if(sectionStart == -1 || posTest == -1){
                        break;
                    }
                    if(posTest>posCont){
                        continue;
                    }
                    break;
                }
                if(sectionStart == -1 || posTest == -1){
                    break;
                }
                string tagType = html.Substring(sectionStart,2);
                string tagEnd = tagType.Insert(1,"/");
                sectionEnd = html.IndexOf(tagEnd, sectionStart)-2;
                posTest = sectionEnd;
                while(true){
                    sectionEnd = html.IndexOf(tagEnd, sectionEnd+1);
                    posTest = html.LastIndexOf(tagType, posTest-1);
                    if(sectionEnd == -1 || posTest == -1){
                        break;
                    }
                    if(posTest>sectionStart){
                        continue;
                    }
                    break;
                }
                if(sectionEnd == -1 || posTest == -1){
                    break;
                }
                sectionEnd = html.IndexOf(">", sectionEnd);
                posCurr = sectionEnd;
                sections.Add(new section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart));
            }
            return sections.ToArray();
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
                string tagString = html.Substring(posStart, posEnd-posStart+1);
                if(tagString.Contains("</"+tagType)){
                    if(depth == 0){
                        return new section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart);
                    } else{
                        depth -= 1;
                    }
                } else if(tagString.Contains(tagFinder)){
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
                    string tagString = html.Substring(posStart, posEnd-posStart+1);
                    if(tagString.Contains("</"+tagType)){
                        if(depth == 0){
                            sections.Add(new section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart));
                            break;
                        } else{
                            depth -= 1;
                        }
                    } else if(tagString.Contains(tagFinder)){
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
                if(posEnd == -1){
                    return toClean;
                }
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
            property = property.Insert(0," ");
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

    public class tag{
        public string source;
        public int tagPos;

        public tag(string source, int tagPos = -1){
            this.source = source;
            this.tagPos = tagPos;
        }

        public string getProperty(string property){
            property = property.Insert(0," ");
            int posStart = source.IndexOf(property);
            if(posStart == -1){
                return null;
            }
            posStart = source.IndexOf("\"",posStart);
            int posEnd = source.IndexOf("\"",posStart+1);
            string content = source.Substring(posStart+1, posEnd-posStart-1); 
            return content;
        }
    }
}
