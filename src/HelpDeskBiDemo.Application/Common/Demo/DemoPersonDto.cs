using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Demo;

public sealed record DemoPersonDto(
    int Id,
    string FullName,
    string JobTitle,
    string Department,
    DemoRole Role,
    string DisplayLabel);
