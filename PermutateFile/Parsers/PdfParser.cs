using Aspose.Pdf;

namespace PermutateFile.Parsers;

public class PdfParser : IParser
{
    public bool IsCorrupted(Stream stream)
    {
        try
        {
            var document = new Document(stream);
            return false;
        }
        catch (Exception e)
        {
            return true;
        }
    }
}