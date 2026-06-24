using HelpDeskBiDemo.Application.Common.Owner;

namespace HelpDeskBiDemo.Application.Abstractions;

public interface IOwnerManagementService
{
    Task<OwnerDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task CreateCompanyAsync(string companyName, CancellationToken cancellationToken = default);

    Task SetCompanyActivationAsync(int companyId, bool isActive, CancellationToken cancellationToken = default);

    Task ResetCompanyDataAsync(int companyId, CancellationToken cancellationToken = default);

    Task DeleteCompanyAsync(int companyId, CancellationToken cancellationToken = default);
}
