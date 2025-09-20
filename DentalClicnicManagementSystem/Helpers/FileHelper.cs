namespace CMS.Helpers
{
   
    public class FileHelper
    {
        private readonly IWebHostEnvironment _env;
        public FileHelper(IWebHostEnvironment env) => _env = env;

        /// <summary>
        /// Saves the uploaded file under wwwroot/<folder>/ and returns its relative URL.
        /// Returns null if no file was supplied.
        /// </summary>
        public async Task<string?> SaveAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            var root = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(root);

            var ext = Path.GetExtension(file.FileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(root, name);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{folder}/{name}".Replace('\\', '/');
        }

        /// <summary>
        /// Deletes the physical file that corresponds to url (if it exists and is not the default image).
        /// </summary>
        public void DeleteIfNotDefault(string? url, string defaultUrl = "/uploads/doctors/default.jpg")
        {
            if (string.IsNullOrEmpty(url) || url.Equals(defaultUrl, StringComparison.OrdinalIgnoreCase))
                return;

            var path = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
