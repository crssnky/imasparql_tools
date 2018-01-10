using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using YamlDotNet.Serialization;

namespace yamlRead {
    class Program {
        static void Main(string[] args) {
            var file = new StreamReader(@"D:\GitJON\utada_data\data\eigyo.yml", Encoding.UTF8);
            var deserializer = new DeserializerBuilder().Build();
            var root = deserializer.Deserialize<ComuRoot>(file);

            const string NODE_TITLE = "MillionLiveEigyo";
            var comu = new StreamWriter(@"MillionLiveEigyo.rdf", false, Encoding.UTF8);
            var texts = new List<string>();
            var names = new Dictionary<string, string>();
            var xmlns = new StreamReader(@"xmlns.txt", Encoding.UTF8);
            comu.WriteLine(xmlns.ReadToEnd());
            xmlns.Close();

            using (WebClient webclient = new WebClient()) {
                for (int j = 0; j < root.comu.Count; j++) {
                    var cd = root.comu[j];
                    texts.Clear();
                    comu.WriteLine("<rdf:Description rdf:about=\"detail/MillionLive" + URIencoding("営業:" + cd.title) + "\">");
                    comu.WriteLine("<schema:title xml:lang=\"ja\">" + "MillionLive営業:" + cd.title + "</schema:title>");
                    comu.WriteLine("<schema:location xml:lang=\"ja\">" + cd.title + "</schema:location>");
                    Console.WriteLine(cd.title);
                    var participant = new List<string>();
                    for (int i = 0; i < cd.dialog.Count; i++) {
                        var element = cd.dialog[i];
                        if (!participant.Contains(element.name)) {
                            participant.Add(element.name);
                            if (element.name != "") comu.WriteLine(sparqling(webclient, element.name, 1));
                        }

                        string nodeName = NODE_TITLE + (j < 10 ? "0" + j.ToString() : j.ToString()) + "_" + (i < 10 ? "0" + i.ToString() : i.ToString());
                        comu.WriteLine("<imas:Script rdf:nodeID=\"" + nodeName + "\"/>");
                        texts.Add("<rdf:Description rdf:nodeID=\"" + nodeName + "\">");
                        texts.Add(sparqling(webclient, element.name, 0));
                        texts.Add("<schema:text xml:lang=\"ja\">" + element.text + "</schema:text>");
                        texts.Add("<imas:ScriptNumber rdf:datatype=\"http://www.w3.org/2001/XMLSchema#integer\">" + i + "</imas:ScriptNumber>");
                        texts.Add("<rdf:type rdf:resource=\"https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#ScriptText\"/>");
                        texts.Add("</rdf:Description>");
                    }
                    comu.WriteLine("<rdf:type rdf:resource=\"https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#Communication\"/>");
                    comu.WriteLine("</rdf:Description>");
                    foreach (var i in texts) {
                        comu.WriteLine(i);
                    }
                    comu.WriteLine();
                }
            }

            comu.WriteLine("</rdf:RDF>");
            comu.Close();
        }

        static string URIencoding(string str) {
            return Uri.EscapeDataString(str).Replace("-", "%2d").Replace(".", "%2e").Replace("~", "%7e").ToLower();
        }
        static string sparqling(WebClient web, string name, int version) {
            string ret = "";
            if (name.IndexOf("高木社長") > -1) {
                name = "高木順二朗";
            }
            if (name.IndexOf("&") > -1) {
                var names = name.Split('&');
                foreach (var i in names) {
                    var download = web.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3e%0d%0aPREFIX%20rdf%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f1999%2f02%2f22%2drdf%2dsyntax%2dns%23%3e%0d%0aPREFIX%20rdfs%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f2000%2f01%2frdf%2dschema%23%3e%0d%0aPREFIX%20imas%3a%20%3chttps%3a%2f%2fsparql%2ecrssnky%2exyz%2fimasrdf%2fURIs%2fimas%2dschema%2ettl%23%3e%0d%0a%0d%0aSELECT%20distinct%20%3fs%20%0d%0aWHERE%20%7b%0d%0a%20%20%3fs%20schema%3aname%7cschema%3aalternateName%20%3fo%3b%0d%0a%20%20%20%20%20%20rdf%3atype%2frdfs%3asubClassOf%2a%20schema%3aPerson%3b%0d%0a%20%20%20%20%20%20schema%3amemberOf%20%3fm%3b%0d%0a%20%20%20%20%20filter%28regex%28str%28%3fo%29%2c%22" + URIencoding(i) + "%22%29%26%26contains%28str%28%3fm%29%2c%22Mil%22%29%29%2e%0d%0a%7d");
                    var xml = new XmlDocument();
                    xml.LoadXml(download);

                    switch (version) {
                        case 0:
                            if (xml.ChildNodes[1].ChildNodes[1].InnerText != "") {
                                if (ret.Length > 0) ret += "\n";
                                ret += "<imas:Source rdf:resource=\"" + xml.ChildNodes[1].ChildNodes[1].InnerText + "\"/>";
                            } else {
                                ret = "<imas:SpeakerLabel xml:lang=\"ja\">" + i + "</imas:SpeakerLabel>";
                            }
                            break;
                        case 1:
                            if (xml.ChildNodes[1].ChildNodes[1].InnerText != "") {
                                if (ret.Length > 0) ret += "\n";
                                ret += "<schema:participant rdf:resource=\"" + xml.ChildNodes[1].ChildNodes[1].InnerText + "\"/>";
                            } else {
                                ret += "<schema:participant xml:lang=\"ja\">" + i + "</schema:participant>";
                            }
                            break;
                    }
                }
                if (version == 0) ret += "\n<imas:SpeakerLabel xml:lang=\"ja\">" + name.Replace("&", "＆") + "</imas:SpeakerLabel>";
            } else {
                if (name == "") name = "null";
                var download = web.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3e%0d%0aPREFIX%20rdf%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f1999%2f02%2f22%2drdf%2dsyntax%2dns%23%3e%0d%0aPREFIX%20rdfs%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f2000%2f01%2frdf%2dschema%23%3e%0d%0aPREFIX%20imas%3a%20%3chttps%3a%2f%2fsparql%2ecrssnky%2exyz%2fimasrdf%2fURIs%2fimas%2dschema%2ettl%23%3e%0d%0a%0d%0aSELECT%20distinct%20%3fs%20%0d%0aWHERE%20%7b%0d%0a%20%20%3fs%20schema%3aname%7cschema%3aalternateName%20%3fo%3b%0d%0a%20%20%20%20%20%20rdf%3atype%2frdfs%3asubClassOf%2a%20schema%3aPerson%3b%0d%0a%20%20%20%20%20filter%28regex%28str%28%3fo%29%2c%22" + URIencoding(name) + "%22%29%29%2e%0d%0a%7d");
                var xml = new XmlDocument();
                xml.LoadXml(download);

                if (name == "null") name = "";
                switch (version) {
                    case 0:
                        if (xml.ChildNodes[1].ChildNodes[1].InnerText != "") {
                            ret = "<imas:Source rdf:resource=\"" + xml.ChildNodes[1].ChildNodes[1].InnerText + "\"/>";
                            ret += "\n<imas:SpeakerLabel xml:lang=\"ja\">" + name + "</imas:SpeakerLabel>";
                        } else {
                            ret = "<imas:SpeakerLabel xml:lang=\"ja\">" + name + "</imas:SpeakerLabel>";
                        }
                        break;
                    case 1:
                        if (xml.ChildNodes[1].ChildNodes[1].InnerText != "") {
                            ret = "<schema:participant rdf:resource=\"" + xml.ChildNodes[1].ChildNodes[1].InnerText + "\"/>";
                        } else {
                            ret = "<schema:participant xml:lang=\"ja\">" + name + "</schema:participant>";
                        }
                        break;
                }

            }

            return ret;
        }
    }

    public class ComuRoot {
        public List<Comu> comu { get; set; }
        public ComuRoot() {
            comu = new List<Comu>();
        }
    }
    public class Comu {
        public string title { get; set; }
        public List<Texts> dialog { get; set; }
        public Comu() {
            dialog = new List<Texts>();
        }
    }
    public class Texts {
        public string name { get; set; }
        public string text { get; set; }
    }
}
