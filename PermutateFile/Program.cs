{
    if (args.Length == 0)
    {
        Console.WriteLine("Usage: PermutateFile <folder> <ChunkSize>, Default ChunkSize is 1048576");
        return;
    }

    var chunkSize = 1048576;
    if (args.Length == 2)
        int.TryParse(args[1], out chunkSize);

    var filenames = Directory.GetFiles(args[0], "*.*", SearchOption.AllDirectories);
    for (var i = 0; i < filenames.Length; i++)
    {
        var filename = filenames[i];
        Console.WriteLine($"Procesing {i + 1} of {filenames.Length}, {filename}");
        try
        {
            var file = File.Open(filename, FileMode.Open);
            var fileData = new byte[file.Length];
            _ = file.Read(fileData, 0, fileData.Length);
            file.Close();

            var chunkCount = fileData.Length / chunkSize;
            var lastChunkSize = fileData.Length % chunkSize;
            if (lastChunkSize != 0)
                chunkCount++;

            var directory = Path.GetDirectoryName(filename);
            var pureFilename = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

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
            foreach (var permutation in permutations)
            {
                var chunks = new List<byte[]>();
                for (int j = 0; j < chunkCount; j++)
                    chunks.Add(Array.Empty<byte>());

                var read = 0;
                for (var j = 0; j < permutation.Count; j++)
                {
                    var index = permutation[j];
                    var size = dict[j];
                    chunks[index] = fileData.Skip(read).Take(size).ToArray();
                    read += size;
                }


                var newFilename = pureFilename;
                foreach (var j in permutation)
                    newFilename += j;
                newFilename += extension;
                newFilename = Path.Combine(directory ?? "", newFilename);
                var newFile = File.Open(newFilename, FileMode.Create);
                for (int j = 0; j < chunkCount; j++)
                    newFile.Write(chunks[j], 0, chunks[j].Length);
                newFile.Close();
            }



            // var chunks = new List<byte[]>();
            // var set = new List<int>();
            // for (int j = 0; j < chunkCount; j++)
            // {
            //     var data = new byte[chunkSize];
            //     var length = file.Read(data, 0, data.Length);
            //     chunks.Add(data.Take(length).ToArray());
            //     set.Add(j);
            // }
            // file.Close();
            //
            // var directory = Path.GetDirectoryName(filename);
            // var pureFilename = Path.GetFileNameWithoutExtension(filename);
            // var extension = Path.GetExtension(filename);
            // var permutations = GetPermutations(set);
            // foreach (var permutation in permutations)
            // {
            //     var newFilename = pureFilename;
            //     foreach (var j in permutation)
            //         newFilename += j;
            //     newFilename += extension;
            //
            //     newFilename = Path.Combine(directory ?? "", newFilename);
            //     var nfile = File.Open(newFilename, FileMode.Create);
            //     foreach (var index in permutation)
            //     {
            //         var data = chunks[index];
            //         nfile.Write(data, 0, data.Length);
            //     }
            //     nfile.Close();
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in file {filename}, Message : {ex.Message}");
        }
    }
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