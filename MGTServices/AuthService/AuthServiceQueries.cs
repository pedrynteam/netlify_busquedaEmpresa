using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.EntityFrameworkCore;
using IdentityServer.Data;
using MGTModel.AuthService;

namespace MGTServices.AuthService
{
    [ExtendObjectType(Name = "Query")]
    public class AuthServiceQueries
    {
        /*
        /// <summary>
        /// Gets the currently logged in user.
        /// </summary>
        // [Authorize]
        [UseFirstOrDefault]
        [UseSelection]
        public IQueryable<UserApp> GetMe(
            //[GlobalState]string currentUserEmail,
            [GlobalState]Guid currentPersonId,
            [Service]MVCDbContext dbContext) =>
            dbContext.UserApp.Where(t => t.Id == currentPersonId.ToString());
            */

        /// <summary>
        /// Gets the currently logged in user.
        /// </summary>
        [Authorize]
        public Task<UserApp> GetMeAsync(
            [GlobalState]string currentUserId,
            UserAppByIdDataLoader personById,
            CancellationToken cancellationToken) =>
            personById.LoadAsync(currentUserId, cancellationToken);

        /// <summary>
        /// Gets the currently logged in user.
        /// </summary>
        [Authorize]
        public async Task<UserApp> GetMeByEmailAsync(
            [GlobalState]string currentEmail,
            [Service]ApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            return await dbContext.UserApp.Where(t => t.Email == currentEmail).FirstOrDefaultAsync();
        }


        /// <summary>
        /// Gets access to all the UserApp known to this service.
        /// </summary>
        // [Authorize]
        [UsePaging]
        [UseSelection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<UserApp> GetUserApp(
            [Service]ApplicationDbContext dbContext) =>
            dbContext.UserApp;

        /// <summary>
        /// Gets a user by its email address.
        /// </summary>
        // [Authorize]
        [UseFirstOrDefault]
        [UseSelection]
        public IQueryable<UserApp> GetUserAppByEmailAsync(
            string email,
            [Service]ApplicationDbContext dbContext) =>
            dbContext.UserApp.Where(t => t.Email == email);

        /// <summary>
        /// Gets a user by its id.
        /// </summary>
        // [Authorize]
        [UseFirstOrDefault]
        [UseSelection]
        public IQueryable<UserApp> GetUserAppByIdAsync(
            Guid token,
            [Service]ApplicationDbContext dbContext) =>
            dbContext.UserApp.Where(t => t.Id.Equals(token));
    }
}
