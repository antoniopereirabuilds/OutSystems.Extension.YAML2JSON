using System;
using System.IO;
using YamlDotNet.Serialization;

namespace OutSystems.YAML2JSON
{
    /// <summary>
    /// The Yaml2Json class implements the IYAML2JSON interface, providing
    /// functionality to convert YAML strings to JSON strings using the YamlDotNet library.
    /// </summary>
    public class Yaml2Json : IYAML2JSON
    {
        // Bounds to protect the (Lambda-backed) worker from untrusted-input resource exhaustion.
        private const int MaxInputLength = 1_048_576; // 1 MB of characters
        private const int MaxNestingDepth = 100;

        /// <summary>
        /// The ConvertYamlToJson method takes a YAML string, converts it to JSON format, and provides
        /// success and error information. If the conversion is successful, it returns the JSON string
        /// and sets IsSuccess to true. If an exception occurs, it captures the error message and sets IsSuccess to false.
        /// </summary>
        /// <param name="YamlToConvert">YAML string to be converted.</param>
        /// <param name="ConvertedJSON">Output parameter for the converted JSON string.</param>
        /// <param name="IsSuccess">Output parameter indicating the success of the conversion.</param>
        /// <param name="ErrorData">Output structure for errors encountered during the conversion.</param>
        public void ConvertYamlToJson(string YamlToConvert, 
                                      out string ConvertedJSON, 
                                      out bool IsSuccess, 
                                      out Yaml2Json_Error ErrorData)
        {
            // Set outputs to default value
            IsSuccess = false;
            ConvertedJSON = String.Empty;
            ErrorData = new Yaml2Json_Error();

            try
            {

                if (String.IsNullOrEmpty(YamlToConvert))
                {
                    throw new ArgumentException("The yaml text cannot be empty.");
                }

                if (YamlToConvert.Length > MaxInputLength)
                {
                    throw new ArgumentException("The yaml text exceeds the maximum allowed size of " + MaxInputLength + " characters.");
                }

                // Guard against DoS payloads (alias expansion / deep nesting) before the
                // recursive deserialize+serialize runs. A StackOverflowException from deep
                // recursion is not catchable, so it must be prevented up front.
                ValidateBounds(YamlToConvert);

                using var input = new StringReader(YamlToConvert);
                var deserializer = new DeserializerBuilder()
                    .WithAttemptingUnquotedStringTypeDeserialization()
                    .Build();

           
                var yamlObject = deserializer.Deserialize(input);
                var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                    .JsonCompatible()                  
                    .Build();

                ConvertedJSON = serializer.Serialize(yamlObject);

                IsSuccess = true;
            }
            // SyntaxErrorException and SemanticErrorException both derive from YamlException.
            catch (YamlDotNet.Core.YamlException ex)
            {
                IsSuccess = false;
                ErrorData.Start = new Yaml2Json_ErrorPosition(ex.Start.Line, ex.Start.Column, ex.Start.Index);
                ErrorData.End = new Yaml2Json_ErrorPosition(ex.End.Line, ex.End.Column, ex.End.Index);
                ErrorData.Message = "[" + ex.Start + "] - [" + ex.End + "]: " + ex.Message;
            }
            catch (ArgumentException ex)
            {
                IsSuccess = false;
                ErrorData.Message = "Error: " + ex.Message;
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                ErrorData.Message = "Error: " + ex.Message;
            }
        }

        /// <summary>
        /// Streams the YAML events once (iteratively, no recursion) to reject documents that
        /// would cause resource exhaustion during conversion: YAML aliases (which JSON cannot
        /// represent and which expand exponentially when serialized), and nesting deeper than
        /// <see cref="MaxNestingDepth"/> (which would risk an uncatchable StackOverflowException).
        /// Malformed YAML surfaces here as a YamlException, handled by the caller's catch.
        /// </summary>
        private static void ValidateBounds(string yaml)
        {
            using var reader = new StringReader(yaml);
            var parser = new YamlDotNet.Core.Parser(reader);
            int depth = 0;

            while (parser.MoveNext())
            {
                switch (parser.Current)
                {
                    case YamlDotNet.Core.Events.MappingStart:
                    case YamlDotNet.Core.Events.SequenceStart:
                        depth++;
                        if (depth > MaxNestingDepth)
                        {
                            throw new ArgumentException("The yaml text exceeds the maximum allowed nesting depth of " + MaxNestingDepth + ".");
                        }
                        break;
                    case YamlDotNet.Core.Events.MappingEnd:
                    case YamlDotNet.Core.Events.SequenceEnd:
                        depth--;
                        break;
                    case YamlDotNet.Core.Events.AnchorAlias:
                        throw new ArgumentException("The yaml text contains aliases (*), which are not supported.");
                }
            }
        }
    }
}