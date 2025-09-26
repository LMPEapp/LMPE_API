using LMPE_API.DAL;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("groups")]
    public class GroupsController : ControllerBase
    {
        private readonly GroupeConversationDal _dal;

        public GroupsController(GroupeConversationDal dal)
        {
            _dal = dal;
        }

        // GET /groups
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<GroupeConversation>> GetAll()
        {
            try
            {
                // Récupérer l'ID et isAdmin depuis le token JWT
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value;

                if (userIdClaim == null || isAdminClaim == null)
                    return Unauthorized("Token invalide");

                long tokenUserId = long.Parse(userIdClaim);
                bool isAdmin = bool.Parse(isAdminClaim);
                var groupes = _dal.GetAll(tokenUserId);
                return Ok(groupes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }


        // POST /groups
        [Authorize]
        [HttpPost]
        public ActionResult<GroupeConversation> Create([FromBody] GroupeConversationIn input)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value;

                if (userIdClaim == null || isAdminClaim == null)
                    return Unauthorized("Token invalide");

                long tokenUserId = long.Parse(userIdClaim);
                bool isAdmin = bool.Parse(isAdminClaim);

                var id = _dal.Insert(input);
                _dal.AddUsers(id, [tokenUserId]);
                var g = _dal.GetById(id)!;
                return CreatedAtAction(nameof(GetById), new { id = g.Id }, g);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // GET /groups/{id}
        [Authorize]
        [HttpGet("{id:long}")]
        public ActionResult<GroupeConversation> GetById(long id)
        {
            try
            {
                var g = _dal.GetById(id);
                if (g == null) return NotFound();
                return Ok(g);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // PUT /groups/{id}
        [Authorize]
        [HttpPut("{id:long}")]
        public IActionResult Update(long id, [FromBody] GroupeConversationIn input)
        {
            try
            {
                return _dal.Update(id, input) ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // DELETE /groups/{id}
        [Authorize]
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                return _dal.Delete(id) ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // POST /groups/{groupId}/users
        [Authorize]
        [HttpPost("{groupId:long}/users")]
        public IActionResult AddUsers(long groupId, [FromBody] UserGroupeIn input)
        {
            try
            {
                _dal.AddUsers(groupId, input.UserIds);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // DELETE /groups/{groupId}/users/{userId}
        [Authorize]
        [HttpDelete("{groupId:long}/users/{userId:long}")]
        public IActionResult RemoveUser(long groupId, long userId)
        {
            try
            {
                return _dal.RemoveUser(groupId, userId) ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // GET /groups/{groupId}/users
        [Authorize]
        [HttpGet("{groupId:long}/users")]
        public ActionResult<IEnumerable<User>> GetUsers(long groupId)
        {
            try
            {
                return Ok(_dal.GetUsers(groupId)); // renvoie bien les utilisateurs complets
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }
    }

}
