using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Interfaces;
using AutoMapper;
using WebApplication.Core.Common.Exceptions;
using WebApplication.Core.Users.Commands;
using WebApplication.Infrastructure.Entities;

namespace WebApplication.Core.Users.Handlers
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UpdateUserHandler(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userService.GetAsync(request.Id, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException($"User with ID {request.Id} not found.");
            }

            // Update user properties
            user.GivenNames = request.GivenNames;
            user.LastName = request.LastName;

            // Ensure ContactDetail is not null before assigning
            if (user.ContactDetail == null)
            {
                user.ContactDetail = new ContactDetail();
            }

            user.ContactDetail.EmailAddress = request.EmailAddress;
            user.ContactDetail.MobileNumber = request.MobileNumber;

            // Update the user in the database
            await _userService.UpdateAsync(user, cancellationToken);

            // Map and return the updated user
            return _mapper.Map<UserDto>(user);
        }
    }
}
