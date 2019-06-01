var json = require("./res.json")

console.log("<rdf:RDF");
console.log('    xmlns:schema="http://schema.org/"');
console.log('    xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"');
console.log('    xmlns:xsd="http://www.w3.org/2001/XMLSchema#"');
console.log('    xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#"');
console.log('    xmlns:imas="https://sparql.crssnky.xyz/imasrdf/URIs/imas-schema.ttl#"');
console.log("    >")

var jsons = json["results"]["bindings"];
jsons.forEach(i => {
  var idol = i.m.value.replace("https://sparql.crssnky.xyz/imasrdf/RDFs/", "");
  var units = i.u.value.split(",,,,,");

  console.log("  <rdf:Description rdf:about=\"" + idol + "\">");
  units.forEach(j => {
    console.log("    <schema:memberOf rdf:resource=\"" + j + "\"/>")
  });
  console.log("  </rdf:Description>");
});
console.log("</rdf:RDF>\n");