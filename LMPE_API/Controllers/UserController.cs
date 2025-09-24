using LMPE_API.DAL;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDal _dal;

        public UserController(UserDal dal)
        {
            _dal = dal;
        }

        // GET /user
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll()
        {
            try
            {
                var users = _dal.GetAll();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // GET /user/{id}
        [Authorize]
        [HttpGet("{id:long}")]
        public ActionResult<User> GetById(long id)
        {
            try
            {
                var user = _dal.GetById(id);
                if (user == null) return NotFound();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // POST /user
        [Authorize]
        [HttpPost]
        public ActionResult<User> Create([FromBody] UserIn input)
        {
            try
            {
                // ⚡ Hash du mot de passe
                input.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.PasswordHash);

                var id = _dal.Insert(input);
                var user = _dal.GetById(id)!;

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // PUT /user/{id}
        [Authorize]
        [HttpPut("{id:long}")]
        public IActionResult Update(long id, [FromBody] UserIn input)
        {
            try
            {
                if (id == 1)
                {
                    return Forbid();
                }
                var ok = _dal.Update(id, input);
                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        // DELETE /user/{id}
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                if (id == 1)
                {
                    return Forbid();
                }
                var ok = _dal.Delete(id);
                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }
    }
}
