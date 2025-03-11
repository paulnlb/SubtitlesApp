using System.Text;
using SubtitlesServer.TranslationApi.Helpers;

namespace SubtitlesServer.TranslationApi.Tests.HelpersTests;

[TestFixture]
public class JsonHelperTests
{
    [Test]
    public void UnwrapJsonArrayFromRootObject_ExpectUnwrappedSuccessfully()
    {
        var input = """{ "movies":[{ "name":"Garfield", "genre": "Comedy"}]}""";
        var expectedOutput = """[{ "name":"Garfield", "genre": "Comedy"}]""";

        var actualOutput = JsonHelper.UnwrapJsonArrayFromRootObject(input);

        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }

    [Test]
    public async Task UnwrapJsonArrayFromRootObjectAsync_ExpectUnwrappedSuccessfully()
    {
        var inputEnumerable = GetTestInputEnumerable();
        var expectedOutputEnumerable = GetTestOutputEnumerable();

        var actualOutputEnumerable = JsonHelper.UnwrapJsonArrayFromRootObjectAsync(inputEnumerable);

        var actualOutputEnumerator = actualOutputEnumerable.GetAsyncEnumerator();
        var expectedOutputEnumerator = expectedOutputEnumerable.GetAsyncEnumerator();

        while (await expectedOutputEnumerator.MoveNextAsync() && await actualOutputEnumerator.MoveNextAsync())
        {
            Assert.That(actualOutputEnumerator.Current, Is.EqualTo(expectedOutputEnumerator.Current));
        }
    }

    private static async IAsyncEnumerable<string> GetTestInputEnumerable()
    {
        yield return "{ ";
        yield return "\"movies\":";
        yield return "[{ ";
        yield return "\"name\":\"Garfield\", ";
        yield return "\"genre\": \"Comedy\"";
        yield return "}]}";
    }

    private static async IAsyncEnumerable<string> GetTestOutputEnumerable()
    {
        yield return "[{ ";
        yield return "\"name\":\"Garfield\", ";
        yield return "\"genre\": \"Comedy\"";
        yield return "}]";
    }
}
