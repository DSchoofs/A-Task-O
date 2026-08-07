namespace Schoofs_Atasko.Services;

public interface ICurrentUserService
{
    int? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }
}