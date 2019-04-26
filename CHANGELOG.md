## 0.1.4 (26-04-2019):

* Fixed a bug introduced in 0.1.3 that could lead to splitting of file names that contain dots.
* Fixed data loss occurring when two or more dot-separated keys have a common segment prefix somewhere deep in object structure.
* Implemented splitting of dot-separated names for array nodes that only contain plain values.
* Added support for conditional value parsers.
* Added support for custom settings node converters.

## 0.1.3 (08-04-2019):

Added unwrapping of ObjectNode with single child node by empty key.

## 0.1.2 (04-03-2019): 

Initial prerelease.