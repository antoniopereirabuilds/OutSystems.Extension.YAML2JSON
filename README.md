# Outsystems-ODC-Ext-YAML2JSON

YAML2JSON is an OutSystems ODC extension that enables the conversion of YAML strings into JSON format using the [YamlDotNet library](https://github.com/aaubry/YamlDotNet) (v15.3.0). It is designed to facilitate seamless data format conversion within OutSystems applications, enhancing data interoperability and management.

## Features
### Actions

This extension provides the following action:

- **ConvertYamlToJson Action**: This method accepts a YAML string, converts it into a JSON format, and outputs the results along with success and error information.

| Type       | Parameter       | Description                                                          | Data Type                  |
|------------|-----------------|----------------------------------------------------------------------|----------------------------|
| Input      | `yamlToConvert` | The YAML string that needs to be converted.                          | Text                       |
| Output     | `convertedJSON` | Output parameter that holds the converted JSON string.               | Text                       |
| Output     | `isSuccess`     | Output parameter indicating whether the conversion was successful.   | Boolean                    |
| Output     | `errorMessage`  | Output parameter that stores any error messages encountered during the conversion. | Yaml2Json_Error structure |

The method provides robust error handling. If the conversion process encounters any issues, it captures the specific error message, sets the `isSuccess` flag to `false`, and provides detailed error information through the `errorMessage` parameter.

#### Structures

- **Yaml2Json_Error Structure**: structure holds detailed information about any errors that occur during the conversion process.

| Field     | Description                                                         | Data Type                           |
|-----------|---------------------------------------------------------------------|-------------------------------------|
| `Start`   | Marks the start position of the error within the YAML content.      | Yaml2Json_ErrorPosition structure   |
| `End`     | Marks the end position of the error.                                | Yaml2Json_ErrorPosition structure   |
| `Message` | Contains the descriptive error message generated during conversion. | Text                                |


- **Yaml2Json_ErrorPosition Structure**: structure is used to specify the line, column, and index positions where an error occurred.

| Field    | Description                                           | Data Type | Default Value |
|----------|-------------------------------------------------------|-----------|---------------|
| `Line`   | The line number where the error occurred.             | Integer   | -1            |
| `Column` | The column number where the error occurred.           | Integer   | -1            |
| `Index`  | The index within the document where the error occurred.| Integer  | -1            |




## Installation

To install the YAML2JSON extension in your OutSystems environment, follow the standard procedure for installing ODC extensions.

## License

This project is licensed under the BSD 3-Clause License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to the YamlDotNet library maintainers for providing the core functionality used in this extension.
- OutSystems community for the ongoing support and feedback.