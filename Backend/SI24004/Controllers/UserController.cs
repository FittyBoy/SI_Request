using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SI24004.Models;
using SI24004.Models.Requests;
using SI24004.ModelsMysql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly SqlServerContext _mySqlContext;
        private readonly IConfiguration _config;
        public UserController(PostgrestContext context, IConfiguration config, SqlServerContext mysqlContext)
        {
            _context = context;
            _config = config;
            _mySqlContext = mysqlContext;
        }
        [HttpPost("login")]
        [EnableCors("AllowFrontend")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.UserId) || string.IsNullOrEmpty(loginRequest.UserPassword))
            {
                return BadRequest(new { Message = "User ID and Password are required." });
            }

            try
            {
                var user = await _context.Users
                            .Include(u => u.Role)
                            .Include(u => u.Section)
                            .FirstOrDefaultAsync(x => x.UserId == loginRequest.UserId);


                if (user == null)
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // สร้าง JWT Token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config.GetValue<string>("Jwt:Secret") ?? throw new InvalidOperationException("JWT Secret not found"));

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User"),
                new Claim("SectionId", user.SectionId?.ToString() ?? Guid.Empty.ToString())
            }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new
                {
                    Token = tokenString,
                    SectionId = user.SectionId,
                    SectionName = user.Section?.SectionName,
                    User = new
                    {
                        user.Id,
                        user.UserId,
                        user.UserName,
                        user.Role?.RoleName,
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Error = ex.Message });
            }
        }
        [HttpPost("loginMySql")]
        [EnableCors("AllowFrontend")]
        public async Task<IActionResult> LoginMySql([FromBody] LoginDto loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.UserId) || string.IsNullOrEmpty(loginRequest.UserPassword))
            {
                return BadRequest(new { Message = "User ID and Password are required." });
            }

            try
            {
                // ค้นหา user จาก database ตาม UserLoginName
                var user = await _mySqlContext.UserInfos // ใช้ UserInfos ตามที่มีอยู่
                            .FirstOrDefaultAsync(x => x.UserLoginName == loginRequest.UserId); // ใช้ property ที่ถูกต้อง

                if (user == null)
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // ตรวจสอบ password (แนะนำให้เข้ารหัสใน production)
                if (user.UserPassword != loginRequest.UserPassword)
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // อัพเดท Last_Login
                user.LastLogin = DateTime.Now;
                await _mySqlContext.SaveChangesAsync();

                // สร้าง JWT Token
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        UserId = user.UserId,
                        UserCode = user.UserCode,
                        UserLoginName = user.UserLoginName,
                        UserFullname = user.UserFullname,
                        UserEmail = user.UserEmail,
                        UserImage = user.UserImage,
                        UserLevel = user.UserLevel,
                        UserGroupId = user.UserGroupId,
                        SectionId = user.SectionId,
                        LastLogin = user.LastLogin
                    },
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                // Log exception ที่นี่
                return StatusCode(500, new { Message = "An error occurred while processing your request." });
            }
        }

        private string GenerateJwtToken(UserInfo user) // ใช้ UserInfo ตาม Model ที่มีอยู่
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _config.GetValue<string>("Jwt:Secret");

            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("JWT Secret not found in configuration");

            var key = Encoding.ASCII.GetBytes(jwtSecret);

            // สร้าง claims จากข้อมูลในตาราง
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserFullname ?? user.UserLoginName),
                new Claim(ClaimTypes.Email, user.UserEmail ?? ""),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("UserCode", user.UserCode?.ToString() ?? ""),
                new Claim("UserLoginName", user.UserLoginName ?? ""),
                new Claim("UserFullname", user.UserFullname ?? ""),
                new Claim("UserEmail", user.UserEmail ?? ""),
                new Claim("UserLevel", user.UserLevel?.ToString() ?? "1"),
                new Claim("UserGroupId", user.UserGroupId.ToString() ?? ""),
                new Claim("SectionId", user.SectionId.ToString() ?? ""),
                new Claim("jti", Guid.NewGuid().ToString()), // JWT ID
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // กำหนด Role ตาม User_Level
            string roleName = GetRoleName(user.UserLevel ?? 1);
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // กำหนดเวลาหมดอายุ 8 ชั่วโมง
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow,
                Issuer = _config.GetValue<string>("Jwt:Issuer") ?? "YourAppName",
                Audience = _config.GetValue<string>("Jwt:Audience") ?? "YourAppUsers",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Method สำหรับแปลง User_Level เป็น Role name
        private string GetRoleName(int userLevel)
        {
            return userLevel switch
            {
                1 => "Admin",
                2 => "Manager",
                3 => "User",
                _ => "Guest"
            };
        }

        // Model class สำหรับ UserInfo table (ปรับให้ตรงกับ Entity ที่มีอยู่)
        

        // เพิ่ม method สำหรับเข้ารหัส password (แนะนำให้ใช้)
        private string HashPassword(string password)
        {
            // ใช้ BCrypt สำหรับ hash password
            // return BCrypt.Net.BCrypt.HashPassword(password);
            return password; // placeholder - ไม่แนะนำให้ใช้ใน production
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // ใช้ BCrypt สำหรับตรวจสอบ password
            // return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            return password == hashedPassword; // placeholder - ไม่แนะนำให้ใช้ใน production
        }
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = HttpContext.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return Unauthorized();

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var userId))
                return Unauthorized();

            var dbUser = await _context.Users
                .Include(u => u.Section)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (dbUser == null)
                return Unauthorized();

            var userInfo = new
            {
                Id = dbUser.Id,
                UserName = dbUser.UserName,
                Role = user.FindFirst(ClaimTypes.Role)?.Value,
                SectionId = dbUser.SectionId,
                SectionName = dbUser.Section?.SectionName
            };

            return Ok(userInfo);
        }


    }

}
