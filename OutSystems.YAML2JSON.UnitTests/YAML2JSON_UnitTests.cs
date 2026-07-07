using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OutSystems.YAML2JSON.UnitTests;

public static class JsonHelper
{
    public static string FormatJson(string json)
    {
        var parsedJson = JToken.Parse(json);
        return parsedJson.ToString(Formatting.Indented);
    }
}

[TestFixture]
public class Tests
{
    [TestCase("TestsData/test1_input.yaml", "TestsData/test1_result.json")]
    [TestCase("TestsData/test2_input.yaml", "TestsData/test2_result.json")]
    [TestCase("TestsData/test3_input.yaml", "TestsData/test3_result.json")]
    [TestCase("TestsData/QuotedLiterals.yaml", "TestsData/QuotedLiterals_expected.json")]
    public void ConvertYAML2JSON_ConvertSuccessfully(string input, string expectedResult)
    {
        var yaml2json = new Yaml2Json();
        var yamlText_input = File.ReadAllText(input);
        var expected_yamlText_output = File.ReadAllText(expectedResult);

        yaml2json.ConvertYamlToJson(yamlText_input, out string result, out bool isSuccess, out Yaml2Json_Error errorData);

        string formattedExpectedJson = JsonHelper.FormatJson(expected_yamlText_output);
        string formattedGeneratedJson = JsonHelper.FormatJson(result);

        Assert.That(formattedGeneratedJson, Is.EqualTo(formattedExpectedJson));
        Assert.That(isSuccess, Is.True);
        Assert.That(errorData.Message, Is.EqualTo(String.Empty));
    }

    [Test]
    public void ConvertYAML2JSON_ReturnsErrorWhenInvalidYAML()
    {
        var yaml2json = new Yaml2Json();
        var yamlText_input = File.ReadAllText("TestsData/invalid_input.yaml");

        yaml2json.ConvertYamlToJson(yamlText_input, out string result, out bool isSuccess, out Yaml2Json_Error errorData);
        Assert.IsFalse(isSuccess);
        Assert.IsEmpty(result);
        Assert.IsNotEmpty(errorData.Message);
    }

    [Test]
    public void ConvertYAML2JSON_ReturnsSemanticException()
    {
        var yaml2json = new Yaml2Json();
        var yamlTextInput = File.ReadAllText("TestsData/semantic_invalid_input.yaml");

        yaml2json.ConvertYamlToJson(yamlTextInput, out string result, out bool isSuccess, out Yaml2Json_Error errorData);

        Assert.IsFalse(isSuccess, "Conversion should not be successful for invalid input.");
        Assert.IsEmpty(result, "Result should be empty for invalid input.");
        Assert.IsNotEmpty(errorData.Message, "Error message should not be empty for invalid input.");

        Assert.Multiple(() =>
        {
            Assert.Greater(errorData.Start.Line, 0, "Start.Line should be greater than 0");
            Assert.Greater(errorData.Start.Column, 0, "Start.Column should be greater than 0");
            Assert.Greater(errorData.Start.Index, 0, "Start.Index should be greater than 0");
        });

        Assert.Multiple(() =>
       {
           Assert.Greater(errorData.End.Line, 0, "End.Line should be greater than 0");
           Assert.Greater(errorData.End.Column, 0, "End.Column should be greater than 0");
           Assert.Greater(errorData.End.Index, 0, "End.Index should be greater than 0");
       });

    }

    [Test]
    public void ConvertYAML2JSON_EmptyInput()
    {
        var yaml2json = new Yaml2Json();

        yaml2json.ConvertYamlToJson(String.Empty, out string result, out bool isSuccess, out Yaml2Json_Error errorData);
        Assert.That(isSuccess, Is.False);
        Assert.That(result, Is.Empty);

        Assert.That(errorData.Message, Is.Not.Empty);
        Assert.That(errorData.Message, Is.EqualTo("Error: The yaml text cannot be empty."));
    }

    [Test]
    public void ConvertYAML2JSON_NullInput()
    {
        var yaml2json = new Yaml2Json();

        yaml2json.ConvertYamlToJson(null!, out string result, out bool isSuccess, out Yaml2Json_Error errorData);
        Assert.That(isSuccess, Is.False);
        Assert.That(result, Is.Empty);

        Assert.That(errorData.Message, Is.EqualTo("Error: The yaml text cannot be empty."));
    }

    [Test]
    public void ConvertYAML2JSON_NonYamlError_LeavesDefaultErrorPositions()
    {
        var yaml2json = new Yaml2Json();

        yaml2json.ConvertYamlToJson(String.Empty, out string result, out bool isSuccess, out Yaml2Json_Error errorData);

        Assert.That(isSuccess, Is.False);
        Assert.Multiple(() =>
        {
            Assert.That(errorData.Start.Line, Is.EqualTo(-1));
            Assert.That(errorData.Start.Column, Is.EqualTo(-1));
            Assert.That(errorData.Start.Index, Is.EqualTo(-1));
            Assert.That(errorData.End.Line, Is.EqualTo(-1));
            Assert.That(errorData.End.Column, Is.EqualTo(-1));
            Assert.That(errorData.End.Index, Is.EqualTo(-1));
        });
    }

    [Test]
    public void ConvertYAML2JSON_RejectsAliases_BillionLaughs()
    {
        var yaml2json = new Yaml2Json();
        // Classic "billion laughs" alias-expansion payload.
        var input =
            "a: &a [\"x\",\"x\",\"x\",\"x\"]\n" +
            "b: &b [*a,*a,*a,*a]\n" +
            "c: &c [*b,*b,*b,*b]\n" +
            "d: [*c,*c,*c,*c]\n";

        yaml2json.ConvertYamlToJson(input, out string result, out bool isSuccess, out Yaml2Json_Error errorData);

        Assert.That(isSuccess, Is.False);
        Assert.That(result, Is.Empty);
        Assert.That(errorData.Message, Does.Contain("aliases"));
    }

    [Test]
    public void ConvertYAML2JSON_RejectsExcessiveNestingDepth()
    {
        var yaml2json = new Yaml2Json();
        // 200 levels of flow-sequence nesting exceeds the depth cap (100).
        var input = new string('[', 200) + new string(']', 200);

        yaml2json.ConvertYamlToJson(input, out string result, out bool isSuccess, out Yaml2Json_Error errorData);

        Assert.That(isSuccess, Is.False);
        Assert.That(result, Is.Empty);
        Assert.That(errorData.Message, Does.Contain("nesting depth"));
    }

    [Test]
    public void ConvertYAML2JSON_RejectsOversizedInput()
    {
        var yaml2json = new Yaml2Json();
        var input = new string('a', 1_048_576 + 1); // one char over the 1 MB cap

        yaml2json.ConvertYamlToJson(input, out string result, out bool isSuccess, out Yaml2Json_Error errorData);

        Assert.That(isSuccess, Is.False);
        Assert.That(result, Is.Empty);
        Assert.That(errorData.Message, Does.Contain("maximum allowed size"));
    }
}