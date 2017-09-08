using System;
using SharpParser;

namespace ConsoleApplication{
    public class Program{
        public static void Main(string[] args){
            //Example
            Page pag = new Page("https://www.reddit.com/");
            Section sec = pag.FindSectionByProperty("div", "id", "siteTable");
            Console.WriteLine(Page.ReadablePage(sec.content));
            /*section[] sections = pag.findAllSectionsByContent("je");
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
            section tea = sections[1];
            tea = pag.findSectionParent(tea.tagPos);*/ //TODO: Fix findSectionParent
            /*
            pag = new page("http://diktator.wz.cz");
            //Console.WriteLine(pag.content);
            tag[] tags = pag.findAllTags("a");
            for (int i = 0; i < tags.Length; i++){
                Console.WriteLine(tags[i].getProperty("href"));
            }
            tags = pag.findAllTagsByProperty("a","id","odpoved0");
            for (int i = 0; i < tags.Length; i++){
                Console.WriteLine(tags[i].source);
            }*/
            /*section[] sections = pag.findAllSections("a");
            for (int i = 0; i < sections.Length; i++){
                //Console.WriteLine(sections[i].content);
                Console.WriteLine(sections[i].content);
            }
            Console.WriteLine(pag.findSection("style").source);
            Console.WriteLine(pag.findSectionByProperty("div", "class", "createGameText").source);*/
            //page utPage = new page("https://www.youtube.com/watch?v=L_jWHffIx5E");
            //Console.WriteLine(page.readablePage(utPage.content));
            //Console.WriteLine("Smash Mouth - All Star: "+utPage.findSectionByContent("zhlédnutí").content);
        }
    }
}
