using System;

namespace MGTModel.AuthService
{
    public class JWTInfo
    {
        public string Token { get; set; }
        public string TokenEncrypted { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Requested { get; set; }
        public DateTime Expires { get; set; }
    }
}
