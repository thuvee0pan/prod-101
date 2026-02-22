namespace ExecutionOS.API.DTOs;

public record GoogleLoginRequest(string IdToken);

public record AuthResponse(
    string Token,
    Guid UserId,
    string Email,
    string Name,
    string? ProfilePicture
);
