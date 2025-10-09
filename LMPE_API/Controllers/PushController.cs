using LMPE_API.DAL;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("push")]
    public class PushController : ControllerBase
    {
        private readonly PushDal _dal;

        public PushController(PushDal dal)
        {
            _dal = dal;
        }

        // POST /push/register
        [HttpPost("register")]
        [Authorize]
        public IActionResult Register([FromBody] PushSubscriptionDto dto)
        {
            try
            {
                var sub = new PushSubscription
                {
                    UserId = dto.UserId,
                    Endpoint = dto.Subscription.Endpoint,
                    P256dh = dto.Subscription.Keys.P256dh,
                    Auth = dto.Subscription.Keys.Auth
                };

                _dal.Insert(sub);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }
    }
}
