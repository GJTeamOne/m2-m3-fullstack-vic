using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication.Infrastructure.Contexts;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly InMemoryContext _dbContext;

        public UserService(InMemoryContext dbContext)
        {
            _dbContext = dbContext;

            // this is a hack to seed data into the in memory database. Do not use this in production.
            _dbContext.Database.EnsureCreated();
        }

        /// <inheritdoc />
        public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            User? user = await _dbContext.Users.Where(user => user.Id == id)
                                         .Include(x => x.ContactDetail)
                                         .FirstOrDefaultAsync(cancellationToken);

            return user;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> FindAsync(string? givenNames, string? lastName, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(givenNames))
            {
                query = query.Where(u => u.GivenNames.Contains(givenNames));
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(u => u.LastName == lastName);
            }

            return await query.Include(u => u.ContactDetail).ToListAsync(cancellationToken);
        }


        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetPaginatedAsync(int page, int count, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.Include(u => u.ContactDetail)
                                         .OrderBy(u => u.Id)
                                         .Skip((page - 1) * count)
                                         .Take(count)
                                         .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }


        /// <inheritdoc />
        public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var existingUser = await _dbContext.Users
                                               .Include(u => u.ContactDetail)
                                               .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found.");
            }

            _dbContext.Entry(existingUser).CurrentValues.SetValues(user);
            if (user.ContactDetail != null)
            {
                _dbContext.Entry(existingUser.ContactDetail).CurrentValues.SetValues(user.ContactDetail);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return existingUser;
        }

        /// <inheritdoc />
        public async Task<User?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken);

            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return user;
        }


        /// <inheritdoc />
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.CountAsync(cancellationToken);
        }

    }
}
