using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Core.Users.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public int Id { get; set; }
        public string GivenNames { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;

        public class Validator : AbstractValidator<UpdateUserCommand>
        {
            private readonly IUserService _userService;

            public Validator(IUserService userService)
            {
                _userService = userService;

                RuleFor(x => x.Id)
                    .GreaterThan(0)
                    .MustAsync(UserExists).WithMessage("User does not exist.");

                RuleFor(x => x.GivenNames)
                    .NotEmpty();

                RuleFor(x => x.LastName)
                    .NotEmpty();

                RuleFor(x => x.EmailAddress)
                    .NotEmpty()
                    .EmailAddress();

                RuleFor(x => x.MobileNumber)
                    .NotEmpty();
            }

            private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
            {
                var user = await _userService.GetAsync(userId, cancellationToken);
                return user != null;
            }
        }
    }
}
