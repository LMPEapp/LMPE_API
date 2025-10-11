using LMPE_API.DAL;
using LMPE_API.Models;

namespace LMPE_API.Helpers
{
    public class MessageHelper
    {
        private readonly MessageDal _messageDal;
        private readonly FileStorageDal _fileStorageDal;

        public MessageHelper(MessageDal messageDal, FileStorageDal fileStorageDal)
        {
            _messageDal = messageDal;
            _fileStorageDal = fileStorageDal;
        }

        /// <summary>
        /// Supprime tous les messages vieux de plus de X jours et leurs fichiers associés.
        /// </summary>
        /// <param name="days">Nombre de jours avant suppression (par défaut 30)</param>
        public void DeleteOldMessages(int days = 30)
        {
            // 1️⃣ Récupère les anciens messages
            var oldMessages = _messageDal.GetOldMessages(days);

            foreach (var message in oldMessages)
            {
                // 2️⃣ Supprime le fichier si ce n'est pas un message texte
                if (message.Type != "texte" && !string.IsNullOrEmpty(message.Content))
                {
                    _fileStorageDal.DeleteFile("messages", message.Content);
                }

                // 3️⃣ Supprime le message de la base
                _messageDal.Delete(message.Id);
            }
        }
        public async Task RunDailyCleanup(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                DeleteOldMessages();
                await Task.Delay(TimeSpan.FromHours(24), token);
            }
        }
    }
}
