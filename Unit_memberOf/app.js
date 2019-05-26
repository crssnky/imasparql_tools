var request = require("request");

const url = "https://sparql.crssnky.xyz/spql/imas/query?output=json&force-accept=text%2Fplain&query=PREFIX%20schema%3A%20%3Chttp%3A%2F%2Fschema.org%2F%3E%0APREFIX%20rdf%3A%20%3Chttp%3A%2F%2Fwww.w3.org%2F1999%2F02%2F22-rdf-syntax-ns%23%3E%0APREFIX%20imas%3A%20%3Chttps%3A%2F%2Fsparql.crssnky.xyz%2Fimasrdf%2FURIs%2Fimas-schema.ttl%23%3E%0ASELECT%20%20%3Fm%20(group_concat(%3Fs%3Bseparator%3D%22%2C%2C%2C%2C%2C%22)as%20%3Fu)%0AWHERE%20%7B%0A%20%20%3Fs%20rdf%3Atype%20imas%3AUnit%3B%0A%20%20%20%20%20schema%3Amember%20%3Fm.%0A%7Dgroup%20by%20(%3Fm)%20order%20by%20(%3Fm)";

console.log("<rdf:RDF");
console.log('    xmlns:schema="http://schema.org/"');
console.log('    xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"');
console.log('    xmlns:xsd="http://www.w3.org/2001/XMLSchema#"');
console.log('    xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#"');
console.log('    xmlns:imas="https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#"');
console.log("    >")

request.get(url, function (err, response, body) {
  var jsons = JSON.parse(body)["results"]["bindings"];
  jsons.forEach(i => {
    var idol = i.m.value.replace("https://sparql.crssnky.xyz/imasrdf/RDFs/", "");
    var units = i.u.value.split(",,,,,");

    console.log("  <rdf:Description rdf:about=\"" + idol + "\">");
    units.forEach(j => {
      console.log("    <schema:memberOf rdf:resource=\"" + j + "\"/>")
    });
    console.log("  </rdf:Description>");
  });
  console.log("</rdf:RDF>\n")
});