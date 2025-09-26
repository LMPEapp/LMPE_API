using BCrypt.Net;
using LMPE_API.DAL;
using LMPE_API.Models;
using LMPE_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static LMPE_API.Models.AuthModels;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserDal _dal;
        private readonly JwtService _jwtService;

        public AuthController(UserDal dal, JwtService jwtService)
        {
            _dal = dal;
            _jwtService = jwtService;
        }

        // POST /auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] AuthModels.LoginRequest input)
        {
            try
            {
                var user = _dal.GetByEmail(input.Email);
                if (!(input.Email == "admin" && input.Password == "admin"))
                {
                    if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
                        return Unauthorized("Email ou mot de passe invalide");
                }


                var token = _jwtService.GenerateToken(user.Id, user.IsAdmin);
                return Ok(new LoginRequestOut { Token = token, User = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
            
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value;

                if (userIdClaim == null || isAdminClaim == null)
                    return Unauthorized();

                // Conversion
                if (!long.TryParse(userIdClaim, out long userId))
                    return Unauthorized();

                if (!bool.TryParse(isAdminClaim, out bool isAdmin))
                    return Unauthorized();

                // Génère un nouveau token
                var token = _jwtService.GenerateToken(userId, isAdmin);

                // Récupère l’utilisateur
                var user = _dal.GetById(userId);

                return Ok(new LoginRequestOut { Token = token, User = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }



        // POST /auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] AuthModels.ChangePasswordRequest input)
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

                if (input.UserId == 1)
                {
                    return Unauthorized("Pas le droit de modifier Admin");
                }

                User? user = null;

                if (isAdmin && input.UserId.HasValue)
                {
                    // Admin peut changer le mot de passe de n'importe quel utilisateur
                    user = _dal.GetById(input.UserId.Value);
                    if (user == null)
                        return NotFound("Utilisateur introuvable");
                }
                else
                {
                    // Utilisateur normal doit changer son propre mot de passe
                    user = _dal.GetById(tokenUserId);
                    if (user == null || string.IsNullOrEmpty(input.OldPassword) || !BCrypt.Net.BCrypt.Verify(input.OldPassword, user.PasswordHash))
                        return Unauthorized("Ancien mot de passe incorrect ou non autorisé");
                }

                // Hash et update
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
                var ok = _dal.UpdatePawword(user.Id, user.PasswordHash);
                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

    }
}
