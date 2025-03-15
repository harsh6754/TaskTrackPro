using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using Repositories.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterLoginController : ControllerBase
    {
        private readonly IRegisterLoginInterface _registerLoginInterface;
        private readonly IConfiguration _config;
        public RegisterLoginController(IRegisterLoginInterface registerLoginInterface, IConfiguration config)
        {
            _registerLoginInterface = registerLoginInterface ?? throw new ArgumentNullException(nameof(registerLoginInterface));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private string GenerateJwtToken(t_Register register)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"] ?? "default_subject"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, register.c_email),
        new Claim("UserId", register.c_userId.ToString()) // Add UserId for identification
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key missing.")));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: signIn
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpGet("GetCountries")]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var countries = await _registerLoginInterface.GetCountries();
                return Ok(countries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching countries.", error = ex.Message });
            }
        }

        [HttpGet("GetStates")]
        public async Task<IActionResult> GetStates([FromQuery] int countryId)
        {
            try
            {
                var states = await _registerLoginInterface.GetStates(countryId);
                return Ok(states);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching states.", error = ex.Message });
            }
        }

        [HttpGet("GetDistricts")]
        public async Task<IActionResult> GetDistricts([FromQuery] int stateId)
        {
            try
            {
                var districts = await _registerLoginInterface.GetDistricts(stateId);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching districts.", error = ex.Message });
            }
        }

        [HttpGet("GetCities")]
        public async Task<IActionResult> GetCities([FromQuery] int districtId)
        {
            try
            {
                var cities = await _registerLoginInterface.GetCities(districtId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching cities.", error = ex.Message });
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] Repositories.Models.t_Register register)
        {
            if (register.ImageFile != null && register.ImageFile.Length > 0)
            {
                var fileName = register.c_email + Path.GetExtension(
                    register.ImageFile.FileName
                );
                var filePath = Path.Combine("../Client/wwwroot/Images", fileName);
                register.c_image = fileName;
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await register.ImageFile.CopyToAsync(fileStream);
                }
            }
            Console.WriteLine(register.c_image);
            try
            {
                var result = await _registerLoginInterface.Register(register);
                if (result == 1)
                {
                    return Ok(new { message = "User registered successfully." });
                }
                else
                {
                    return BadRequest(new { message = "User registration failed." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while registering user.", error = ex.Message });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] t_Login login)
        {
            try
            {
                var user = await _registerLoginInterface.Login(login);
                if (user != null)
                {
                    string token = GenerateJwtToken(user);
                    return Ok(new { user, token }); // Return both user and token
                }
                else
                {
                    return NotFound(new { message = "Invalid username or password." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while logging in.", error = ex.Message });
            }
        }

    }
}