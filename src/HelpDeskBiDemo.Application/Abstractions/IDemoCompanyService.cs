using HelpDeskBiDemo.Application.Common.Demo;
using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Abstractions;

public interface IDemoCompanyService
{
    Task<CompanyContextDto?> GetCompanyContextAsync(int companyId, CancellationToken cancellationToken = default);

    Task<DemoPersonDto?> GetDefaultAdminAsync(int companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DemoPersonDto>> GetPeopleByRoleAsync(
        int companyId,
        DemoRole role,
        CancellationToken cancellationToken = default);

    Task<DemoPersonDto?> GetPersonAsync(
        int companyId,
        int personId,
        DemoRole role,
        CancellationToken cancellationToken = default);

    Task<DemoDashboardDto?> GetDashboardAsync(
        int companyId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default);
}
