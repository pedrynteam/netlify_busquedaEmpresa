using System;
using System.Threading.Tasks;
using HotChocolate.Subscriptions;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using MGTModel.AuthService;
using MGTModel.Helpers;
using MGTModel.Helpers.DataAnnotationsValidator;
using IdentityServer.Data;

namespace MGTServices.AuthService
{
    public class RegisterUserTxn
    {
        public RegisterUserTxn()
        {
        }

        public async Task<RegisterUserTxnPayload> Execute(
            RegisterUserTxnInput input,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext contextMVC,
            bool autoCommit)
        {
            RegisterUserTxnPayload _payload = new RegisterUserTxnPayload
            {
                PayloadResult = new PayloadResult
                {
                    Messages = new List<MessageItem>()
                }
            };

            //***** Declare variables and messages                         
            string okMessage = "RegisterUserTxn***USER_REGISTERED";
            string errorMessageExceptionCode = "RegisterUserTxn***TXN_EXCEPTION";
            string errorMessageException = "TXN_EXCEPTION";
            string claimType = "UserType";
            string claimUserType1 = "User";
            string claimUserType2 = "RegisteredUser";
            string validatorCode = "InputValidationError";
            string errorWrongConfirmationPassMessage = "RegisterUserTxnInput***Compare***ConfirmPassword"; // To translate to: Password and Confirm Password don't match.
            string errorWrongConfirmationPassDetail = "ConfirmPassword"; // To translate to: Password and Confirm Password don't match.

            // Error handling
            bool error = false; // To Handle Only One Error

            try
            {
                // DBContext by convention is a UnitOfWork, track changes and commits when SaveChanges is called
                // Multithreading issue: so if we use only one DBContext, it will track all entities (_context.Customer.Add) and commit them when SaveChanges is called, 
                // No Matter what transactions or client calls SaveChanges.
                // Note: Rollback will not remove the entities from the tracker in the context. so better dispose it.

                //***** Make The Validations - Be careful : Concurrency. Same name can be saved multiple times if called at the exact same time. Better have an alternate database constraint

                // Validate Model Annotations (if needed)                                
                var validationResults = new List<ValidationResult>();
                DataAnnotationsValidator.TryValidateObjectRecursive(input, validationResults);
                if (validationResults.Count > 0)
                {
                    // "code": "InputValidationError",
                    // "message": "RegisterUserTxnInput***LastName***Required", // To be transalated
                    // "detail": "LastName" // To be used by the FrontEnd
                    _payload.PayloadResult = PayloadResult.ResultBad(Messages: validationResults.Select(o => new MessageItem { Code = validatorCode, Message = o.ErrorMessage, Detail = o.MemberNames.FirstOrDefault() }).ToList());
                    error = true;
                }

                // Validate Confirm email, the Annotation is not working
                if (!input.Password.Equals(input.ConfirmPassword))
                {
                    _payload.PayloadResult.Messages.Add(new MessageItem { Code = validatorCode, Message = errorWrongConfirmationPassMessage, Detail = errorWrongConfirmationPassDetail });
                    error = true;
                }

                if (error) // Validations
                {
                    return _payload;
                }

                if (!error)
                {
                    var newUser = new IdentityUser { UserName = input.Email, Email = input.Email };
                    var result = await userManager.CreateAsync(newUser, input.Password);

                    if (!result.Succeeded)
                    {
                        /*  1. Password validation: PasswordTooShort, PasswordRequiresNonAlphanumeric, PasswordRequiresLower, PasswordRequiresUpper, PasswordRequiresDigit
                                "code": "InputValidationError",
                                "message": "RegisterUserTxnInput***PasswordTooShort",
                                "detail": "Password" 

                            2. Any other error validation:
                                "code": "RegisterUserTxnInput***DuplicateUserName",
                                "message": "RegisterUserTxnInput***DuplicateUserName",
                                "detail": ""                         
                         */
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: (result.Errors.Select(o => new MessageItem { Code = o.Code.StartsWith("Password") ? validatorCode : "RegisterUserTxnInput***" + o.Code, Message = "RegisterUserTxnInput***" + o.Code, Detail = o.Code.StartsWith("Password") ? "Password" : "" })).ToList());
                        return _payload;
                    }

                    // Create the user in MVC Database
                    UserApp userApp = new UserApp
                    {
                        IdentityId = newUser.Id,
                        Email = input.Email,
                        FirstName = input.FirstName,
                        LastName = input.LastName,
                        Active = true,
                        LastSeen = DateTime.Now
                    };

                    contextMVC.UserApp.Add(userApp);

                    //***** Save and Commit to the Database (Atomic because is same Context DB) 
                    if (!error && autoCommit)
                    {
                        await contextMVC.SaveChangesAsync(); // Call it only once so do all other operations first
                    }

                    // Save the Name Claim into the database, add the claims in the JWT, encrypt the JWT, and use an interceptor in middleware to get this values an use them globally in the request
                    var _claims = new[]
                    {
                        new Claim(ClaimTypes.Name, userApp.Id),
                        new Claim(ClaimTypes.Email, userApp.Email),
                        new Claim("Identity", userApp.IdentityId),
                        new Claim(ClaimTypes.Role, claimUserType1),
                        new Claim(ClaimTypes.Role, claimUserType2),
                        new Claim(claimType, claimUserType1) // Create all user equal, add other claims later in database                             
                    };

                    var resultClaims = await userManager.AddClaimsAsync(newUser, _claims); // Save claims in the Database

                    if (!resultClaims.Succeeded)
                    {
                        _payload.PayloadResult = PayloadResult.ResultBad(Messages: (resultClaims.Errors.Select(o => new MessageItem { Code = "RegisterUserTxn***" + o.Code, Message = "RegisterUserTxn***" + o.Code, Detail = "" })).ToList());
                        return _payload;
                    }

                    //***** Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                    _payload.PayloadResult = PayloadResult.ResultGood(Messages: new List<MessageItem> { new MessageItem { Code = okMessage, Message = okMessage, Detail = "" } });
                    _payload.User = userApp;

                }// if (!error)
            }
            catch (Exception ex) // Main try 
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                string innerError = (ex.InnerException != null) ? ex.InnerException.Message : "";
                System.Diagnostics.Debug.WriteLine("Error Inner: " + innerError);
                _payload = new RegisterUserTxnPayload
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

    }
}
