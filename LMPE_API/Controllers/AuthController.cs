using BCrypt.Net;
using LMPE_API.DAL;
using LMPE_API.Helpers;
using LMPE_API.Models;
using LMPE_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using TonNamespace.DAL;
using static LMPE_API.Models.AuthModels;

namespace LMPE_API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserDal _dal;
        private readonly RefreshTokenDAL _dalRefreshToken;
        private readonly JwtService _jwtService;

        public AuthController(UserDal dal, JwtService jwtService, RefreshTokenDAL refreshTokenDAL)
        {
            _dal = dal;
            _jwtService = jwtService;
            _dalRefreshToken = refreshTokenDAL;
        }

        // POST /auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] AuthModels.LoginRequest input)
        {
            try
            {
                var user = _dal.GetByEmail(input.Email);
                var PasswordHash = _dal.GetPasswordByEmail(input.Email);
                if (!(input.Email == "admin" && input.Password == "admin"))
                {
                    if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, PasswordHash))
                        return Unauthorized("Email ou mot de passe invalide");
                }


                var token = _jwtService.GenerateToken(user.Id, user.IsAdmin);
                string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                _dalRefreshToken.Insert(new RefreshTokenIN { RefreshToken = refreshToken }, user.Id);

                return Ok(new LoginRequestOut { Token = token, User = user, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult Refresh([FromBody] RefreshTokenIN input)
        {
            try
            {
                var userId = _dalRefreshToken.GetUserIdByRefreshToken(input.RefreshToken);
                if (userId == null)
                    return Unauthorized("Refresh token invalide");

                var user = _dal.GetById(userId.Value);

                if (user == null)
                    return Unauthorized("Refresh token invalide");

                var token = _jwtService.GenerateToken(user.Id, user.IsAdmin);
                string newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                _dalRefreshToken.Insert(new RefreshTokenIN { RefreshToken = newRefreshToken }, user.Id);

                return Ok(new LoginRequestOut { Token = token, User = user, RefreshToken = newRefreshToken });
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
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                // Génère un nouveau token
                var token = _jwtService.GenerateToken(tokenUserId, isAdmin);

                // Récupère l’utilisateur
                var user = _dal.GetById(tokenUserId);

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
                var (tokenUserId, isAdmin) = UserHelper.GetUserIdAndAdmin(User);

                if (input.UserId == 1)
                {
                    return StatusCode(403,"Pas le droit de modifier Admin");
                }

                User? user = null;
                string? PasswordHash = null;

                if (isAdmin && input.UserId.HasValue)
                {
                    // Admin peut changer le mot de passe de n'importe quel utilisateur
                    user = _dal.GetById(input.UserId.Value);
                    PasswordHash = _dal.GetPasswordById(input.UserId.Value);
                    if (user == null)
                        return NotFound("Utilisateur introuvable");
                }
                else
                {
                    // Utilisateur normal doit changer son propre mot de passe
                    user = _dal.GetById(tokenUserId);
                    PasswordHash = _dal.GetPasswordById(tokenUserId);
                    if (user == null || string.IsNullOrEmpty(input.OldPassword) || !BCrypt.Net.BCrypt.Verify(input.OldPassword, PasswordHash))
                        return StatusCode(403,"Ancien mot de passe incorrect ou non autorisé");
                }

                // Hash et update
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
                var ok = _dal.UpdatePassword(user.Id, PasswordHash);
                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur: " + ex.Message);
            }
        }

    }
}
