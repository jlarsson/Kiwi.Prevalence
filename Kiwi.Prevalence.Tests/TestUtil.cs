using System.IO;

namespace Kiwi.Prevalence.Tests
{
    public static class TestUtil
    {
        public static string CreateCleanTestFolder(string path)
        {
            var folderPath = Path.GetFullPath(path);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }
    }
}