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
    public class AgendaController : ControllerBase
    {
        private readonly AgendaDal _dal;
        private readonly IHubContext<AgendaHub> _hub;

        private readonly string globalGroup = AgendaHub.Groupe;

        public AgendaController(AgendaDal dal, IHubContext<AgendaHub> hub)
        {
            _dal = dal;
            _hub = hub;
        }

        // GET /agenda?startDate=...
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<AgendaOut>> GetAll([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                var agendas = _dal.GetAll(userId, startDate, endDate);
                return Ok(agendas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // GET /agenda/{id}
        [Authorize]
        [HttpGet("{id:long}")]
        public ActionResult<AgendaOut> GetById(long id)
        {
            try
            {
                var agenda = _dal.GetById(id);
                return agenda != null ? Ok(agenda) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // POST /agenda
        [Authorize]
        [HttpPost]
        public ActionResult<AgendaOut> Create([FromBody] AgendaIn input)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                input.CreatedBy = userId;

                var id = _dal.Insert(input);
                var agenda = _dal.GetById(id)!;

                _hub.Clients.Group(globalGroup).SendAsync(AgendaHub.AgendaCreated, agenda);

                return Ok(agenda);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // PUT /agenda/{id}
        [Authorize]
        [HttpPut("{id:long}")]
        public IActionResult Update(long id, [FromBody] AgendaIn input)
        {
            try
            {
                var result = _dal.Update(id, input);
                if (result)
                {
                    var agenda = _dal.GetById(id);
                    _hub.Clients.Group(globalGroup).SendAsync(AgendaHub.AgendaUpdated, agenda);
                }

                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // DELETE /agenda/{id}
        [Authorize]
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                var result = _dal.Delete(id);
                if (result)
                {
                    _hub.Clients.Group(globalGroup).SendAsync(AgendaHub.AgendaDeleted, id);
                }
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // POST /agenda/{agendaId}/users
        [Authorize]
        [HttpPost("{agendaId:long}/users")]
        public IActionResult AddUsersToAgenda(long agendaId, [FromBody] AgendaUserIn input)
        {
            _dal.AddUsersToAgenda(agendaId, input.UserIds);
            _hub.Clients.Group($"{globalGroup}{agendaId}").SendAsync(AgendaHub.AgendaUsersUpdated, agendaId);
            return Ok();
        }

        // DELETE /agenda/{agendaId}/users
        [Authorize]
        [HttpDelete("{agendaId:long}/users")]
        public IActionResult RemoveUsersFromAgenda(long agendaId, [FromBody] AgendaUserIn input)
        {
            _dal.RemoveUsersFromAgenda(agendaId, input.UserIds);
            _hub.Clients.Group($"{globalGroup}{agendaId}").SendAsync(AgendaHub.AgendaUsersUpdated, agendaId);
            return NoContent();
        }

        // GET /agenda/{agendaId}/users
        [Authorize]
        [HttpGet("{agendaId:long}/users")]
        public ActionResult<IEnumerable<User>> GetUsersForAgenda(long agendaId)
        {
            var users = _dal.GetUsersForAgenda(agendaId);
            return Ok(users);
        }
    }
}
