using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.Database;
using StorkItmeServer.FromBody.UserGroup;
using StorkItmeServer.Help;
using StorkItmeServer.Model;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Server.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace StorkItmeServer.Controllers
{
    [Route("usergroup"), Authorize]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        private readonly ILogger<UserGroupController> _logger;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;

        private readonly IUserGroupServ _userGroupServ;
        private readonly IStorkItmeServ _storkItmeServ;
        private readonly IUserServ _userServ;


        public UserGroupController(ILogger<UserGroupController> logger,IUserGroupServ userGroupServ, IStorkItmeServ storkItmeServ, IUserServ userServ)
        {
            _logger = logger;
            _roleAuthorizationHandler = new RoleAuthorizationHandler();
            _userGroupServ = userGroupServ;
            _storkItmeServ = storkItmeServ;
            _userServ = userServ;
        }

        [HttpGet("GetAll")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> GetAll(bool ShowAllGroup = false)
        {
            try
            {

                var user = await _userServ.GetByClaimsPrincipal(User);
                var userRoles =  UserHelp.Role(User);
                List<UserGroupDTO> userGroups = new List<UserGroupDTO>();

                if (_roleAuthorizationHandler.CheckUserRole("Manager", userRoles) && ShowAllGroup)
                {
                    // Retrieve the list of user groups from the database
                    userGroups = await _userGroupServ.GetAll().Select(gp => new UserGroupDTO(gp)
                    {
                        StorkItmes = gp.StorkItmes.Select(s => new StorkItmeDTO(s)).ToList(),
                        Users = _roleAuthorizationHandler.CheckUserRole("Manager", userRoles) ? gp.Users.Select(u => new UserDTO(u)).ToList() : null,

                    }).ToListAsync();
                }
                else
                {
                    userGroups = user.UserGroups.Select(g => new UserGroupDTO(g)
                    {
                        StorkItmes = g.StorkItmes.Select(s => new StorkItmeDTO(s)).ToList(),
                        Users = _roleAuthorizationHandler.CheckUserRole("Manager", userRoles) ? g.Users.Select(u => new UserDTO(u)).ToList() : null,
                    }).ToList();
                }
  
                return Ok(userGroups); // Return as JSON response
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

                UserGroup userGroup = _userGroupServ.Get(uuid);

                bool roleCheck = _roleAuthorizationHandler.CheckUserRole("Manager", userRoles);

                if (userGroup != null && (roleCheck || user.UserGroups.Contains(userGroup)))
                {

                    UserGroupDTO userGroupDTO = new UserGroupDTO(userGroup);

                    userGroupDTO.StorkItmes = userGroup.StorkItmes.Select(s => new StorkItmeDTO(s)).ToList();

                    if (roleCheck)
                    {
                        userGroupDTO.Users = userGroup.Users.Select(u => new UserDTO(u)).ToList();
                    }

                    return Ok(userGroupDTO);
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
        public IActionResult Create([FromBody] UserGroupFromBody userGroupFromBody)
        {
            try
            {
                UserGroup userGroup = new UserGroup();
                userGroup.Name = userGroupFromBody.Name;
                userGroup.Color = userGroupFromBody.Color;

                userGroup = _userGroupServ.Create(userGroup);
                
                return Ok(new UserGroupDTO(userGroup));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPut("Updata")]
        [Authorize(Policy = "Manager")]
        public IActionResult Updata([FromBody] UserGroupFromBody userGroupFromBody,int id)
        {
            try { 

                UserGroup userGroup = _userGroupServ.Get(id);

                if (userGroup is not null)
                {
                    
                    userGroup.Name = userGroupFromBody.Name;
                    userGroup.Color=userGroupFromBody.Color;

                   if(_userGroupServ.Updata(userGroup))
                        return Ok(new UserGroupDTO(userGroup));
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
        public async Task<IActionResult> AddUser([FromBody] UserGroupIdUserIdFromBody fromBody)
        {
            try
            {
                UserGroup userGroup = _userGroupServ.Get(fromBody.UserGroupId);

                User user = await _userServ.Get(fromBody.UserId);

                if (user != null && userGroup != null)
                {
                    userGroup.Users.Add(user);

                    if (_userGroupServ.Updata(userGroup))
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
                UserGroup userGroup = _userGroupServ.Get(uuid);

                if (userGroup is not null)
                {
                    
                    _userGroupServ.Delete(userGroup);

                    return Ok();
                }
                else
                {
                    return StatusCode(500, "No UserGroup find");
                }


                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }


        }

        [HttpDelete("RemoveUser")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> RemoveUser([FromBody] UserGroupIdUserIdFromBody fromBody)
        {
            try
            {
                UserGroup userGroup = _userGroupServ.Get(fromBody.UserGroupId);

                User user = await _userServ.Get(fromBody.UserId);


                if (user != null && userGroup != null)
                {

                    userGroup.Users.Remove(user);
                    if(_userGroupServ.Updata(userGroup))
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
