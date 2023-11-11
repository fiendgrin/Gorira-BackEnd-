namespace Gorira.Helpers
{
    public static class FileManagerExtension
    {
        public static async Task<string> Save(this IFormFile file, string rootPath, params string[] path)
        {
            string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + file.FileName.Substring(file.FileName.LastIndexOf("."));
            string folderPath = Path.Combine(path);
            string filePath = Path.Combine(rootPath, folderPath, fileName);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }
    }
}
