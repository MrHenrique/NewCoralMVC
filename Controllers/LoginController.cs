using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoralBlog.Models;
using CoralBlog.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoralBlog.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly RepoUsers _repoUsers;
        private readonly IConfiguration _configuration;

        public LoginController(
            ILogger<LoginController> logger,
            RepoUsers repoUsers,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _repoUsers = repoUsers;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email e senha são obrigatórios.");
                return View();
            }

            try
            {
                var user = await _repoUsers.GetUserByEmailAndPassword(email, password);
                if (user.Success)
                {
                    var token = GenerateJwtToken(user.Return);
                    Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Login inválido.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Um erro ocorreu ao tentar fazer login.");
                ModelState.AddModelError("", "Erro ao tentar fazer login.");
                return View();
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BlogUser user)
        {
            var result = await _repoUsers.CreateUser(user);
            if (result.Success)
            {
                var token = GenerateJwtToken(result.Return);
                Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", result.Error);
                return View();
            }
        }

        private string GenerateJwtToken(BlogUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                }
            );
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Login");
        }
    }
}
