
extern crate tendril;
extern crate html5ever;

use std::io::{self, Write};
use std::default::Default;
use tendril::{ByteTendril, ReadExt};
use html5ever::driver::ParseOpts;
use html5ever::tokenizer::Attribute;
use html5ever::tree_builder::TreeBuilderOpts;
use html5ever::{parse as parse_ever, one_input, serialize};
use html5ever::rcdom::{RcDom, Handle, Element, ElementEnum, NodeEnum, Node, Text};
fn main() {
    let text = "<div></div>".to_owned();
    let mut source = ByteTendril::new();
    text.as_bytes().read_to_tendril(&mut source).unwrap();
    let source = source.try_reinterpret().unwrap();
    let dom: RcDom = parse_ever(one_input(source), Default::default());
    //=> dom.document
}
