using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharpParser{
    public class Page{
        public string url;
        public string html;
        public string content;

        public Page(string url){
            this.url = url;
            Task<string> loader = LoadHTML(url);
            loader.Wait();
            this.html = loader.Result;
            this.content = RemoveTags(loader.Result, true);
        }

        public Page(string url, string html){
            this.url = url;
            this.html = html;
            this.content = RemoveTags(html, true);
        }

        public async Task<string> LoadHTML(string url){
            using(HttpClient client = new HttpClient())
            using(HttpResponseMessage response = await client.GetAsync(url))
            using(HttpContent content = response.Content){
                string html = await content.ReadAsStringAsync();
                return html;
            }
        }

        public Tag FindTag(string tagType, int offset = 0){
            //Finds nth tag where n is offset
            int posCurr = 0;
            int found = 0;
            string tagFinder = GetTagFinder(tagType);
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
                    return new Tag(tagString, posStart);
                }
                found += 1;
            }
        }

        public Tag[] FindAllTags(string tagType, int numberToFind = -1){
            List<Tag> tags = new List<Tag>();
            int posCurr = 0;
            int found = 0;
            string tagFinder = GetTagFinder(tagType);
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
                tags.Add(new Tag(tagString, posStart));
                found += 1;
                if(numberToFind == found){
                    break;
                }
            }
            return tags.ToArray();
        }

        public int FindTagPosition(string tagType, int offset = 0){
            //Returns position of the "<" of the tag.
            string tagFinder = GetTagFinder(tagType);
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

        public int[] FindAllTagPositions(string tagType){
            List<int> positions = new List<int>();
            string tagFinder = GetTagFinder(tagType);
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

        public Section FindSection(string tagType, int offset = 0){
            int posCurr = FindTagPosition(tagType, offset);
            string tagFinder = GetTagFinder(tagType);
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
                        return new Section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart);
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

        public Section[] FindAllSections(string tagType){
            List<Section> sections = new List<Section>();
            string tagFinder = GetTagFinder(tagType);
            int[] positions = FindAllTagPositions(tagType);
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
                            sections.Add(new Section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart));
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

        public Tag FindTagByProperty(string tagType, string property, string propertyValue){
            int posCurr = 0;
            string tagFinder = GetTagFinder(tagType);
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
                    return new Tag(tagString, posStart);
                }
            }
        }

        public Tag[] FindAllTagsByProperty(string tagType, string property, string propertyValue, int numberToFind = -1){
            List<Tag> tags = new List<Tag>();
            int posCurr = 0;
            int found = 0;
            property = property.Insert(0," ");
            string tagFinder = GetTagFinder(tagType);
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
                    tags.Add(new Tag(tagString, posStart));
                    found += 1;
                }
                if(numberToFind == found){
                    break;
                }
            }
            return tags.ToArray();
        }

        public int FindTagPositionByProperty(string tagType, string property, string propertyValue){
            int posCurr = 0;
            string tagFinder = GetTagFinder(tagType);
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

        public int[] FindAllTagPositionsByProperty(string tagType, string property, string propertyValue){
            List<int> positions = new List<int>();
            string tagFinder = GetTagFinder(tagType);
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
        
        public Section FindSectionParent(int position){
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
            return new Section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart);
        }

        public Section FindSectionChild(int position){
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
            return new Section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart);
        }

        public Section[] FindAllSectionChildren(int position){
            List<Section> sections = new List<Section>();
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
                sections.Add(new Section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart));
                posCurr = sectionEnd;
            }
            return sections.ToArray();
        }

        public Section FindSectionByContent(string content){
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
            return new Section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart);
        }

        public Section[] FindAllSectionsByContent(string content){
            List<Section> sections = new List<Section>();
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
                sections.Add(new Section(html.Substring(sectionStart, sectionEnd-sectionStart+1), sectionStart));
            }
            return sections.ToArray();
        }

        public Section FindSectionByProperty(string tagType, string property, string propertyValue){
            int posCurr = FindTagPositionByProperty(tagType, property, propertyValue);
            string tagFinder = GetTagFinder(tagType);
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
                        return new Section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart);
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

        public Section[] FindAllSectionsByProperty(string tagType, string property, string propertyValue){
            List<Section> sections = new List<Section>();
            string tagFinder = GetTagFinder(tagType);
            int[] positions = FindAllTagPositionsByProperty(tagType, property, propertyValue);
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
                            sections.Add(new Section(html.Substring(sectionStart, posCurr-sectionStart), sectionStart));
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

        public static string GetTagFinder(string toFix){
            //toFix += " ";
            toFix = toFix.Insert(0,"<");
            return toFix;
        }

        public static string RemoveTags(string toClean, bool clearSpecial = false){
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

        public static string ReadablePage(string toClean){
            toClean = Page.RemoveTags(toClean, true); 
            string[] lines = toClean.Split(new string[]{"\n"}, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++){
                string line = lines[i];
                line = line.Trim();
                lines[i] = line;
            }
            return string.Join("\n", lines);
        }
    }
}