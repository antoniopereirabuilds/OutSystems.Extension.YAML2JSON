using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.YAML2JSON {
    /// <summary>
    /// The IYAML2JSON defines the IYAML2JSON interface, providing
    /// functionality to convert YAML strings to JSON strings using the YamlDotNet library.
    /// </summary>
    [OSInterface(Description = "Extension to allow converting a YAML string into a JSON format using the YamlDotNet library.", IconResourceName = "OutSystems.YAML2JSON.resources.YAML2JSON_icon.png")]
    public interface IYAML2JSON {
        /// <summary>
        /// The ConvertYamlToJson method takes a YAML string, converts it to JSON format, and provides
        /// success and error information. If the conversion is successful, it returns the JSON string
        /// and sets isSuccess to true. If an exception occurs, it captures the error message and sets isSuccess to false.
        /// </summary>
        /// <param name="YamlToConvert">YAML string to be converted.</param>
        /// <param name="ConvertedJSON">Output parameter for the converted JSON string.</param>
        /// <param name="IsSuccess">Output parameter indicating the success of the conversion.</param>
        /// <param name="ErrorData">Output structure holding error position and message when the conversion fails.</param>
        [OSAction(Description = "Converts a YAML string into a JSON format using the YamlDotNet library. If an exception occurs, it captures the error message and sets IsSuccess to false.", IconResourceName = "OutSystems.YAML2JSON.resources.YAML2JSON_icon.png")]
        void ConvertYamlToJson(
            [OSParameter(Description = "The yaml text to convert.")]
            string YamlToConvert,
            [OSParameter(Description = "The resulting json from the conversion.")]
            out string ConvertedJSON,
            [OSParameter(Description = "'True' if the conversion was successful. Otherwise, 'False'.")]
            out bool IsSuccess,
            [OSParameter(Description = "The information in case of error.")]
            out Yaml2Json_Error ErrorData);
    }
}