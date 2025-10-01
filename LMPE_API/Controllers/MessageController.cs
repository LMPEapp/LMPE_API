using LMPE_API.DAL;
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
    public class MessageController : ControllerBase
    {
        private readonly MessageDal _dal;
        private readonly IHubContext<MessageHub> _hub;

        public MessageController(MessageDal dal, IHubContext<MessageHub> hub)
        {
            _dal = dal;
            _hub = hub;
        }


        // GET groupe/{groupId}
        [Authorize]
        [HttpGet("groupe/{groupId:long}")]
        public ActionResult<IEnumerable<MessageOut>> GetByGroup(long groupId, [FromQuery] long? lastMessageId = null)
        {
            try
            {
                var messages = _dal.GetByGroupId(groupId, 20, lastMessageId);
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
                // optionnel : récupérer UserId depuis le token JWT
                // input.UserId = Convert.ToInt64(User.Claims.First(c => c.Type == "Id").Value);

                var id = _dal.Insert(groupId, input);
                var message = _dal.GetById(id)!;

                _hub.Clients.Group($"{MessageHub.Groupe}{groupId}").SendAsync(MessageHub.ReceiveMessage, message);

                return Ok(message);
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
                var result = _dal.Update(messageId, input);

                if (result)
                {
                    var message = _dal.GetById(messageId);
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
    }
}
