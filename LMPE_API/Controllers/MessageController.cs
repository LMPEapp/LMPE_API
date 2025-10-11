using LMPE_API.DAL;
using LMPE_API.Helpers;
using LMPE_API.Hubs;
using LMPE_API.Models;
using LMPE_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MessageDal _dal;
        private readonly IHubContext<MessageHub> _hub;
        private readonly PushService _pushService;
        private readonly FileStorageDal _dalFile;

        private readonly string resource = "messages";

        public MessageController(MessageDal dal, IHubContext<MessageHub> hub, PushService pushService, FileStorageDal dalFile)
        {
            _dal = dal;
            _hub = hub;
            _pushService = pushService;
            _dalFile = dalFile;
        }


        // GET groupe/{groupId}
        [Authorize]
        [HttpGet("groupe/{groupId:long}")]
        public ActionResult<IEnumerable<MessageOut>> GetByGroup(long groupId, [FromQuery] long? lastMessageId = null)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);
                var messages = _dal.GetByGroupId(groupId, tokenUserId, 20, lastMessageId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }


        // POST groupe/{groupId}
        [Authorize]
        [HttpPost("groupe/{groupId:long}")]
        public ActionResult<MessageOut> Create(long groupId, [FromBody] MessageIn input)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                input.UserId = tokenUserId;

                var id = _dal.Insert(groupId, input);
                var message = _dal.GetById(id, tokenUserId)!;

                _hub.Clients.Group($"{MessageHub.Groupe}{groupId}").SendAsync(MessageHub.ReceiveMessage, message);

                var userIdsToNotify = _dal.GetUserIdsToNotify(groupId);

                foreach (var userIdToNotify in userIdsToNotify)
                {
                    _hub.Clients.Group($"{MessageHub.User}{userIdToNotify}").SendAsync(MessageHub.ReceiveMessage, message);
                    if (userIdToNotify != tokenUserId)
                    {
                        try
                        {
                            _pushService.SendToUser(userIdToNotify,
                                $"Nouveau message {message.UserPseudo}",
                                $"{message.Content}");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erreur envoi push à l'utilisateur {userIdToNotify}: {ex.Message}");
                        }
                    }
                }


                return Ok(message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet("groupe/{groupId:long}/readAll")]
        public ActionResult<int> ReadAll(long groupId)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                // Appel DAL pour supprimer toutes les notifications non lues pour ce groupe
                int deletedCount = _dal.ReadAllMessagesForUser(groupId, tokenUserId);

                return Ok(deletedCount); // nombre de lignes supprimées
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet("Notification")]
        public ActionResult<int> GetNotificationCount()
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                // Appel DAL pour récupérer le nombre total de notifications
                int totalNotifications = _dal.GetTotalUnreadNotifications(tokenUserId);

                return Ok(totalNotifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }



        // PUT /message/{messageId}
        [Authorize]
        [HttpPut("groupe/{groupId:long}/{messageId:long}")]
        public IActionResult Update(long groupId, long messageId, [FromBody] MessageIn input)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                var result = _dal.Update(messageId, input);

                if (result)
                {
                    var message = _dal.GetById(messageId, tokenUserId);
                    _hub.Clients.Group($"{MessageHub.Groupe}{groupId}").SendAsync(MessageHub.UpdateMessage, message);
                }
                
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // DELETE /message/{messageId}
        [Authorize]
        [HttpDelete("groupe/{groupId:long}/{messageId:long}")]
        public IActionResult Delete(long groupId, long messageId)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);
                var message = _dal.GetById(messageId, tokenUserId)!;
                _dalFile.DeleteFile(resource, message.Content);
                var result = _dal.Delete(messageId);
                if (result)
                {
                   _hub.Clients.Group($"{MessageHub.Groupe}{groupId}").SendAsync(MessageHub.DeleteMessage, messageId);
                }
                
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpPost("groupe/{groupId:long}/upload")]
        public IActionResult Upload(long groupId, [FromForm] IFormFile file)
        {
            string unique = Guid.NewGuid().ToString("N");

            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                if (file == null || file.Length == 0)
                    return BadRequest("Aucun fichier envoyé");

                // 📂 Détection du type de fichier
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string[] videoExtensions = { ".mp4", ".mov", ".avi", ".mkv", ".webm" };

                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string contentType = file.ContentType.ToLowerInvariant();

                // Vérifie le type
                bool isImage = imageExtensions.Contains(extension) || contentType.StartsWith("image/");
                bool isVideo = videoExtensions.Contains(extension) || contentType.StartsWith("video/");

                // Définit le type pour le message
                string type;
                if (isImage) type = "image";
                else if (isVideo) type = "video";
                else type = "fichier";


                // 💾 Sauvegarde du fichier
                var filename = _dalFile.SaveFile(file, resource, unique);

                // 📨 Création du message
                var messageIN = new MessageIn
                {
                    Type = type,
                    UserId = tokenUserId,
                    Content = filename
                };

                var id = _dal.Insert(groupId, messageIN);
                var message = _dal.GetById(id, tokenUserId)!;

                // 📡 Envoi via SignalR
                _hub.Clients.Group($"{MessageHub.Groupe}{groupId}")
                    .SendAsync(MessageHub.ReceiveMessage, message);

                return Ok(message);
            }
            catch (Exception ex)
            {
                _dalFile.DeleteFile(resource, unique);
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

    }
}
