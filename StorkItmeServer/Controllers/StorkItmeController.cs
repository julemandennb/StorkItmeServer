using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Database;
using StorkItmeServer.FromBody.StorkItme;
using StorkItmeServer.Handler;
using StorkItmeServer.Model;
using StorkItmeServer.Model.DTO;
using System.Linq;
using System.Security.Claims;
using System.Xml.Linq;

namespace StorkItmeServer.Controllers
{
    [Route("storkitme")]
    [ApiController]
    public class StorkItmeController : ControllerBase
    {
        private readonly ILogger<UserGroupController> _logger;
        private readonly DataContext _context;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;
        private readonly UserManager<User> _userManager;

        public StorkItmeController(ILogger<UserGroupController> logger, DataContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _roleAuthorizationHandler = new RoleAuthorizationHandler();
            _userManager = userManager;
        }

        [HttpGet("Get")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();

                StorkItme storkItme = _context.StorkItme.FirstOrDefault(x => x.Id == id);

                if (storkItme is not null)
                {

                    bool roleCheck = _roleAuthorizationHandler.CheckUserRole("Manager", userRoles);

                    bool HavRightGroups = user.UserGroups.Contains(storkItme.UserGroup);

                    if (roleCheck || roleCheck)
                    {

                        StorkItmeDTO storkItmeDTO = new StorkItmeDTO(storkItme)
                        {
                            UserGroup = new UserGroupDTO(storkItme.UserGroup)
                        };

                        return Ok(storkItmeDTO);
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }


        }

        [HttpGet("Getall")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> GetAll(bool GetAllStorkItme = false)
        {
            try
            {

                var user = await _userManager.GetUserAsync(User);
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();

                bool roleCheck = _roleAuthorizationHandler.CheckUserRole("Manager", userRoles);

                List<StorkItmeDTO> storkItmeDTOs = new List<StorkItmeDTO>();

                if(roleCheck && GetAllStorkItme)
                    storkItmeDTOs = await _context.StorkItme.Select(x => new StorkItmeDTO(x) {UserGroup = new UserGroupDTO(x.UserGroup) }).ToListAsync();
                else
                {
                    storkItmeDTOs = await _context.StorkItme.Where(g => user.UserGroups.Contains(g.UserGroup)).Select(b => new StorkItmeDTO(b) { UserGroup = new UserGroupDTO(b.UserGroup) } ).ToListAsync();
                }

                return Ok(storkItmeDTOs);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Create")]
        [Authorize(Policy = "Member")]
        public IActionResult Create([FromBody] StorkItmeFromBody storkItmeFromBody)
        {
            try
            {
                // Check for valid UserGroup early
                var userGroup = _context.UserGroup.FirstOrDefault(u => u.Id == storkItmeFromBody.UserGroupId);

                if (userGroup is null)
                {
                    // Log and return BadRequest with a message to explain the issue
                    _logger.LogWarning("UserGroup with ID {UserGroupId} not found.", storkItmeFromBody.UserGroupId);
                    return BadRequest("Invalid UserGroup ID provided.");
                }


                // Create StorkItme if UserGroup is valid
                var storkItme = new StorkItme()
                {
                    Name = storkItmeFromBody.Name,
                    Description = storkItmeFromBody.Description,
                    Type = storkItmeFromBody.Type,
                    BestBy = storkItmeFromBody.BestBy,
                    Stork = storkItmeFromBody.Stork,
                    ImgUrl = storkItmeFromBody.ImgUrl,
                    UserGroupId = userGroup.Id,
                };

                _context.StorkItme.Add(storkItme);
                _context.SaveChanges();

                // Log successful creation
                _logger.LogInformation("StorkItme with ID {StorkItmeId} created successfully.", storkItme.Id);

                // Return a successful response with DTO
                return Ok(new StorkItmeDTO(storkItme) { UserGroup = new UserGroupDTO(userGroup) });
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while creating the StorkItme.");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("Updata")]
        [Authorize(Policy = "Member")]
        public IActionResult Updata(int id , [FromBody] StorkItmeFromBody storkItmeFromBody)
        {

            try
            {

                StorkItme storkItme = _context.StorkItme.FirstOrDefault(st => st.Id == id);

                if(storkItme is not null)
                {
                    bool error = false;

                    storkItme.Name = storkItmeFromBody.Name;
                    storkItme.Description = storkItmeFromBody.Description;
                    storkItme.Type = storkItmeFromBody.Type;
                    storkItme.BestBy = storkItmeFromBody.BestBy;
                    storkItme.Stork = storkItmeFromBody.Stork;

                    storkItme.ImgUrl = storkItmeFromBody.ImgUrl;


                    if (storkItme.UserGroupId != storkItmeFromBody.UserGroupId)
                    {
                        UserGroup userGroup = _context.UserGroup.FirstOrDefault(g => g.Id == storkItmeFromBody.UserGroupId);

                        if (userGroup is not null)
                        {
                            storkItme.UserGroupId = userGroup.Id;
                            storkItme.UserGroup = userGroup;
                        }
                        else
                            error = true;
                    }

                    if (!error)
                    {
                        _context.SaveChanges();
                        return Ok();
                    }
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpDelete("Delete")]
        [Authorize(Policy = "Manager")]
        public IActionResult Delete(int id)
        {
            try
            {
                StorkItme storkItme = _context.StorkItme.FirstOrDefault(g => g.Id == id);

                if(storkItme is not null)
                {
                    _context.StorkItme.Remove(storkItme);

                    _context.SaveChanges();

                    return Ok();
                }

                return StatusCode(500, "No storkItme find");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
