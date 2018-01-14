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
            Console.WriteLine("CallTable");

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

                        //URI取得
                        var downloaStr = webclient.DownloadString("https://sparql.crssnky.xyz/spql/imas/query?query=PREFIX%20schema%3a%20%3chttp%3a%2f%2fschema%2eorg%2f%3ePREFIX%20rdf%3a%20%3chttp%3a%2f%2fwww%2ew3%2eorg%2f1999%2f02%2f22%2drdf%2dsyntax%2dns%23%3ePREFIX%20imas%3a%20%3chttps%3a%2f%2fsparql%2ecrssnky%2exyz%2fimasrdf%2fURIs%2fimas%2dschema%2ettl%23%3eSELECT%20%3fcallers%20%3fcallees%20WHERE%20%7b%3fcallers%20rdf%3atype%20imas%3aIdol%3b%20schema%3aname%20%3fcallero%3bfilter%28regex%28str%28%3fcallero%29%2c%22" + callerName + "%22%29%29%2e%3fcallees%20%20rdf%3atype%20imas%3aIdol%3bschema%3aname%20%3fcalleeo%3bfilter%28regex%28str%28%3fcalleeo%29%2c%22" + calleeName + "%22%29%29%2e%7d");
                        var xml = new XmlDocument();
                        xml.LoadXml(downloaStr);
                        if (xml.ChildNodes[1].ChildNodes[1].HasChildNodes) {
                            var callerURI = xml.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[0].InnerText;
                            var calleeURI = xml.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[1].InnerText;
                            Console.WriteLine(callerURI.Replace("https://sparql.crssnky.xyz/imasrdf/RDFs/detail/","") + "\t\t" + calleeURI.Replace("https://sparql.crssnky.xyz/imasrdf/RDFs/detail/", "") + "\t\t" + calledName);
                        }
                    }
                }
            }
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
