using System.Drawing;

namespace PermutateFile.Parsers;

public class ImageParser:IParser
{
    public bool IsCorrupted(Stream stream)
    {
        try
        {
            var bitmap = new Bitmap(stream);
            return false;
        }
        catch
        {
            return true;
        }
    }
}