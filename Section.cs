namespace SharpParser{
    public class Section{
        public string source;
        public string content;
        public int tagPos;

        public Section(string source, int tagPos = -1){
            this.source = source;
            this.content = Page.RemoveTags(source);
            this.tagPos = tagPos;
        }

        public string GetProperty(string property){
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

    public class Tag{
        public string source;
        public int tagPos;

        public Tag(string source, int tagPos = -1){
            this.source = source;
            this.tagPos = tagPos;
        }

        public string GetProperty(string property){
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