using MGTModel.Helpers;

namespace MGTModel.AuthService
{
    public class RegisterUserTxnPayload
    {
        public PayloadResult PayloadResult { get; set; }
        public UserApp User { get; set; }
    }
}
