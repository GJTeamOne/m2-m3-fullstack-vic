using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.Core.Common.Models;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Core.Users.Queries;
using WebApplication.Infrastructure.Interfaces;
using AutoMapper;

namespace WebApplication.Core.Users.Handlers
{
    public class ListUsersHandler : IRequestHandler<ListUsersQuery, PaginatedDto<IEnumerable<UserDto>>>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ListUsersHandler(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<PaginatedDto<IEnumerable<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userService.GetPaginatedAsync(request.PageNumber, request.ItemsPerPage, cancellationToken);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            var totalCount = await _userService.CountAsync(cancellationToken);

            return new PaginatedDto<IEnumerable<UserDto>>
            {
                Data = userDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.ItemsPerPage,
                HasNextPage = (request.PageNumber * request.ItemsPerPage) < totalCount
            };
        }
    }
}
