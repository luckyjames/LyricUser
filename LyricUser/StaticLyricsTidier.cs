using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LyricUser
{
    /// <summary>
    /// Iterates through all lyrics tidying them up
    /// </summary>
    class StaticLyricsTidier
    {
        private static void TidyFile(string filePath)
        {
            XmlLyricsFileParsingStrategy reader = new XmlLyricsFileParsingStrategy(filePath);
            try
            {
                reader.ReadAll();
            }
            catch
            {
                // Something went wrong; only then do I try and correct the file
                System.Diagnostics.Debug.Print("Correcting {0}..", filePath);

                // Read using brute force
                IDictionary<string, string> data = XmlLyricsFileParsingStrategy.BruteForce(filePath);

                // Write again using normal XML writing to create file
                string newPath = filePath + ".correct.xml";
                XmlLyricsFileWritingStrategy.WriteToFile(newPath, data);

                // Check subsequent read now works
                XmlLyricsFileParsingStrategy resultantReader = new XmlLyricsFileParsingStrategy(newPath);
                try
                {
                    resultantReader.ReadAll();
                }
                catch
                {
                    // Delete the new file and bomb out
                    File.Delete(newPath);

                    throw;
                }

                // ReadAll will throw if the problem is still not fixed
                // so now we know it's fixed, swap the old file with the new one..
                File.Delete(filePath);
                File.Move(newPath, filePath);
            }
        }

        private static void TidyLyricsInTree(string rootFolderPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(rootFolderPath);
            FileInfo[] allFiles = dirInfo.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo fileInfo in allFiles)
            {
                TidyFile(fileInfo.FullName);
            }
        }
    }
}
