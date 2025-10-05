using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Models;
using AvailabilityApp.Api.Repositories;
using AvailabilityApp.Api.Utils;

namespace AvailabilityApp.Api.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<UserDto>> GetUserAsync(Guid userId);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                if (await _userRepository.ExistsAsync(registerDto.Email))
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        Errors = new List<string> { "Email already registered" }
                    };
                }

                // Create new user
                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    BusinessName = registerDto.BusinessName,
                    PhoneNumber = registerDto.PhoneNumber
                };

                var userId = await _userRepository.CreateAsync(user);

                // Generate JWT token
                var token = _jwtTokenGenerator.GenerateToken(userId, user.Email);

                var userDto = new UserDto
                {
                    Id = userId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    BusinessName = user.BusinessName,
                    PhoneNumber = user.PhoneNumber
                };

                return new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Message = "User registered successfully",
                    Data = new AuthResponseDto
                    {
                        Token = token,
                        User = userDto
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during registration",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Get user by email
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        Errors = new List<string> { "Email or password is incorrect" }
                    };
                }

                // Verify password
                if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        Errors = new List<string> { "Email or password is incorrect" }
                    };
                }

                // Generate JWT token
                var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    BusinessName = user.BusinessName,
                    PhoneNumber = user.PhoneNumber
                };

                return new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new AuthResponseDto
                    {
                        Token = token,
                        User = userDto
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    BusinessName = user.BusinessName,
                    PhoneNumber = user.PhoneNumber
                };

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = userDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching user",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}