namespace ClientAcess.Models.Enums
{
    public enum ApiCode
    {
        Success = 0,
        EmailNotConfirmed = 1003,
        FailedToAssignRole = 1004,
        UserAlreadyExists = 1005,
        EmailAlreadyExists = 1006,
        UserCreationFailed = 1007,
        TokenGenerationFailed = 1008,
        UserAccountLocked = 1009,
        Disable2FA = 1010,
        UserNotFound = 1011,
        InvalidOTP = 1012,
        InvalidRefreshToken = 1013,
        RefreshTokenExpired = 1014,
        InvalidInputData = 1015,
        FailSendEmail = 1016,
        UserAlreadyExist = 1017,
        ExpiredTokenEmail = 1018,

        InvalidLogin = 1019,
        FailSendOTP = 1020,
        InvalidCredentials = 1021,
        EmailRequired = 1022,
        ResetLink = 1023,

        EmailNotFound = 1024,
        FailedResetPassword = 1025,
        FailedRefreshToken = 1026
    }
}
