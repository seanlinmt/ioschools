using System.Collections.Generic;

namespace ioschoolsWebsite.Library
{
    public class LoadedContent
    {
        public string content { get; set; }
        public List<string> filenames { get; set; }

        public LoadedContent()
        {
            filenames = new List<string>();
        }
    }
}
