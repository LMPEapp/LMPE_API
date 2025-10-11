namespace LMPE_API.DAL
{
    public class FileStorageDal
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageDal(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string SaveFile(IFormFile file, string resourceType, string resourceId)
        {
            // Crée le dossier correspondant à la ressource
            var folderPath = Path.Combine(_env.ContentRootPath, "uploads", resourceType);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Génère le nom de fichier unique : {resourceId}{extension}
            var fileName = resourceId + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            DeleteFile(resourceType, resourceId);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        public string GetUrl(string resourceType, string fileName)
        {
            return $"uploads/{resourceType}/{fileName}";
        }

        public bool DeleteFile(string resourceType, string resourceId)
        {
            try
            {
                var folderPath = Path.Combine(_env.ContentRootPath, "uploads", resourceType);
                if (!Directory.Exists(folderPath))
                    return false;

                // Recherche tous les fichiers commençant par l'id
                var files = Directory.GetFiles(folderPath, resourceId + ".*");
                if (files.Length == 0)
                    return false; // aucun fichier trouvé

                foreach (var file in files)
                {
                    File.Delete(file);
                }

                return true; // fichiers supprimés
            }
            catch
            {
                return false; // erreur lors de la suppression
            }
        }


    }

}
