using System.ComponentModel.DataAnnotations;

namespace MGTModel.AuthService
{
    public class LoginUserTxnAccessTokenInput
    {        
        [Required(ErrorMessage = "LoginUserTxnAccessTokenInput***Required***Email")]
        [EmailAddress(ErrorMessage = "LoginUserTxnAccessTokenInput***EmailAddress***Email")]
        public string Email { get; set; }
     
        [Required(ErrorMessage = "LoginUserTxnAccessTokenInput***Required***RefreshToken")]
        public string RefreshToken { get; set; }
    }
}
