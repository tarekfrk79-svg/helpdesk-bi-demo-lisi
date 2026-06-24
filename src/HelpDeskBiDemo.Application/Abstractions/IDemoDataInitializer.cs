namespace HelpDeskBiDemo.Application.Abstractions;

public interface IDemoDataInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
