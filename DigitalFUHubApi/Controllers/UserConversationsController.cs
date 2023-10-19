using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Managers;
using DigitalFUHubApi.Services;
using DTOs.UserConversation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserConversationsController : ControllerBase
    {
        private readonly IUserConversationRepository _repository;

        public UserConversationsController(IUserConversationRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult Update ([FromBody] UpdateUserConversationRequestDTO request)
        {
            try
            {
                if (!request.IsValid())
                {
                    return BadRequest(new Status());
                }

                (bool, string) result = _repository.Update(request);

                return Ok(new Status { ResponseCode = result.Item2 });

            } catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return BadRequest(new Status());
            }
        }
    }
}
