using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Xml;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            using (StreamReader file = File.OpenText(@"..\icu.json")) {
                JsonSerializer serializer = new JsonSerializer();
                Json json = (Json)serializer.Deserialize(file, typeof(Json));

                Console.WriteLine("icu.json:" + json.units.Count);
                var units = new List<Unitdef>();
                var comu = new StreamWriter(@"Unit2.rdf", false, Encoding.UTF8);
                var member = new StreamWriter(@"Member.rdf", false, Encoding.UTF8);
                var alter = new StreamWriter(@"Alter.rdf", false, Encoding.UTF8);
                var xmlns = new StreamReader(@"xmlns.txt", Encoding.UTF8);
                comu.WriteLine(xmlns.ReadToEnd());
                xmlns.Close();

                using (WebClient webclient = new WebClient()) {
                    webclient.Encoding = Encoding.UTF8;
                    foreach (var i in json.units) {
                        var unit = new Unitdef(i.uname, i.ids);
                        if (units.Contains(unit)) {
                            continue;
                        }

                        if (sparqling(webclient, i.uname)) {
                            if (nameSame(webclient, member, ref units, unit, json.idols) || memberSame(webclient, alter, ref units, unit, json.idols)) {
                                continue;
                            }

                            //書込み
                            Console.WriteLine(i.uname + ":" + i.uid);
                            comu.WriteLine("<rdf:Description rdf:about=\"detail/" + URIencoding(i.uname) + "\">");
                            comu.WriteLine("<schema:name rdf:datatype=\"https://www.w3.org/TR/xmlschema11-2/#string\">" + i.uname.Replace("&", "＆") + "</schema:name>");
                            foreach (var j in i.ids) {
                                sparqling(webclient, json.idols, j, comu);
                            }
                            comu.WriteLine("<rdf:type rdf:resource=\"https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#Unit\"/>");
                            comu.WriteLine("</rdf:Description>");
                            units.Add(unit);
                        }
                    }
                }
                comu.WriteLine("</rdf:RDF>");
                member.Close();
                alter.Close();
                comu.Close();
            }
        }
        static string URIencoding(string str) {
            return Uri.EscapeDataString(str).Replace("-", "%2d").Replace(".", "%2e").Replace("~", "%7e").ToUpper();
        }
        static void sparqling(WebClient web, List<Idol> idols, int id, StreamWriter comu) {
            var name = idols[id - 1].name;
            var download = web.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3e%0d%0aPREFIX%20rdf%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f1999%2f02%2f22%2drdf%2dsyntax%2dns%23%3e%0d%0aPREFIX%20rdfs%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f2000%2f01%2frdf%2dschema%23%3e%0d%0aPREFIX%20imas%3a%20%3chttps%3a%2f%2fsparql%2ecrssnky%2exyz%2fimasrdf%2fURIs%2fimas%2dschema%2ettl%23%3e%0d%0a%0d%0aSELECT%20distinct%20%3fs%20%0d%0aWHERE%20%7b%0d%0a%20%20%3fs%20schema%3aname%7cschema%3aalternateName%20%3fo%3b%0d%0a%20%20%20%20%20%20rdf%3atype%2frdfs%3asubClassOf%2a%20schema%3aPerson%3b%0d%0a%20%20%20%20%20%20schema%3amemberOf%20%3fm%3b%0d%0a%20%20%20%20%20filter%28regex%28str%28%3fo%29%2c%22" + URIencoding(name) + "%22%29%26%26contains%28str%28%3fm%29%2c%22Cin%22%29%29%2e%0d%0a%7d");
            var xml = new XmlDocument();
            xml.LoadXml(download);

            comu.WriteLine("<schema:member rdf:resource=\"" + xml.ChildNodes[1].ChildNodes[1].InnerText + "\"/>");
        }
        static bool sparqling(WebClient web, string name) {
            var download = web.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3e%0d%0aPREFIX%20rdf%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f1999%2f02%2f22%2drdf%2dsyntax%2dns%23%3e%0d%0aPREFIX%20imas%3a%20%3chttps%3a%2f%2fsparql%2ecrssnky%2exyz%2fimasrdf%2fURIs%2fimas%2dschema%2ettl%23%3e%0d%0aSELECT%20%3fs%0d%0aWHERE%20%7b%0d%0a%20%20%3fs%20schema%3aname%20%3fname%3b%0d%0a%20%20%20%20%20%20%20rdf%3atype%20imas%3aUnit%3b%0d%0a%20%20%20%20%20%20%20optional%7b%3fs%20schema%3aalternateName%20%3fname2%3b%7d%0d%0a%20%20%20%20%20%20%20bind%28%22" + URIencoding(name) + "%22%20as%20%3fsearch%29%0d%0a%20%20%20filter%28contains%28str%28%3fname%29%2c%3fsearch%29%7c%7ccontains%28str%28%3fname2%29%2c%3fsearch%29%29%0d%0a%7d");
            var xml = new XmlDocument();
            xml.LoadXml(download);

            if (xml.ChildNodes[1].ChildNodes[1].InnerText != "") {
                return false;
            }
            return true;
        }
        static bool nameSame(WebClient web, StreamWriter writer, ref List<Unitdef> units, Unitdef unit, List<Idol> idols) {
            foreach (var i in units) {
                if (i.name == unit.name) {
                    foreach (var j in unit.idols) {
                        if (!i.idols.Contains(j)) {
                            var download = web.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3e%0d%0aSELECT%20%3fs%20%0d%0aWHERE%20%7b%0d%0a%20%20%3fs%20schema%3aname%20%22" + idols[j - 1].name + "%22%40ja%3b%0d%0a%7d");
                            var xml = new XmlDocument();
                            xml.LoadXml(download);
                            writer.WriteLine("<!--" + unit.name + "-->");
                            writer.WriteLine("<schema:member rdf:resource=\"" + xml.ChildNodes[1].ChildNodes[1].InnerText + "\"/>");
                            i.idols.Add(j);
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        static bool memberSame(WebClient web, StreamWriter writer, ref List<Unitdef> units, Unitdef unit, List<Idol> idols) {
            //ここ
            foreach (var i in units) {
                if (i.idols.Count == unit.idols.Count) {
                    i.idols.Sort();
                    unit.idols.Sort();
                    if (i.idols.Equals(unit.idols)) {
                        writer.WriteLine("<!--" + i.name + "-->");
                        writer.WriteLine("<schema:alternateName rdf:datatype=\"https://www.w3.org/TR/xmlschema11-2/#string\">" + unit.name.Replace("&", "＆") + "</schema:alternateName>");

                        units.Add(unit);
                        return true;
                    }
                }
            }
            //SPARQL
            var member = "";
            var que = new string[] { "https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3ePREFIX%20rdf%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f1999%2f02%2f22%2drdf%2dsyntax%2dns%23%3ePREFIX%20imas%3a%20%3chttps%3a%2f%2fsparql%2ecrssnky%2exyz%2fimasrdf%2fURIs%2fimas%2dschema%2ettl%23%3eSELECT%20%3fname%20WHERE%20%7b%20%20%3fs%20rdf%3atype%20imas%3aUnit%3bschema%3aname%20%3fname%3bschema%3amember%20", "%3bschema%3amember%20%3fm%2e%7dgroup%20by%20%3fname%20having%28count%28%3fm%29%3d", "%29" };
            foreach (var i in unit.idols) {
                var download2 = web.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3e%0d%0aSELECT%20%3fs%20%0d%0aWHERE%20%7b%0d%0a%20%20%3fs%20schema%3aname%20%22" + idols[i - 1].name + "%22%40ja%3b%0d%0a%7d");
                var xml2 = new XmlDocument();
                xml2.LoadXml(download2);
                member += "<" + xml2.ChildNodes[1].ChildNodes[1].InnerText + ">,";
            }
            member = member.Substring(0, member.Length - 1);
            var query = que[0] + member + que[1] + unit.idols.Count + que[2];

            var download = web.DownloadString(query);
            var xml = new XmlDocument();
            xml.LoadXml(download);
            if (xml.ChildNodes[1].ChildNodes[1].InnerText != "") {
                writer.WriteLine("<!--" + xml.ChildNodes[1].ChildNodes[1].InnerText + "-->");
                writer.WriteLine("<schema:alternateName rdf:datatype=\"https://www.w3.org/TR/xmlschema11-2/#string\">" + unit.name.Replace("&", "＆") + "</schema:alternateName>");

                units.Add(unit);
                return true;
            }
            return false;
        }
    }

    public class Json {
        public List<Idol> idols { get; set; }
        public List<Event> events { get; set; }
        public List<Unit> units { get; set; }
        public Json() {
            idols = new List<Idol>();
            events = new List<Event>();
            units = new List<Unit>();
        }
    }
    public class Idol {
        public string id { get; set; }
        public string type { get; set; }
        public string img { get; set; }
        public string name { get; set; }
        public string kana { get; set; }
        public List<int> uids { get; set; }
        public Idol() {
            uids = new List<int>();
        }
    }
    public class Event {
        public string eid { get; set; }
        public string edate { get; set; }
        public string ename { get; set; }
        public string etype { get; set; }
    }
    public class Unit {
        public string eid { get; set; }
        public string edate { get; set; }
        public string uname { get; set; }
        public int uid { get; set; }
        public List<int> ids { get; set; }
        public Unit() {
            ids = new List<int>();
        }
    }

    public class Unitdef {
        public string name { get; set; }
        public List<int> idols { get; set; }
        public Unitdef(string s, List<int> l) {
            name = s;
            idols = l;
        }
    }
}
