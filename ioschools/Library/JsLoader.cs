using System;
using System.IO;
using System.Text;
using System.Xml;

namespace ioschools.Library
{
    public class JsLoader
    {
        private readonly string JAVASCRIPT_BASE_DIR = AppDomain.CurrentDomain.BaseDirectory + "/Scripts/";
        
        public static readonly JsLoader Instance = new JsLoader();

        public LoadedContent LoadFeatures(string featurePath, string xmlFile)
        {
            var jscontent = new LoadedContent();
            string xmlcontent = System.IO.File.ReadAllText(string.Concat(JAVASCRIPT_BASE_DIR, featurePath, "/", xmlFile));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlcontent);
            XmlNodeList files = doc.GetElementsByTagName("files");
            StringBuilder sb = new StringBuilder();
            foreach (XmlElement file in files)
            {
                XmlNodeList libraries = file.GetElementsByTagName("script");
                foreach (XmlElement script in libraries)
                {
                    String source = script.Attributes["src"].Value;
                    string filename = string.Concat(JAVASCRIPT_BASE_DIR, featurePath, "/", source);
                    jscontent.filenames.Add(filename);
                    string content = System.IO.File.ReadAllText(filename);
                    sb.Append(content);
                }
            }
#if DEBUG
            jscontent.content = sb.ToString();
#else
            // can't use closure at the moment because it messes up jqgrid
            jscontent.content = MyMin.parse(sb.ToString(), false, false);
#endif
            return jscontent;
        }

        public string LoadViewJavascript(string path)
        {
            var jslocation = string.Concat(JAVASCRIPT_BASE_DIR, path, ".js");
            string jscontent = System.IO.File.ReadAllText(jslocation);
#if DEBUG
            return jscontent;
#else
            //string minified = compress.getJSMachine(jscontent);
            string minified = MyMin.parse(jscontent, false, false);
            return minified;
#endif
        }
    }
}