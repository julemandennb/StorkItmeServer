using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.FromBody.StorkItme;
using StorkItmeServer.Help;
using StorkItmeServer.Model;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Server.Interface;
using System.Security.Claims;

namespace StorkItmeServer.Controllers
{
    [Route("storkitme")]
    [ApiController]
    public class StorkItmeController : ControllerBase
    {
        private readonly ILogger<StorkItmeController> _logger;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;
        private readonly UserManager<User> _userManager;

        private readonly IStorkItmeServ _storkItmeService;
        private readonly IUserGroupServ _userGroupService;

        public StorkItmeController(
            ILogger<StorkItmeController> logger,
            UserManager<User> userManager,
            IStorkItmeServ storkItmeService,
            IUserGroupServ userGroupService)
        {
            _logger = logger;
            _userManager = userManager;
            _storkItmeService = storkItmeService;
            _userGroupService = userGroupService;

            _roleAuthorizationHandler = new RoleAuthorizationHandler();
        }

        // ------------------------
        // GET SINGLE ITEM
        // ------------------------

        [HttpGet("Get")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> GetAsync(int? id, string? itemNumber, string? ean)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
              
                var role = UserHelp.Role(User);

                StorkItme? item = id != null
                    ? _storkItmeService.Get(id.Value)
                    : itemNumber != null
                        ? _storkItmeService.GetFromItemNumber(itemNumber)
                        : ean != null
                            ? _storkItmeService.GetFromEAN(ean)
                            : null;

                if (item == null)
                    return NotFound();

                bool isManager = _roleAuthorizationHandler.CheckUserRole("Manager", role);
                bool hasAccess = isManager || user.UserGroups.Contains(item.UserGroup);

                if (!hasAccess)
                    return Forbid();

                return Ok(new StorkItmeDTO(item)
                {

                     UserGroup = item.UserGroup != null ? new UserGroupDTO(item.UserGroup) : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAsync");
                return StatusCode(500);
            }
        }

        // ------------------------
        // GET ALL
        // ------------------------

        [HttpGet("GetAll")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> GetAll(bool includeAll = false)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var role = UserHelp.Role(User);

                bool isManager = _roleAuthorizationHandler.CheckUserRole("Manager", role);

                var items = _storkItmeService.GetAll();

                if (!isManager || !includeAll)
                {
                    var groupIds = user.UserGroups.Select(x => x.Id).ToList();
                    items = items
                        .Where(x => x.UserGroupId.HasValue && groupIds.Contains(x.UserGroupId.Value))
                        .ToList();
                }

                var result = items.Select(x => new StorkItmeDTO(x)
                {
                    UserGroup = x.UserGroup != null ? new UserGroupDTO(x.UserGroup) : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll");
                return StatusCode(500);
            }
        }

        // ------------------------
        // CREATE
        // ------------------------

        [HttpPost("Create")]
        [Authorize(Policy = "Member")]
        public IActionResult Create([FromBody] StorkItmeFromBody dto)
        {
            try
            {
                int? userGroupId = null;
                UserGroup? userGroup = null;

                if (dto.UserGroupId.HasValue)
                {
                    userGroup = _userGroupService.Get(dto.UserGroupId.Value);

                    if (userGroup == null)
                        return BadRequest("Invalid user group");

                    userGroupId = userGroup.Id;
                }

                var item = new StorkItme
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Type = dto.Type,
                    BestBy = dto.BestBy,
                    Stork = dto.Stork,
                    UserGroupId = userGroupId,
                    StoreLocation = dto.StoreLocation,
                    ItemNumber = dto.ItemNumber,
                    EAN = dto.EAN
                };

                var created = _storkItmeService.Create(item);

                if (created == null)
                    return StatusCode(500);

                return Ok(new StorkItmeDTO(created)
                {
                    UserGroup = userGroup != null ? new UserGroupDTO(userGroup) : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create");
                return StatusCode(500);
            }
        }

        // ------------------------
        // UPDATE
        // ------------------------

        [HttpPut("Update")]
        [Authorize(Policy = "Member")]
        public IActionResult Update(int id, [FromBody] StorkItmeFromUpdateBody dto)
        {
            try
            {
                var item = _storkItmeService.Get(id);

                if (item == null)
                    return NotFound();

                item.Name = dto.Name ?? item.Name;
                item.Description = dto.Description ?? item.Description;
                item.Type = dto.Type ?? item.Type;
                item.BestBy = dto.BestBy ?? item.BestBy;
                item.Stork = dto.Stork ?? item.Stork;
                item.StoreLocation = dto.StoreLocation ?? item.StoreLocation;
                item.ItemNumber = dto.ItemNumber ?? item.ItemNumber;
                item.EAN = dto.EAN ?? item.EAN;

                if (dto.UserGroupId.HasValue && dto.UserGroupId != item.UserGroupId)
                {
                    var userGroup = _userGroupService.Get(dto.UserGroupId.Value);

                    if (userGroup == null)
                        return BadRequest("Invalid user group");

                    item.UserGroupId = userGroup.Id;
                    item.UserGroup = userGroup;
                }

                bool success = _storkItmeService.Update(item);

                return success ? Ok() : StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update");
                return StatusCode(500);
            }
        }

        // ------------------------
        // DELETE
        // ------------------------

        [HttpDelete("Delete")]
        [Authorize(Policy = "Manager")]
        public IActionResult Delete(int id)
        {
            try
            {
                var item = _storkItmeService.Get(id);

                if (item == null)
                    return NotFound();

                return _storkItmeService.Delete(item)
                    ? Ok()
                    : StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return StatusCode(500);
            }
        }
    }
}