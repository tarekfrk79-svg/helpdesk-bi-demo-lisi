using HelpDeskBiDemo.Application.Common.Demo;
using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Web.ViewModels;

public sealed class PersonSelectionViewModel
{
    public int CompanyId { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public DemoRole Role { get; init; }

    public string RoleLabel { get; init; } = string.Empty;

    public IReadOnlyList<DemoPersonDto> People { get; init; } = Array.Empty<DemoPersonDto>();
}
