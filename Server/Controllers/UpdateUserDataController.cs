using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpdateUserDataController : ControllerBase
    {
        private readonly IRegisterLoginInterface _registerLoginInterface;
        public UpdateUserDataController(IRegisterLoginInterface registerLoginInterface)
        {
            _registerLoginInterface = registerLoginInterface ?? throw new ArgumentNullException(nameof(registerLoginInterface));
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromForm] t_ChangePassword changePassword)
        {
            try
            {
                var result = await _registerLoginInterface.ChangePassword(changePassword);
                if (result == 0)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new { message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user.", error = ex.Message });
            }
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] t_UserUpdateProfile userUpdateProfile)
        {
            try
            {
                if (string.IsNullOrEmpty(userUpdateProfile.c_email) || userUpdateProfile.c_userId == 0)
                {
                    return BadRequest(new { message = "UserID and Email are required" });
                }
                // Ensure user profile is updated first
                t_UserUpdateProfile result = await _registerLoginInterface.UpdateProfile(userUpdateProfile);

                // Process image only if a new file is uploaded
                if (userUpdateProfile.ImageFile != null && userUpdateProfile.ImageFile.Length > 0)
                {
                    var fileName = userUpdateProfile.c_email + Path.GetExtension(userUpdateProfile.ImageFile.FileName);
                    var filePath = Path.Combine("../Client/wwwroot/Images", fileName);
                      // Delete the existing file if it exists
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
                   using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await userUpdateProfile.ImageFile.CopyToAsync(stream);
            }
                    result.newImage = fileName;
                }

                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating profile.", error = ex.Message });
            }
        }
    }
}