using PermutateFile.Parsers;

if (args.Length == 0)
{
    Console.WriteLine("Usage: PermutateFile <folder> <ChunkSize>, Default ChunkSize is 1048576");
    return;
}

var chunkSize = 1048576;
if (args.Length == 2)
    int.TryParse(args[1], out chunkSize);

var filenames = Directory.GetFiles(args[0], "*.*", SearchOption.AllDirectories);
for (int i = 0; i < filenames.Length; i++)
{
    var filename = filenames[i];
    Console.WriteLine($"Procesing {i + 1} of {filenames.Length}, {filename}");

    var extension = Path.GetExtension(filename);

    IParser? parser = null;
    switch (extension.ToLower())
    {
        case ".docx":
        case ".pptx":
        case ".rar":
        case ".zip":
            parser = new CompressParser();
            break;
        case ".jpg":
        case ".jpeg":
        case ".png":
            parser = new ImageParser();
            break;
        case ".pdf":
            parser = new PdfParser();
            break;
    }

    if (parser == null)
    {
        MoveTo(filename, "NotProcessed");
        continue;
    }

    var file = File.Open(filename, FileMode.Open);
    var fileData = new byte[file.Length];
    _ = file.Read(fileData, 0, fileData.Length);
    file.Close();

    var chunkCount = fileData.Length / chunkSize;
    var lastChunkSize = fileData.Length % chunkSize;
    if (lastChunkSize != 0)
        chunkCount++;

    if (chunkCount >= 8)
    {
        MoveTo(filename, "Big");
        continue;
    }

    var set = new List<int>();
    var dict = new Dictionary<int, int>();
    for (var j = 0; j < chunkCount; j++)
    {
        set.Add(j);
        if (j == chunkCount - 1 && lastChunkSize != 0)
            dict.Add(j, lastChunkSize);
        else
            dict.Add(j, chunkSize);
    }

    var permutations = GetPermutations(set);

    var answers = new List<byte[]>();
    foreach (var permutation in permutations)
    {
        var orj = true;
        for (int j = 0; j < permutation.Count; j++)
            if (permutation[j] != j)
            {
                orj = false;
                break;
            }

        if (orj)
            continue;

        var chunks = new List<byte>[chunkCount];
        var skip = 0;
        foreach (var idx in permutation)
        {
            var take = dict[idx];
            chunks[idx] = fileData.Skip(skip).Take(take).ToList();
            skip += take;
        }

        var newFileData = new List<byte>();
        for (int j = 0; j < chunkCount; j++)
            newFileData.AddRange(chunks[j]);

        var stream = new MemoryStream(newFileData.ToArray());
        if (!parser.IsCorrupted(stream))
            answers.Add(newFileData.ToArray());
        stream.Close();
    }

    if (answers.Count == 1)
    {
        WriteTo(filename, "OK", answers[0], -1);
        File.Delete(filename);
    }
    else if (answers.Count > 1)
    {
        for (var j = 0; j < answers.Count; j++)
        {
            var answer = answers[j];
            WriteTo(filename, "Multi", answer, j);
        }

        File.Delete(filename);
    }
    else
        MoveTo(filename, "Corrupted");
}

void MoveTo(string filename, string folder)
{
    var fileId = Path.GetDirectoryName(filename).Substring(args[0].Length + 1);
    var pureFilename = Path.GetFileName(filename);

    var newFolder = Path.Combine(args[0], "..", folder, fileId);
    if (!Directory.Exists(newFolder))
        Directory.CreateDirectory(newFolder);
    var newFilename = Path.Combine(newFolder, pureFilename);
    File.Move(filename, newFilename);
}

void WriteTo(string filename, string folder, byte[] fileData, int index)
{
    var fileId = Path.GetDirectoryName(filename).Substring(args[0].Length + 1);
    var pureFilename = Path.GetFileNameWithoutExtension(filename);
    var extension = Path.GetExtension(filename);

    var newFolder = Path.Combine(args[0], "..", folder, fileId);
    if (!Directory.Exists(newFolder))
        Directory.CreateDirectory(newFolder);
    string newFilename = "";
    
    if (index >= 0)
        newFilename = Path.Combine(newFolder, pureFilename + $"_{index}") + extension;
    else
        newFilename = Path.Combine(newFolder, pureFilename) + extension;

    var file = File.OpenWrite(newFilename);
    file.Write(fileData, 0, fileData.Length);
    file.Close();
}

List<List<int>> GetPermutations(List<int> set)
{
    var retVal = new List<List<int>>();
    if (set.Count <= 1)
    {
        retVal.Add(set);
        return retVal;
    }

    foreach (var item in set)
    {
        var newSet = new List<int>();
        newSet.AddRange(set.Where(x => x != item));
        var result = GetPermutations(newSet);
        foreach (var res in result)
        {
            res.Insert(0, item);
            retVal.Add(res);
        }
    }

    return retVal;
}










//
// var filenames = Directory.GetFiles(args[0], "*.*", SearchOption.AllDirectories);
// for (var i = 0; i < filenames.Length; i++)
// {
//     var filename = filenames[i];
//     Console.WriteLine($"Procesing {i + 1} of {filenames.Length}, {filename}");
//     try
//     {
//         var file = File.Open(filename, FileMode.Open);
//         var fileData = new byte[file.Length];
//         _ = file.Read(fileData, 0, fileData.Length);
//         file.Close();
//
//         var chunkCount = fileData.Length / chunkSize;
//         var lastChunkSize = fileData.Length % chunkSize;
//         if (lastChunkSize != 0)
//             chunkCount++;
//
//         var set = new List<int>();
//         var dict = new Dictionary<int, int>();
//         for (var j = 0; j < chunkCount; j++)
//         {
//             set.Add(j);
//             if (j == chunkCount - 1 && lastChunkSize != 0)
//                 dict.Add(j, lastChunkSize);
//             else
//                 dict.Add(j, chunkSize);
//         }
//
//         var directory = Path.GetDirectoryName(filename);
//         var pureFilename = Path.GetFileNameWithoutExtension(filename);
//         var extension = Path.GetExtension(filename);
//
//         var permutations = GetPermutations(set);
//         foreach (var permutation in permutations)
//         {
//             var orj = true;
//             for (int j = 0; j < permutation.Count; j++)
//                 if (permutation[j] != j)
//                 {
//                     orj = false;
//                     break;
//                 }
//
//             if (orj)
//                 continue;
//
//             var newFilename = pureFilename;
//             foreach (var j in permutation)
//                 newFilename += j;
//             newFilename += extension;
//             newFilename = Path.Combine(directory ?? "", newFilename);
//             var newFile = File.Open(newFilename, FileMode.Create);
//
//
//             var chunks = new List<byte>[chunkCount];
//             var skip = 0;
//             foreach (var idx in permutation)
//             {
//                 var take = dict[idx];
//                 chunks[idx] = fileData.Skip(skip).Take(take).ToList();
//                 skip += take;
//             }
//             
//             for (int j = 0; j < chunkCount; j++)
//             {
//                 var data = chunks[j].ToArray();
//                 newFile.Write(data, 0, data.Length);
//             }
//
//             newFile.Close();
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error in file {filename}, Message : {ex.Message}");
//     }
// }
