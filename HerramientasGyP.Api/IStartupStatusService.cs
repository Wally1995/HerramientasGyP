namespace HerramientasGyP.Api;

public interface IStartupStatusService
{
    bool IsSeedComplete { get; set; }
}