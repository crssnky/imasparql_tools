const fs = require('fs');
const rs = fs.createReadStream('D:\\Gits\\imasparql\\RDFs\\Staff.rdf');
const ws = fs.createWriteStream('D:\\Gits\\imasparql\\RDFs\\a.rdf');
const readline = require('readline');

let rl = readline.createInterface(rs, {});

let uri = "";
let name = ["", ""];
let out = "";
rl.on('line', function (line) {
  ws.write(line + "\n");
  if (line.includes("<schema:name xml:lang=\"ja\">")) {
    out = "<rdfs:label rdf:datatype=\"http://www.w3.org/2001/XMLSchema#string\">" + line.replace("    <schema:name xml:lang=\"ja\">", "").replace("</schema:name>", "") + "</rdfs:label>";
    ws.write(out + "\n");
  }
});