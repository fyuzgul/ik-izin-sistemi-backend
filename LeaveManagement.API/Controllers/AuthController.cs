using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var result = await _authService.CreateUserAsync(createUserDto);
                if (result == null)
                {
                    return BadRequest(new { message = "Kullanıcı adı veya e-posta zaten kullanılıyor" });
                }

                return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserDto updateUserDto)
        {
            try
            {
                var result = await _authService.UpdateUserAsync(id, updateUserDto);
                if (!result)
                {
                    return BadRequest(new { message = "Kullanıcı güncellenemedi" });
                }

                return Ok(new { message = "Kullanıcı başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                var result = await _authService.DeactivateUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                return Ok(new { message = "Kullanıcı başarıyla deaktive edildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("users/{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var result = await _authService.ChangePasswordAsync(id, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
                if (!result)
                {
                    return BadRequest(new { message = "Eski şifre yanlış veya kullanıcı bulunamadı" });
                }

                return Ok(new { message = "Şifre başarıyla değiştirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("users/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(id, resetPasswordDto.NewPassword);
                if (!result)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                return Ok(new { message = "Şifre başarıyla sıfırlandı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _authService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            try
            {
                var role = await _authService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Rol bulunamadı" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
