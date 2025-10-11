using LMPE_API.DAL;
using LMPE_API.Helpers;
using LMPE_API.Hubs;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageReactionController : ControllerBase
    {
        private readonly MessageReactionDal _dal;
        private readonly IHubContext<MessageHub> _hub;

        public MessageReactionController(MessageReactionDal dal, IHubContext<MessageHub> hub)
        {
            _dal = dal;
            _hub = hub;
        }

        [Authorize]
        [HttpPost("groupe/{groupId:long}")]
        public ActionResult<MessageReactionOut> AddReaction(long groupId, [FromBody] MessageReactionIn reaction)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                reaction.UserId = tokenUserId;
                var id = _dal.Insert(reaction);

                var reactionOut = _dal.GetById(id);
                _hub.Clients.Group($"{MessageHub.Groupe}{groupId}").SendAsync(MessageHub.ReceiveReaction, reactionOut);
                return Ok(reactionOut); // retourne l'Id généré
            }
            catch (Exception ex)
            {
                // Gestion simple des erreurs (duplicate key, etc.)
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("groupe/{groupId:long}/{id:long}")]
        public ActionResult DeleteReaction(long groupId, long id)
        {
            var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

            var success = _dal.Delete(id);
            if (success)
            {
                _hub.Clients.Group($"{MessageHub.Groupe}{groupId}").SendAsync(MessageHub.DeleteReaction, id);
                return NoContent();
            }
                
            else
                return NotFound(new { message = "Réaction non trouvée" });
        }
    }
}
