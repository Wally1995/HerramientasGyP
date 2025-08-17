namespace HerramientasGyP.Api;

public class StartupStatusService : IStartupStatusService
{
    public bool IsSeedComplete { get; set; } = false;
}