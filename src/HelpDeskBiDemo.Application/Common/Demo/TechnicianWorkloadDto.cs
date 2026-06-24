namespace HelpDeskBiDemo.Application.Common.Demo;

public sealed record TechnicianWorkloadDto(
    int TechnicianId,
    string TechnicianName,
    int ActiveTickets,
    int InProgressTickets,
    int OverdueTickets,
    int ResolvedTickets,
    double AverageResolutionHours);
