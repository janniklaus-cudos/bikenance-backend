namespace Backend.Jobs;

public sealed class JobGenerationService(IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var runTime = new TimeOnly(3, 0);

        while (!ct.IsCancellationRequested)
        {
            // schedule next task
            var now = DateTimeOffset.Now;
            var next = NextOccurrence(now, runTime);

            var delay = next - now;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            try
            {
                await Task.Delay(delay, ct);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                using var scope = services.CreateScope();
                var generator = scope.ServiceProvider.GetRequiredService<IRepeatJourneyGenerationJob>();
                await generator.RunAsync(ct);
            }
            catch (Exception _)
            {
                // swallow to keep service alive
            }
        }
    }

    private static DateTimeOffset NextOccurrence(DateTimeOffset now, TimeOnly runTime)
    {
        var todayRun = new DateTimeOffset(
            now.Year, now.Month, now.Day,
            runTime.Hour, runTime.Minute, runTime.Second,
            now.Offset);

        return (now <= todayRun) ? todayRun : todayRun.AddDays(1);
    }
}