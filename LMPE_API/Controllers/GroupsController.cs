using LMPE_API.DAL;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            // Récupère l'ID de l'utilisateur depuis le token JWT
            var userId = Convert.ToInt64(User.Claims.First(c => c.Type == "Id").Value);
            var groupes = _dal.GetAll(userId);
            return Ok(groupes);
        }


        // POST /groups
        [Authorize]
        [HttpPost]
        public ActionResult<GroupeConversation> Create([FromBody] GroupeConversationIn input)
        {
            var id = _dal.Insert(input);
            var g = _dal.GetById(id)!;
            return CreatedAtAction(nameof(GetById), new { id = g.Id }, g);
        }

        // GET /groups/{id}
        [Authorize]
        [HttpGet("{id:long}")]
        public ActionResult<GroupeConversation> GetById(long id)
        {
            var g = _dal.GetById(id);
            if (g == null) return NotFound();
            return Ok(g);
        }

        // PUT /groups/{id}
        [Authorize]
        [HttpPut("{id:long}")]
        public IActionResult Update(long id, [FromBody] GroupeConversationIn input)
        {
            return _dal.Update(id, input) ? NoContent() : NotFound();
        }

        // DELETE /groups/{id}
        [Authorize]
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            return _dal.Delete(id) ? NoContent() : NotFound();
        }

        // POST /groups/{groupId}/users
        [Authorize]
        [HttpPost("{groupId:long}/users")]
        public IActionResult AddUsers(long groupId, [FromBody] UserGroupeIn input)
        {
            _dal.AddUsers(groupId, input.UserIds);
            return NoContent();
        }

        // DELETE /groups/{groupId}/users/{userId}
        [Authorize]
        [HttpDelete("{groupId:long}/users/{userId:long}")]
        public IActionResult RemoveUser(long groupId, long userId)
        {
            return _dal.RemoveUser(groupId, userId) ? NoContent() : NotFound();
        }

        // GET /groups/{groupId}/users
        [Authorize]
        [HttpGet("{groupId:long}/users")]
        public ActionResult<IEnumerable<User>> GetUsers(long groupId)
        {
            return Ok(_dal.GetUsers(groupId)); // renvoie bien les utilisateurs complets
        }
    }

}
