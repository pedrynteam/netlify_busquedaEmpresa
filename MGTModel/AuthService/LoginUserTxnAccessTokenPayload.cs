using MGTModel.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MGTModel.AuthService
{
    public class LoginUserTxnAccessTokenPayload
    {
        public PayloadResult PayloadResult { get; set; }
        public JWTInfo JWTInfo { get; set; }
    }
}
