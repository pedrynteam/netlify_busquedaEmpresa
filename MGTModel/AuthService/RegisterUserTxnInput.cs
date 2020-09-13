using System.ComponentModel.DataAnnotations;

namespace MGTModel.AuthService
{
    public class RegisterUserTxnInput
    {        
        [Required(ErrorMessage = "RegisterUserTxnInput***Required***Email")]
        [StringLength(60, ErrorMessage = "RegisterUserTxnInput***StringLength***Email", MinimumLength = 1)]
        [EmailAddress(ErrorMessage = "RegisterUserTxnInput***EmailAddress***Email")]        
        public string Email { get; set; }
        
        [Required(ErrorMessage = "RegisterUserTxnInput***Required***FirstName")]
        [StringLength(60, ErrorMessage = "RegisterUserTxnInput***StringLength***FirstName", MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "RegisterUserTxnInput***Required***LastName")]
        [StringLength(60, ErrorMessage = "RegisterUserTxnInput***StringLength***LastName", MinimumLength = 1)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "RegisterUserTxnInput***Required***Password")]        
        [DataType(DataType.Password, ErrorMessage = "RegisterUserTxnInput***Invalid***Password")]
        public string Password { get; set; }

        //[SkipRecursiveValidation]
        [Required(ErrorMessage = "RegisterUserTxnInput***Required***ConfirmPassword")]        
        // Compare not working to get member [Compare("Password", ErrorMessage = "RegisterUserTxnInput***Compare***ConfirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}
