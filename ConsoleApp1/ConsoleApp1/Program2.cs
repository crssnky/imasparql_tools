using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            using (StreamReader file = File.OpenText(@"..\ii.json")) {
                JsonSerializer serializer = new JsonSerializer();
                Json json = (Json)serializer.Deserialize(file, typeof(Json));

                Console.WriteLine("ii.json:" + json.nodes.Count);

                // Output Files
                var outFile = new StreamWriter(@"../intro.rdf", false, Encoding.UTF8);
                var xmlns = new StreamReader(@"../xmlns.txt", Encoding.UTF8);
                outFile.WriteLine(xmlns.ReadToEnd());
                xmlns.Close();

                using (WebClient webclient = new WebClient()) {
                    webclient.Encoding = Encoding.UTF8;
                    foreach (var i in json.nodes) {
                        Console.WriteLine(i.name);

                        string uri = sparqling(webclient, i.name);
                        if (uri == "") continue;

                        // 2017
                        outFile.WriteLine("<rdf:Description rdf:about=\"detail/" + splitName(uri) + "_IdolIntroduction2017\">");
                        outFile.WriteLine("<imas:Source rdf:resource=\"" + uri + "\"/>");
                        foreach (var j in i.data.Y2017.targets) {
                            var destination = json.nodes.Find(n => n.id == j);
                            outFile.WriteLine("<imas:Destination rdf:resource=\"" + sparqling(webclient, destination.name) + "\"/>");
                        }
                        outFile.WriteLine("<schema:name xml:lang=\"ja\">" + i.data.Y2017.title.Replace("…", "...").Replace("&", "&amp;") + "</schema:name>");
                        outFile.WriteLine("<schema:description xml:lang=\"ja\">" + i.data.Y2017.comment.Replace("…","...").Replace("&", "&amp;") + "</schema:description>");
                        outFile.WriteLine("<schema:releaseDate rdf:datatype=\"http://www.w3.org/2001/XMLSchema#date\">2017-04-24</schema:releaseDate>");
                        outFile.WriteLine("<rdf:type rdf:resource=\"https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#Introduction\"/>");
                        outFile.WriteLine("</rdf:Description>");

                        // 2018
                        outFile.WriteLine("<rdf:Description rdf:about=\"detail/" + splitName(uri) + "_IdolIntroduction2018\">");
                        outFile.WriteLine("<imas:Source rdf:resource=\"" + uri + "\"/>");
                        foreach (var j in i.data.Y2018.targets) {
                            var destination = json.nodes.Find(n => n.id == j);
                            outFile.WriteLine("<imas:Destination rdf:resource=\"" + sparqling(webclient, destination.name) + "\"/>");
                        }
                        outFile.WriteLine("<schema:name xml:lang=\"ja\">" + i.data.Y2018.title.Replace("…", "...").Replace("&", "&amp;") + "</schema:name>");
                        outFile.WriteLine("<schema:description xml:lang=\"ja\">" + i.data.Y2018.comment.Replace("…", "...").Replace("&", "&amp;") + "</schema:description>");
                        outFile.WriteLine("<schema:releaseDate rdf:datatype=\"http://www.w3.org/2001/XMLSchema#date\">2018-04-23</schema:releaseDate>");
                        outFile.WriteLine("<rdf:type rdf:resource=\"https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#Introduction\"/>");
                        outFile.WriteLine("</rdf:Description>");
                    }
                }
                outFile.WriteLine("</rdf:RDF>");
                outFile.Close();
            }
        }
        static string sparqling(WebClient web, string name) {
            var download = web.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3e%0d%0aSELECT%20%3fs%0d%0aWHERE%20%7b%0d%0a%20%20%3fs%20schema%3aname%20%22" + name + "%22%40ja%0d%0a%7d");
            var xml = new XmlDocument();
            xml.LoadXml(download);

            return xml.ChildNodes[1].ChildNodes[1].InnerText;
        }
        static string splitName(string uri) {
            var arry = uri.Split('/');
            return arry[arry.Length - 1].Replace("_", "");
        }
    }
    public class y2017 {
        public string title { get; set; }
        public string comment { get; set; }
        public List<string> targets { get; set; }
        public y2017() {
            targets = new List<string>();
        }
    }
    public class y2018 {
        public string title { get; set; }
        public string comment { get; set; }
        public List<string> targets { get; set; }
        public y2018() {
            targets = new List<string>();
        }
    }
    public class datas {
        public y2017 Y2017 { get; set; }
        public y2018 Y2018 { get; set; }
    }
    public class node {
        public string id { get; set; }
        public string name { get; set; }
        public string kana { get; set; }
        public string type { get; set; }
        public datas data { get; set; }
    }
    public class Json {
        public List<node> nodes { get; set; }
        public Json() {
            nodes = new List<node>();
        }
    }
}