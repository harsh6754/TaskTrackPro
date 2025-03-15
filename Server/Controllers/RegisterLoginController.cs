using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterLoginController : ControllerBase
    {
        private readonly IRegisterLoginInterface _registerLoginInterface;
        public RegisterLoginController(IRegisterLoginInterface registerLoginInterface)
        {
            _registerLoginInterface = registerLoginInterface ?? throw new ArgumentNullException(nameof(registerLoginInterface));
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
        public async Task<IActionResult> Login([FromForm] Repositories.Models.t_Login login)
        {
            try
            {
                var user = await _registerLoginInterface.Login(login);
                if (user != null)
                {
                    return Ok(user);
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