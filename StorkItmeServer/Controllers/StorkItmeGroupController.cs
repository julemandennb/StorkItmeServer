using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.Database;
using StorkItmeServer.FromBody.StorkItmeGroup;
using StorkItmeServer.FromBody.UserGroup;
using StorkItmeServer.Help;
using StorkItmeServer.Help;
using StorkItmeServer.Model;
using StorkItmeServer.Model;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Server.Interface;
using StorkItmeServer.Server.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace StorkItmeServer.Controllers
{
    [Route("storkitmegroup"), Authorize]
    [ApiController]
    public class StorkItmeGroupController : ControllerBase
    {
        private readonly ILogger<StorkItmeGroupController> _logger;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;

        private readonly IGroupServ<StorkItmeGroup> _StorkItmeGroupServ;
        private readonly IStorkItmeServ _storkItmeServ;
        private readonly IUserServ _userServ;


        public StorkItmeGroupController(ILogger<StorkItmeGroupController> logger, IGroupServ<StorkItmeGroup> StorkItmeGroupServ, IStorkItmeServ storkItmeServ, IUserServ userServ)
        {
            _logger = logger;
            _roleAuthorizationHandler = new RoleAuthorizationHandler();
            _StorkItmeGroupServ = StorkItmeGroupServ;
            _storkItmeServ = storkItmeServ;
            _userServ = userServ;
        }

        [HttpGet("GetAll")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> GetAll(bool showAllGroup = false,bool includeStorkItmes = false, bool includeUsers = false)
        {
            try
            {
                var user = await _userServ.GetByClaimsPrincipal(User);
                var userRoles = UserHelp.Role(User);

                bool isManager = _roleAuthorizationHandler
                    .CheckUserRole("Manager", userRoles);

                if (!isManager)
                {
                    showAllGroup = false;
                    includeUsers = false;
                }

                List<StorkItmeGroup> groups = await _StorkItmeGroupServ.GetAll(
                    userId: user.Id,
                    GetAll: showAllGroup,
                    includeStorkItmes: includeStorkItmes,
                    includeUsers: includeUsers
                );

                var result = groups.Select(g => new StorkItmeGroupDto(g)
                {
                    StorkItmes = includeStorkItmes ? g.StorkItmes.Select(s => new StorkItmeDTO(s)).ToList(): null,
                    Users = includeUsers ? g.Users.Select(u => new UserDTO(u)).ToList() : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Get")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> Get(string uuid)
        {
            try
            {
                var user = await _userServ.GetByClaimsPrincipal(User);
                var userRoles = UserHelp.Role(User);

                StorkItmeGroup storkItmeGroup = _StorkItmeGroupServ.Get(uuid);

                bool roleCheck = _roleAuthorizationHandler.CheckUserRole("Manager", userRoles);

                if (storkItmeGroup != null && (roleCheck || user.StorkItmeGroups.Contains(storkItmeGroup)))
                {

                    StorkItmeGroupDto storkItmeGroupDTO = new StorkItmeGroupDto(storkItmeGroup);

                    storkItmeGroupDTO.StorkItmes = storkItmeGroup.StorkItmes.Select(s => new StorkItmeDTO(s)).ToList();

                    if (roleCheck)
                    {
                        storkItmeGroupDTO.Users = storkItmeGroup.Users.Select(u => new UserDTO(u)).ToList();
                    }

                    return Ok(storkItmeGroupDTO);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }



        }


        [HttpPost("Create")]
        [Authorize(Policy = "Manager")]
        public IActionResult Create([FromBody] StorkItmeGroupFromBody StorkItmeGroupFromBody)
        {
            try
            {
                StorkItmeGroup storkItmeGroup = new StorkItmeGroup(){
                   
                    Name = StorkItmeGroupFromBody.Name,
                    Description = StorkItmeGroupFromBody.Description
                };
               

                storkItmeGroup = _StorkItmeGroupServ.Create(storkItmeGroup);

                return Ok(new StorkItmeGroupDto(storkItmeGroup));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPut("Updata")]
        [Authorize(Policy = "Manager")]
        public IActionResult Updata([FromBody] StorkItmeGroupFromBody storkItmeGroupFromBody, int id)
        {
            try
            {

                StorkItmeGroup storkItmeGroup = _StorkItmeGroupServ.Get(id);

                if (storkItmeGroup is not null)
                {

                    storkItmeGroup.Name = storkItmeGroupFromBody.Name;
                    storkItmeGroup.Description = storkItmeGroupFromBody.Description;

                    if (_StorkItmeGroupServ.Update(storkItmeGroup))
                        return Ok(new StorkItmeGroupDto(storkItmeGroup));
                    else
                        return BadRequest();
                }


                return BadRequest();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPut("AddUser")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> AddUser([FromBody] StorkItmeGroupIdUserIdFromBody fromBody)
        {
            try
            {
                StorkItmeGroup storkItmeGroup = _StorkItmeGroupServ.Get(fromBody.StorkItmeGroupId);

                List<User> users = await _userServ.Getall(fromBody.UserId);

                if (users.Count > 0 && storkItmeGroup != null)
                {
                    foreach (var user in users)
                    {
                        if (!storkItmeGroup.Users.Any(u => u.Id == user.Id))
                        {
                            storkItmeGroup.Users.Add(user);
                        }
                    }

                    if (_StorkItmeGroupServ.Update(storkItmeGroup))
                        return Ok();
                    else
                        return BadRequest();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }
        }

        //this is not test 
        [HttpDelete("Delete")]
        [Authorize(Policy = "Manager")]
        public IActionResult Delete(string uuid)
        {
            try
            {
                StorkItmeGroup storkItmeGroup = _StorkItmeGroupServ.Get(uuid);

                if (storkItmeGroup is not null)
                {

                    _StorkItmeGroupServ.Delete(storkItmeGroup);

                    return Ok();
                }
                else
                {
                    return StatusCode(500, "No storkItmeGroup find");
                }



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }


        }

        [HttpDelete("RemoveUser")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> RemoveUser([FromBody] StorkItmeGroupIdUserIdFromBody fromBody)
        {
            try
            {
                StorkItmeGroup storkItmeGroup = _StorkItmeGroupServ.Get(fromBody.StorkItmeGroupId);

                List<User> users = await _userServ.Getall(fromBody.UserId);


                if (users.Count > 0 && storkItmeGroup != null)
                {

                    foreach (var user in users)
                    {
                        if (storkItmeGroup.Users.Any(u => u.Id == user.Id))
                        {
                            storkItmeGroup.Users.Remove(user);
                        }
                    }


                    if (_StorkItmeGroupServ.Update(storkItmeGroup))
                        return Ok();

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
