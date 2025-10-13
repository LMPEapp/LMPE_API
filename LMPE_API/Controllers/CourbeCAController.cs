using LMPE_API.DAL;
using LMPE_API.Helpers;
using LMPE_API.Hubs;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("ca")]
    public class CourbeCAController : ControllerBase
    {
        private readonly CourbeCADal _dal;
        private readonly IHubContext<CourbecaHub> _hub;

        private readonly string globalGroup = CourbecaHub.Groupe;

        public CourbeCAController(CourbeCADal dal, IHubContext<CourbecaHub> hub)
        {
            _dal = dal;
            _hub = hub;
        }

        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<CourbeCAGroupByDatePoint>> GetAllGroupeByDate([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] long? idUser = null)
        {
            try
            {
                return Ok(_dal.GetAllGroupeByDate(startDate, endDate, idUser));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }
        [Authorize]
        [HttpGet("data")]
        public ActionResult<IEnumerable<CourbeCA>> GetAll([FromQuery] long? lastId = null, [FromQuery] int pageSize = 30, [FromQuery] long? idUser = null)
        {
            try
            {
                return Ok(_dal.GetAll(lastId, pageSize, idUser));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id:long}")]
        public ActionResult<CourbeCA> GetById(long id)
        {
            try
            {
                var ca = _dal.GetById(id);
                if (ca == null) return NotFound();
                return Ok(ca);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult<CourbeCA> Create([FromBody] CourbeCAIn input)
        {
            try
            {
                var id = _dal.Insert(input);
                var ca = _dal.GetById(id)!;

                _hub.Clients.Group(globalGroup).SendAsync(CourbecaHub.CourbecaCreated, ca);

                return CreatedAtAction(nameof(GetById), new { id = ca.Id }, ca);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);
                var ca = _dal.GetById(id);
                if (ca.UserId != tokenUserId && !isAdmin)
                {
                    return StatusCode(403,"Token invalide");
                }

                var result = _dal.Delete(id);

                if (result)
                {
                    _hub.Clients.Group(globalGroup).SendAsync(CourbecaHub.CourbecaDeleted, ca);
                }

                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet("sum")]
        public ActionResult<decimal> GetSum([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var sum = _dal.GetSum(startDate, endDate);
                return Ok(sum);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }
    }
}
