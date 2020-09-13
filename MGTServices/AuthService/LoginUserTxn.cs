using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using IdentityServer.Data;
using MGTModel.AuthService;
using MGTModel.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MGTServices.AuthService
{
    public class LoginUserTxn
    {
        public LoginUserTxn()
        {
        }

        public async Task<LoginUserTxnPayload> Execute(
            LoginUserTxnInput input,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext contextMVC,
            bool autoCommit)
        {
            LoginUserTxnPayload _payload = new LoginUserTxnPayload();

            //***** Declare variables and messages
            string errorMessage = "LoginUserTxn***LOGIN_FAIL";
            string okMessage = "LoginUserTxn***LOGIN_PASS";
            string errorMessageExceptionCode = "LoginUserTxn***TXN_EXCEPTION";
            string errorMessageException = "TXN_EXCEPTION";

            // Error handling
            bool error = false; // To Handle Only One Error

            try
            {
                // DBContext by convention is a UnitOfWork, track changes and commits when SaveChanges is called
                // Multithreading issue: so if we use only one DBContext, it will track all entities (_context.Customer.Add) and commit them when SaveChanges is called, 
                // No Matter what transactions or client calls SaveChanges.
                // Note: Rollback will not remove the entities from the tracker in the context. so better dispose it.

                //***** Make The Validations - Be careful : Concurrency. Same name can be saved multiple times if called at the exact same time. Better have an alternate database constraint
                if (!error)
                {
                    // To check later - RefreshToken
                    // https://stackoverflow.com/questions/53659247/using-aspnetusertokens-table-to-store-refresh-token-in-asp-net-core-web-api

                    var result = await signInManager.PasswordSignInAsync(userName: input.Email, password: input.Password, isPersistent: false, lockoutOnFailure: false);

                    if (!result.Succeeded)
                    {
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem>() { new MessageItem { Code = errorMessage, Message = errorMessage, Detail = "" } });
                        return _payload;
                    }

                    var user = await signInManager.UserManager.FindByEmailAsync(input.Email);
                    if (user == null)
                    {
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem>() { new MessageItem { Code = errorMessage, Message = errorMessage, Detail = "" } });
                        return _payload;
                    }

                    // Get the UserApp
                    var userApp = await contextMVC.UserApp.Where(x => x.IdentityId == user.Id).FirstOrDefaultAsync();
                    if (userApp == null)
                    {
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem>() { new MessageItem { Code = errorMessage, Message = errorMessage, Detail = "" } });
                        return _payload;
                    }

                    //***** Save and Commit to the Database (Atomic because is same Context DB) 
                    if (!error && autoCommit)
                    {
                        await contextMVC.SaveChangesAsync(); // Call it only once so do all other operations first
                    }

                    //***** Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                    _payload.PayloadResult = PayloadResult.ResultGood(Messages: new List<MessageItem> { new MessageItem { Code = okMessage, Message = okMessage, Detail = "" } });
                    _payload.JWTInfo = await this.CreateTokenAsync(user, userApp, configuration, userManager);

                }// if (!error)                
            }
            catch (Exception ex) // Main try 
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                string innerError = (ex.InnerException != null) ? ex.InnerException.Message : "";
                System.Diagnostics.Debug.WriteLine("Error Inner: " + innerError);
                _payload = new LoginUserTxnPayload
                {
                    PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem> { new MessageItem { Code = errorMessageExceptionCode, Message = errorMessageException, Detail = ex.Message } })
                }; // Restart variable to avoid returning any already saved data                
            }
            finally
            {
                // Save Logs if needed
            }

            return _payload;
        }

        public async Task<LoginUserTxnAccessTokenPayload> LoginUserAccessToken(
            LoginUserTxnAccessTokenInput input,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext contextMVC,
            bool autoCommit)
        {
            LoginUserTxnAccessTokenPayload _payload = new LoginUserTxnAccessTokenPayload();

            //***** Declare variables and messages
            string errorMessageUserNotFound = "LoginUserAccessTokenTxn***LOGIN_FAIL";
            string errorMessageInvalidAccessToken = "LoginUserAccessTokenTxn***INVALID_ACCESS_TOKEN";
            string okMessage = "LoginUserAccessTokenTxn***LOGIN_PASS";
            string errorMessageExceptionCode = "LoginUserAccessTokenTxn***TXN_EXCEPTION";
            string errorMessageException = "TXN_EXCEPTION";

            // Error handling
            bool error = false; // To Handle Only One Error

            try
            {
                // DBContext by convention is a UnitOfWork, track changes and commits when SaveChanges is called
                // Multithreading issue: so if we use only one DBContext, it will track all entities (_context.Customer.Add) and commit them when SaveChanges is called, 
                // No Matter what transactions or client calls SaveChanges.
                // Note: Rollback will not remove the entities from the tracker in the context. so better dispose it.

                //***** Make The Validations - Be careful : Concurrency. Same name can be saved multiple times if called at the exact same time. Better have an alternate database constraint
                if (!error)
                {
                    // To check later - RefreshToken
                    // https://stackoverflow.com/questions/53659247/using-aspnetusertokens-table-to-store-refresh-token-in-asp-net-core-web-api

                    var user = await signInManager.UserManager.FindByEmailAsync(input.Email);

                    if (user == null)
                    {
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem>() { new MessageItem { Code = errorMessageUserNotFound, Message = errorMessageUserNotFound, Detail = "" } });
                        return _payload;
                    }

                    // var refreshToken = await userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshToken");
                    //var isValid = await userManager.VerifyUserTokenAsync(user, "MyApp", "RefreshToken", input.RefreshToken);

                    // Get the UserApp
                    var userApp = await contextMVC.UserApp.Where(x => x.IdentityId == user.Id).FirstOrDefaultAsync();
                    if (userApp == null)
                    {
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem>() { new MessageItem { Code = errorMessageUserNotFound, Message = errorMessageUserNotFound, Detail = "" } });
                        return _payload;
                    }

                    var dbRefreshToken = await userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshToken");

                    if (!input.RefreshToken.Equals(dbRefreshToken))
                    {
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem>() { new MessageItem { Code = errorMessageInvalidAccessToken, Message = errorMessageInvalidAccessToken, Detail = "" } });
                        return _payload;
                    }

                    //***** Save and Commit to the Database (Atomic because is same Context DB) 
                    if (!error && autoCommit)
                    {
                        await contextMVC.SaveChangesAsync(); // Call it only once so do all other operations first
                    }

                    //***** Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                    _payload.PayloadResult = PayloadResult.ResultGood(Messages: new List<MessageItem> { new MessageItem { Code = okMessage, Message = okMessage, Detail = "" } });
                    _payload.JWTInfo = await this.CreateTokenAsync(user, userApp, configuration, userManager);

                }// if (!error)                
            }
            catch (Exception ex) // Main try 
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                string innerError = (ex.InnerException != null) ? ex.InnerException.Message : "";
                System.Diagnostics.Debug.WriteLine("Error Inner: " + innerError);
                _payload = new LoginUserTxnAccessTokenPayload
                {
                    PayloadResult = PayloadResult.ResultBad(Messages: new List<MessageItem> { new MessageItem { Code = errorMessageExceptionCode, Message = errorMessageException, Detail = ex.Message } })
                }; // Restart variable to avoid returning any already saved data                
            }
            finally
            {
                // Save Logs if needed
            }

            return _payload;
        }


        private async Task<JWTInfo> CreateTokenAsync(
            IdentityUser user,
            UserApp userApp,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager)
        {
            string jwtSection = "JwtSettings";

            // New approach get the claims from DB, add them in the JWT, encrypt the JWT, and use an interceptor in middleware to get this values an use them globally in the request
            var claims = await userManager.GetClaimsAsync(user);

            // Create the token
            var tokenSection = configuration.GetSection(jwtSection);
            DateTime requested = DateTime.Now;
            DateTime expiresAt = requested.AddMinutes(Convert.ToInt32(tokenSection["JwtExpiryInMinutes"]));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSection["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = expiresAt;

            var token = new JwtSecurityToken(
                tokenSection["JwtIssuer"],
                tokenSection["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            JwtSecurityToken jwtEncrypted = new JwtSecurityTokenHandler().CreateJwtSecurityToken(
                    issuer: tokenSection["JwtIssuer"],
                    audience: tokenSection["JwtAudience"],
                    subject: new ClaimsIdentity(claims),
                    expires: expiry,
                    notBefore: requested,
                    issuedAt: requested,
                    signingCredentials: creds,
                    encryptingCredentials: new EncryptingCredentials(key, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512)
                    );

            // Generate the access token
            await userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshToken");
            var newRefreshToken = await userManager.GenerateUserTokenAsync(user, "MyApp", "RefreshToken");
            await userManager.SetAuthenticationTokenAsync(user, "MyApp", "RefreshToken", newRefreshToken);

            var result = new JWTInfo
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                TokenEncrypted = new JwtSecurityTokenHandler().WriteToken(jwtEncrypted),
                Requested = requested,
                Expires = expiresAt,
                RefreshToken = newRefreshToken
            };

            return result;
        }

    }
}
