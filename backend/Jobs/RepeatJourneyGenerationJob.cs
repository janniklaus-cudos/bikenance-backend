
using Backend.Data;
using Backend.Services;

namespace Backend.Jobs;

public interface IRepeatJourneyGenerationJob
{
    Task RunAsync(CancellationToken ct);
}

public sealed class RepeatJourneyGenerationJob(IRepeatJourneyService repeatJourneyService) : IRepeatJourneyGenerationJob
{

    public async Task RunAsync(CancellationToken ct)
    {
        await repeatJourneyService.GenerateAllForToday();
    }
}