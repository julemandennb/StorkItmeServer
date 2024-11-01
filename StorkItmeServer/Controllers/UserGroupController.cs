﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Database;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Handler;
using StorkItmeServer.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using StorkItmeServer.FromBody.UserGroup;

namespace StorkItmeServer.Controllers
{
    [Route("usergroup"), Authorize]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        private readonly ILogger<UserGroupController> _logger;
        private readonly DataContext _context;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;
        private readonly UserManager<User> _userManager;

        public UserGroupController(ILogger<UserGroupController> logger, DataContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _roleAuthorizationHandler = new RoleAuthorizationHandler();
            _userManager = userManager;
        }

        [HttpGet("GetAll")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> GetAll(bool ShowAllGroup = false)
        {
            try
            {

                var user = await _userManager.GetUserAsync(User);
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();
                List<UserGroupDTO> userGroups = new List<UserGroupDTO>();

                if (_roleAuthorizationHandler.CheckUserRole("Manager", userRoles) && ShowAllGroup)
                {
                    // Retrieve the list of user groups from the database
                    userGroups = await _context.UserGroup.Select(gp => new UserGroupDTO(gp)
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
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();

                UserGroup userGroup = _context.UserGroup.FirstOrDefault(x => x.Id == id);

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

                _context.UserGroup.Add(userGroup);
                _context.SaveChanges();
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

                UserGroup userGroup = _context.UserGroup.FirstOrDefault(u => u.Id == id);

                if (userGroup is not null)
                {
                    
                    userGroup.Name = userGroupFromBody.Name;
                    userGroup.Color=userGroupFromBody.Color;

                    _context.SaveChanges();

                    return Ok(new UserGroupDTO(userGroup));
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
        public IActionResult AddUser([FromBody] UserGroupIdUserIdFromBody fromBody)
        {
            try
            {
                UserGroup userGroup = _context.UserGroup.FirstOrDefault(x => x.Id == fromBody.UserGroupId);

                User user = _context.Users.FirstOrDefault(x => x.Id == fromBody.UserId);

                if (user != null && userGroup != null)
                {
                    userGroup.Users.Add(user);

                    _context.UserGroup.Update(userGroup);
                    _context.SaveChanges();



                    return Ok();
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
        public IActionResult Delete(int id)
        {
            try
            {
                UserGroup userGroup = _context.UserGroup.FirstOrDefault(x => x.Id == id);

                if(userGroup is not null)
                {

                    userGroup.Users.Clear();

                    ICollection<StorkItme> storkItmes = userGroup.StorkItmes;

                    _context.StorkItme.RemoveRange(storkItmes);

                    userGroup.StorkItmes.Clear();

                    _context.UserGroup.Remove(userGroup);

                    _context.SaveChanges();

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
        public IActionResult RemoveUser([FromBody] UserGroupIdUserIdFromBody fromBody)
        {
            try
            {
                UserGroup userGroup = _context.UserGroup.FirstOrDefault(x => x.Id == fromBody.UserGroupId);

                User user = _context.Users.FirstOrDefault(x => x.Id == fromBody.UserId);


                if (user != null && userGroup != null)
                {

                    userGroup.Users.Remove(user);
                    _context.SaveChanges();


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
