using LMPE_API.DAL;
using LMPE_API.Hubs;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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
        public ActionResult<IEnumerable<Message>> GetByGroup(long groupId, [FromQuery] long? lastMessageId = null)
        {
            try
            {
                var messages = _dal.GetByGroupId(groupId, 50, lastMessageId);
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
        public ActionResult<Message> Create(long groupId, [FromBody] MessageIn input)
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
        [HttpPut("{messageId:long}")]
        public IActionResult Update(long messageId, [FromBody] MessageIn input)
        {
            try
            {
                return _dal.Update(messageId, input) ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // DELETE /message/{messageId}
        [Authorize]
        [HttpDelete("{messageId:long}")]
        public IActionResult Delete(long messageId)
        {
            try
            {
                return _dal.Delete(messageId) ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }
    }
}
