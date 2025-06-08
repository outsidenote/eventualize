namespace EvDb.Core.Adapters;

public static class TaskExtensions
{
    // TODO: [bnaya 2025-06-04] write a post https://claude.ai/share/960c663d-55f1-45fd-9b3f-0c0a84e6f74f
    public static async Task<bool> FalseWhenCancelAsync(this Task<bool> task)
    {
        try
        {
            return await task;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
    public static async Task SwallowCancellationAsync(this Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // ignore cancellation
        }
    }
}