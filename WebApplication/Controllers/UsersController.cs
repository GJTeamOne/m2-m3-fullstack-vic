using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication.Core.Common.Exceptions;
using WebApplication.Core.Common.Models;
using WebApplication.Core.Users.Commands;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Core.Users.Models;
using WebApplication.Core.Users.Queries;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserAsync(
            [FromQuery] GetUserQuery query,
            CancellationToken cancellationToken)
        {
            UserDto result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("Find")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindUsersAsync(
                [FromQuery] string? givenNames, [FromQuery] string? lastName, CancellationToken cancellationToken)
        {
            var query = new FindUsersQuery { GivenNames = givenNames, LastName = lastName };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.Any())
            {
                return Ok(result);
            }
            else
            {
                return NotFound("No matching users found.");
            }
        }

        [HttpGet("List")]
        [ProducesResponseType(typeof(PaginatedDto<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListUsersAsync(
    CancellationToken cancellationToken, [FromQuery] int pageNumber = 1, [FromQuery] int itemsPerPage = 10)
        {
            var query = new ListUsersQuery { PageNumber = pageNumber, ItemsPerPage = itemsPerPage };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.Data.Any())
            {
                return Ok(result);
            }
            else
            {
                return NotFound("No users found.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUserAsync(
    [FromBody] CreateUserModel model, CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand
            {
                GivenNames = model.GivenNames,
                LastName = model.LastName,
                EmailAddress = model.EmailAddress,
                MobileNumber = model.MobileNumber
            };

            try
            {
                UserDto newUser = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetUserAsync), new { userId = newUser.UserId }, newUser);
            }
            catch (ValidationException ex)
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation errors occurred."
                };

                foreach (var error in ex.Errors)
                {
                    if (!problemDetails.Errors.ContainsKey(error.PropertyName))
                    {
                        problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
                    }
                    else
                    {
                        var messages = problemDetails.Errors[error.PropertyName];
                        var newMessages = new List<string>(messages) { error.ErrorMessage };
                        problemDetails.Errors[error.PropertyName] = newMessages.ToArray();
                    }
                }


                return BadRequest(problemDetails);
            }
        }


        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUserAsync(
    [FromBody] UpdateUserModel model, CancellationToken cancellationToken)
        {
            var command = new UpdateUserCommand
            {
                Id = model.Id,
                GivenNames = model.GivenNames,
                LastName = model.LastName,
                EmailAddress = model.EmailAddress,
                MobileNumber = model.MobileNumber
            };

            try
            {
                UserDto updatedUser = await _mediator.Send(command, cancellationToken);
                return Ok(updatedUser);
            }
            catch (ValidationException ex)
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Request Validation Error",
                    Detail = "See the errors property for more details.",
                    Instance = HttpContext.Request.Path
                };

                foreach (var error in ex.Errors)
                {
                    if (!problemDetails.Errors.ContainsKey(error.PropertyName))
                    {
                        problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
                    }
                    else
                    {
                        var messages = problemDetails.Errors[error.PropertyName];
                        var newMessages = new List<string>(messages) { error.ErrorMessage };
                        problemDetails.Errors[error.PropertyName] = newMessages.ToArray();
                    }
                }

                return BadRequest(problemDetails);
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUserAsync([FromQuery] int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);

            if (id <= 0)
            {
                _logger.LogWarning("DeleteUserAsync called with invalid ID: {UserId}", id);
                return BadRequest("'Id' must be greater than '0'.");
            }

            var command = new DeleteUserCommand { Id = id };

            try
            {
                UserDto deletedUser = await _mediator.Send(command, cancellationToken);
                _logger.LogInformation("User with ID: {UserId} successfully deleted", id);
                return Ok(deletedUser);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("ValidationException in DeleteUserAsync for ID: {UserId}, Error: {Error}", id, ex.Errors);
                return BadRequest(ex.Errors);
            }
            /*catch (NotFoundException ex)
            {
                _logger.LogWarning("NotFoundException in DeleteUserAsync for ID: {UserId}, Error: {Error}", id, ex.Message);
                return NotFound(ex.Message);
            }*/
        }


    }
}
