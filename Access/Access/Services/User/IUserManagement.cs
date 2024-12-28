using Access.Data;
using Access.Models.Authentication;
using Access.Models;

namespace Access.Services.User
{
    public interface IUserManagement
    {
        Task<UserResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser);
        Task<UserResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, ApplicationUser user);
        Task<UserResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel);
        Task<UserResponse<LoginResponse>> GetJwtTokenAsync(ApplicationUser user);
        Task<UserResponse<LoginResponse>> LoginUserWithJWTokenAsync(string otp, string userName);
        //Task<UserResponse<LoginResponse>> RenewAccessTokenAsync(LoginResponse tokens);
    }
}
