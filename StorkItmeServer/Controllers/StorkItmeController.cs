using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.FromBody.StorkItme;
using StorkItmeServer.Help;
using StorkItmeServer.Model;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Server.Interface;

namespace StorkItmeServer.Controllers
{
    [Route("storkitme")]
    [ApiController]
    public class StorkItmeController : ControllerBase
    {
        private readonly ILogger<StorkItmeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;


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
        public async Task<IActionResult> GetAsync(string? uuid, string? itemNumber, string? ean)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                StorkItme? item = uuid != null
                    ? await _storkItmeService.GetAsync(uuid)
                    : itemNumber != null
                        ? await _storkItmeService.GetFromItemNumberAsync(itemNumber)
                        : ean != null
                            ? await _storkItmeService.GetFromEANAsync(ean)
                            : null;

                if (item == null)
                    return NotFound();

                if (!HasAccess(user, item))
                    return Forbid();

                return Ok(ToDto(item));
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
                if (user == null)
                    return Unauthorized();

                string role = UserHelp.Role(User);

                var isManager = _roleAuthorizationHandler.CheckUserRole("Manager", role);

                var items = await _storkItmeService.GetAllAsync();

                if (!isManager || !includeAll)
                {
                    var groupIds = user.UserGroups.Select(x => x.Id).ToHashSet();

                    items = items
                        .Where(x => x.UserGroupId.HasValue && groupIds.Contains(x.UserGroupId.Value))
                        .ToList();
                }

                return Ok(items.Select((StorkItme x) => ToDto(x)).ToList());
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
        public async Task<IActionResult> Create([FromBody] StorkItmeFromBody dto)
        {
            try
            {
                UserGroup? userGroup = null;

                if (dto.UserGroupId.HasValue)
                {
                    userGroup = _userGroupService.Get(dto.UserGroupId.Value);

                    if (userGroup == null)
                        return BadRequest("Invalid user group");
                }

                var item = new StorkItme
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Type = dto.Type,
                    BestBy = dto.BestBy,
                    Stork = dto.Stork,
                    UserGroupId = userGroup?.Id,
                    StoreLocation = dto.StoreLocation,
                    ItemNumber = dto.ItemNumber,
                    EAN = dto.EAN
                };

                var created = await _storkItmeService.CreateAsync(item);

                if (created == null)
                    return StatusCode(500);

                return Ok(ToDto(created, userGroup));
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
        public async Task<IActionResult> Update(int id, [FromBody] StorkItmeFromUpdateBody dto)
        {
            try
            {
                var item = await _storkItmeService.GetAsync(id);

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

                if (dto.UserGroupId.HasValue)
                {
                    var userGroup = _userGroupService.Get(dto.UserGroupId.Value);

                    if (userGroup == null)
                        return BadRequest("Invalid user group");

                    item.UserGroupId = userGroup.Id;
                    item.UserGroup = userGroup;
                }

                var success = await _storkItmeService.UpdateAsync(item);

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
        public async Task<IActionResult> Delete(string uuid)
        {
            try
            {
                var success = await _storkItmeService.DeleteAsync(uuid);

                return success ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return StatusCode(500);
            }
        }

        // ------------------------
        // HELPERS
        // ------------------------

        private bool HasAccess(User user, StorkItme item)
        {
            var role = UserHelp.Role(User);
            var isManager = _roleAuthorizationHandler.CheckUserRole("Manager", role);

            return isManager || user.UserGroups.Contains(item.UserGroup);
        }

        private StorkItmeDTO ToDto(StorkItme item, UserGroup? group = null)
        {
            var resolvedGroup = group ?? item.UserGroup;

            return new StorkItmeDTO(item)
            {
                UserGroup = resolvedGroup != null
                    ? new UserGroupDTO(resolvedGroup)
                    : null
            };
        }
    }
}