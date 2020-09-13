using System.ComponentModel.DataAnnotations;

namespace MGTModel.AuthService
{
    public class LoginUserTxnInput
    {        
        [Required(ErrorMessage = "LoginUserTxnInput***Required***Email")]
        [EmailAddress(ErrorMessage = "LoginUserTxnInput***EmailAddress***Email")]
        public string Email { get; set; }
     
        [Required(ErrorMessage = "LoginUserTxnInput***Required***Password")]
        public string Password { get; set; }
    }
}
