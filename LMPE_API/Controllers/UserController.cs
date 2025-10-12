using LMPE_API.DAL;
using LMPE_API.Helpers;
using LMPE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDal _dal;
        private readonly FileStorageDal _dalFile;

        private readonly string resource = "users";

        public UserController(UserDal dal, FileStorageDal dalFile)
        {
            _dal = dal;
            _dalFile = dalFile;
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
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);
                if (id == 1)
                {
                    return StatusCode(403,"Pas le droit de modifier Admin");
                }
                if (tokenUserId != id && !isAdmin)
                {
                    return StatusCode(403,"Pas le droit de modifier");
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
        [Authorize]
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);
                if (id == 1)
                {
                    return StatusCode(403,"Pas le droit de suprimé Admin");
                }
                if (tokenUserId != id && !isAdmin)
                {
                    return StatusCode(403,"Pas le droit de suprimé");
                }
                var ok = _dal.Delete(id);

                _dalFile.DeleteFile(resource, id.ToString());

                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{id:long}/upload")]
        public IActionResult Upload(long id, [FromForm] IFormFile file)
        {
            try
            {
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);
                if (id != tokenUserId && !isAdmin)
                    return StatusCode(403,"Pas le droit de modifier cette image");

                if (file == null || file.Length == 0)
                    return BadRequest("Aucun fichier envoyé");

                // Sauvegarde via DAL
                var fileName = _dalFile.SaveFile(file, resource, id.ToString());

                var ok = _dal.UpdateUrlImage(id, fileName);

                // Génère l'URL publique
            
                return Ok(new { fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }
    }
}
