using SharpCompress.Readers;

namespace PermutateFile.Parsers;

public class CompressParser : IParser
{
    public bool IsCorrupted(Stream stream)
    {
        try
        {
            var reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                var ms = new MemoryStream();
                reader.WriteEntryTo(ms);
            }
        
            return false;
        }
        catch
        {
            return true;
        }
    }
}