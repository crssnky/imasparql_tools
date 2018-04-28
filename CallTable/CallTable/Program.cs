using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace CallTable {
    class Program {
        static void Main(string[] args) {
            {
                Console.WriteLine("<rdf:RDF");
                Console.WriteLine("    xmlns:jrrk=\"http://purl.org/jrrk#\"");
                Console.WriteLine("    xmlns:ic=\"http://imi.ipa.go.jp/ns/core/rdf#\"");
                Console.WriteLine("    xmlns:skos=\"http://www.w3.org/2004/02/skos/core#\"");
                Console.WriteLine("    xmlns:sac=\"http://statdb.nstac.go.jp/lod/sac/C\"");
                Console.WriteLine("    xmlns:dc=\"http://purl.org/dc/elements/1.1/\"");
                Console.WriteLine("    xmlns:pc=\"http://purl.org/procurement/public-contracts#\"");
                Console.WriteLine("    xmlns:odp=\"http://odp.jig.jp/odp/1.0#\"");
                Console.WriteLine("    xmlns:schema=\"http://schema.org/\"");
                Console.WriteLine("    xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"");
                Console.WriteLine("    xmlns:bibo=\"http://purl.org/ontology/bibo/\"");
                Console.WriteLine("    xmlns:foaf=\"http://xmlns.com/foaf/0.1/\"");
                Console.WriteLine("    xmlns:gr=\"http://purl.org/goodrelations/v1#\"");
                Console.WriteLine("    xmlns:owl=\"http://www.w3.org/2002/07/owl#\"");
                Console.WriteLine("    xmlns:dcterms=\"http://purl.org/dc/terms/\"");
                Console.WriteLine("    xmlns:xsd=\"http://www.w3.org/2001/XMLSchema#\"");
                Console.WriteLine("    xmlns:qb=\"http://purl.org/linked-data/cube#\"");
                Console.WriteLine("    xmlns:rdfs=\"http://www.w3.org/2000/01/rdf-schema#\"");
                Console.WriteLine("    xmlns:georss=\"http://www.georss.org/georss\"");
                Console.WriteLine("    xmlns:cal=\"http://www.w3.org/2002/12/cal/icaltzd#\"");
                Console.WriteLine("    xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\"");
                Console.WriteLine("    xmlns:imas=\"https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#\"");
                Console.WriteLine("    > ");
            }

            //CallTable Get
            using (WebClient webclient = new WebClient()) {
                webclient.Encoding = Encoding.UTF8;
                var tableString = webclient.DownloadString("https://api.imascg.moe/calltable");
                var tables = JsonConvert.DeserializeObject<List<table>>(tableString);

                var charString = webclient.DownloadString("https://api.imascg.moe/characters");
                var characters = JsonConvert.DeserializeObject<List<character>>(charString);
                foreach (var i in characters) {
                    //caller統一list
                    var list = tables.Where(x => x.caller == i.ID);
                    foreach (var j in list) {
                        //呼称組ごと処理
                        var callerName = characters.Where(x => x.ID == j.caller).ToList()[0].Name;
                        var calleeName = characters.Where(x => x.ID == j.callee).ToList()[0].Name;
                        var calledName = j.called;

                        if(true){ 
                        //URI取得
                        var downloaStr = webclient.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3ePREFIX%20rdf%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f1999%2f02%2f22%2drdf%2dsyntax%2dns%23%3ePREFIX%20imas%3a%20%3chttps%3a%2f%2fsparql%2ecrssnky%2exyz%2fimasrdf%2fURIs%2fimas%2dschema%2ettl%23%3ePREFIX%20rdfs%3a%20%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f2000%2f01%2frdf%2dschema%23%3eSELECT%20%3fcallers%20%3fcallees%20WHERE%20%7b%3fcallers%20rdf%3atype%20%3ftype%3b%20schema%3aname%7cschema%3aalternateName%20%3fcallero%3bfilter%28regex%28str%28%3fcallero%29%2c%22" + callerName + "%22%29%29%2e%3ftype%20rdfs%3asubClassOf%2a%20imas%3aCharacter%2e%3fcallees%20%20rdf%3atype%20%3ftype2%3bschema%3aname%7cschema%3aalternateName%20%3fcalleeo%3bfilter%28regex%28str%28%3fcalleeo%29%2c%22" + calleeName + "%22%29%29%2e%3ftype2%20rdfs%3asubClassOf%2a%20imas%3aCharacter%2e%7d");
                        var xml = new XmlDocument();
                        xml.LoadXml(downloaStr);
                            if (xml.ChildNodes[1].ChildNodes[1].HasChildNodes) {
                                var callerURI = xml.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[0].InnerText;
                                var calleeURI = xml.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[1].InnerText;
                                //Console.WriteLine(callerURI.Replace("https://sparql.crssnky.xyz/imasrdf/RDFs/detail/","") + "\t\t" + calleeURI.Replace("https://sparql.crssnky.xyz/imasrdf/RDFs/detail/", "") + "\t\t" + calledName);
                                Console.WriteLine("<rdf:Description rdf:about=\"detail/" + callerURI.Replace("https://sparql.crssnky.xyz/imasrdf/RDFs/detail/", "") + "_" + calleeURI.Replace("https://sparql.crssnky.xyz/imasrdf/RDFs/detail/", "")+"\">");
                                Console.WriteLine("<imas:Source rdf:resource=\"" + callerURI + "\"/>");
                                Console.WriteLine("<imas:Destination rdf:resource=\"" + calleeURI + "\"/>");
                                Console.WriteLine("<imas:Called xml:lang=\"ja\">" + calledName + "</imas:Called>");
                                Console.WriteLine("<rdf:type rdf:resource=\"https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#CallName\"/>");
                                Console.WriteLine("</rdf:Description>");
                            }
                        }
                    }
                }
            }
            Console.WriteLine("</rdf:RDF>");
        }
    }

    public class table {
        public string ID { get; set; }
        public string caller { get; set; }
        public string callee { get; set; }
        public string called { get; set; }
        public string remark { get; set; }
    }

    public class character {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
