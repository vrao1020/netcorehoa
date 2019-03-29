using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HoaCommon.Services;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.OutputModels;
using HoaIdentityUsers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HoaIdentityUsers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserClaimsController : ControllerBase
    {
        private UserManager<ApplicationUser> userManager;
        private readonly IPaginationGenerator paginationGenerator;

        public UserClaimsController(UserManager<ApplicationUser> userManager, IPaginationGenerator paginationGenerator)
        {
            this.userManager = userManager;
            this.paginationGenerator = paginationGenerator;
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(string id)
        {
            //get all users
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userClaims = await userManager.GetClaimsAsync(user);

            //map user to view dto
            var userToReturn = new IDPUserViewDto(user.Id,
                                                   userClaims.Where(x => x.Type == "role")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault(),
                                                   userClaims.Where(x => x.Type == "readonly")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault(),
                                                   userClaims.Where(x => x.Type == "postcreation")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault(),
                                                   user.Email,
                                                   user.EmailConfirmed ? "True" : "False",
                                                   userClaims.Where(x => x.Type == "name")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault());

            return Ok(userToReturn);
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers(int pageNum = 1, int pageSize = 1)
        {
            //get all users
            var users = userManager.Users;

            //apply pagination
            var paginatedUsers = users.Skip((pageNum - 1) * pageSize)
                                      .Take(pageSize);

            //map users to a list of output dto
            var customUsers = new List<IDPUserViewDto>();

            foreach (var item in paginatedUsers)
            {
                var userClaims = await userManager.GetClaimsAsync(item);

                customUsers.Add(new IDPUserViewDto(item.Id,
                                                   userClaims.Where(x => x.Type == "role")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault(),
                                                   userClaims.Where(x => x.Type == "readonly")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault(),
                                                   userClaims.Where(x => x.Type == "postcreation")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault(),
                                                   item.Email,
                                                   item.EmailConfirmed ? "True" : "False",
                                                   userClaims.Where(x => x.Type == "name")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault()));
            }

            //apply pagination headers
            paginationGenerator.GenerateHeaders(users.Count(), pageSize, pageNum, "", "");

            return Ok(customUsers);
        }

        [HttpGet("GetBoardEmails")]
        public async Task<IActionResult> GetBoardEmails()
        {
            //get all users
            var users = await userManager.GetUsersForClaimAsync(new Claim("role", "board"));
            var admin = await userManager.GetUsersForClaimAsync(new Claim("role", "admin"));

            //map users to a list of output dto
            var emails = new List<string>();

            //add board emails
            foreach (var item in users)
            {
                emails.Add(item.Email);
            }

            //add the admin email
            emails.Add(admin?.FirstOrDefault().Email);

            return Ok(emails);
        }

        [HttpGet("GetAdminEmail")]
        public async Task<IActionResult> GetAdminEmail()
        {
            //get admin user            
            var admin = await userManager.GetUsersForClaimAsync(new Claim("role", "admin"));

            if (admin.Count == 0)
            {
                return NotFound();
            }

            var email = admin.FirstOrDefault().Email;

            return Ok(admin?.FirstOrDefault().Email);
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(IDPUserManipulationDto userUpdateDto)
        {
            var userToUpdate = await userManager.FindByIdAsync(userUpdateDto.Id);
            var userClaims = await userManager.GetClaimsAsync(userToUpdate);

            if (userToUpdate == null)
            {
                return NotFound();
            }

            //remove all claims          
            await userManager.RemoveClaimsAsync(userToUpdate,
                new List<Claim>() { new Claim("role", userClaims.Where(x => x.Type == "role")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault()),
                                    new Claim("readonly", userClaims.Where(x => x.Type == "readonly")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault()),
                                    new Claim("postcreation", userClaims.Where(x => x.Type == "postcreation")
                                                          .Select(x => x.Value)
                                                          .FirstOrDefault())});

            //add them back with updated values
            await userManager.AddClaimsAsync(userToUpdate,
                new List<Claim>() { new Claim("role", userUpdateDto.Role),
                                    new Claim("readonly", userUpdateDto.ReadOnly),
                                    new Claim("postcreation", userUpdateDto.PostCreation)});

            return NoContent();
        }

    }
}