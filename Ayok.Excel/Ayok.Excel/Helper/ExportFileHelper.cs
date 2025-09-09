using System.Diagnostics;

namespace Ayok.Excel.Helper
{
    public static class ExportFileHelper
    {
        public static async Task<string> SaveToDirectoryAsync(
            MemoryStream memoryStream,
            string directory,
            string fileName,
            bool addTimestamp = true
        )
        {
            Directory.CreateDirectory(directory);
            string path = (
                addTimestamp
                    ? $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    : (fileName + ".xlsx")
            );
            string fullPath = Path.Combine(directory, path);
            await File.WriteAllBytesAsync(fullPath, memoryStream.ToArray());
            return fullPath;
        }

        public static async Task<string> SaveToDDriveAsync(
            MemoryStream memoryStream,
            string fileName,
            string? subDirectory = null
        )
        {
            string text = "D:\\ExportFiles";
            string directory = (
                string.IsNullOrEmpty(subDirectory) ? text : Path.Combine(text, subDirectory)
            );
            return await SaveToDirectoryAsync(memoryStream, directory, fileName);
        }

        public static async Task<string> SaveToDesktopAsync(
            MemoryStream memoryStream,
            string fileName
        )
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return await SaveToDirectoryAsync(memoryStream, folderPath, fileName);
        }

        public static async Task<string> SaveToDocumentsAsync(
            MemoryStream memoryStream,
            string fileName,
            string? subDirectory = null
        )
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string directory = (
                string.IsNullOrEmpty(subDirectory)
                    ? folderPath
                    : Path.Combine(folderPath, subDirectory)
            );
            return await SaveToDirectoryAsync(memoryStream, directory, fileName);
        }

        public static async Task<List<string>> SaveMultipleToDirectoryAsync(
            Dictionary<string, MemoryStream> exports,
            string directory,
            bool addTimestamp = true
        )
        {
            List<string> savedFiles = new List<string>();
            foreach (KeyValuePair<string, MemoryStream> export in exports)
            {
                savedFiles.Add(
                    await SaveToDirectoryAsync(export.Value, directory, export.Key, addTimestamp)
                );
            }
            return savedFiles;
        }

        public static void OpenFileLocation(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", "/select,\"" + filePath + "\"");
                return;
            }
            throw new FileNotFoundException("文件不存在: " + filePath);
        }

        public static void OpenWithExcel(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                return;
            }
            throw new FileNotFoundException("文件不存在: " + filePath);
        }

        public static string GetFileSizeDisplay(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return "文件不存在";
            }
            long length = new FileInfo(filePath).Length;
            if (length >= 1024)
            {
                if (length >= 1048576)
                {
                    if (length >= 1073741824)
                    {
                        return $"{(double)length / 1073741824.0:F2} GB";
                    }
                    return $"{(double)length / 1048576.0:F2} MB";
                }
                return $"{(double)length / 1024.0:F2} KB";
            }
            return $"{length} B";
        }
    }
}
