using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using IdentityServer.Data;
using MGTModel.AuthService;
using System.Threading.Tasks;

namespace MGTServices.AuthService
{
    [ExtendObjectType(Name = "Mutation")]
    public class AuthServiceMutations
    {
        //[Authorize(Policy = "IsNewUser")]
        [GraphQLDescription("Register a User with e-mail and password")]
        public async Task<RegisterUserTxnPayload> RegisterUser(
            RegisterUserTxnInput input,
            [Service] UserManager<IdentityUser> userManager,
            [Service] ApplicationDbContext contextMVC)
        {
            return await new RegisterUserTxn().Execute(
                input: input,
                userManager: userManager,
                contextMVC: contextMVC,
                autoCommit: true
                );
        }

        // [Authorize(Roles = new string[] {"Admin","def"})]
        [GraphQLDescription("Login a User with e-mail and password and get a JWT")]
        public async Task<LoginUserTxnPayload> LoginUser(
            LoginUserTxnInput input,
            [Service] IConfiguration configuration,
            [Service] UserManager<IdentityUser> userManager,
            [Service] SignInManager<IdentityUser> signInManager,
            [Service] ApplicationDbContext contextMVC)
        {
            return await new LoginUserTxn().Execute(
                input: input,
                configuration: configuration,
                userManager: userManager,
                signInManager: signInManager,
                contextMVC: contextMVC,
                autoCommit: true
                );
        }

        // AccessTokenLogin
        [GraphQLDescription("Use a access token to get a JWT")]
        public async Task<LoginUserTxnAccessTokenPayload> LoginUserAccessToken(
        LoginUserTxnAccessTokenInput input,
        [Service] IConfiguration configuration,
        [Service] UserManager<IdentityUser> userManager,
        [Service] SignInManager<IdentityUser> signInManager,
        [Service] ApplicationDbContext contextMVC)
        {
            return await new LoginUserTxn().LoginUserAccessToken(
                input: input,
                configuration: configuration,
                userManager: userManager,
                signInManager: signInManager,
                contextMVC: contextMVC,
                autoCommit: true
                );
        }
    }
}
