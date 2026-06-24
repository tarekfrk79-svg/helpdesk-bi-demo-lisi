using HelpDeskBiDemo.Application.Common.Access;

namespace HelpDeskBiDemo.Application.Abstractions;

public interface IAccessService
{
    Task<AccessCodeResolution> ResolveAsync(string code, CancellationToken cancellationToken = default);
}
