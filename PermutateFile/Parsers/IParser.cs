namespace PermutateFile.Parsers;

public interface IParser
{
    bool IsCorrupted(Stream stream);
}