using MGTModel.Helpers;

namespace MGTModel.AuthService
{
    public class LoginUserTxnPayload
    {                
        public PayloadResult PayloadResult { get; set; }
        public JWTInfo JWTInfo { get; set; }
    }
}
