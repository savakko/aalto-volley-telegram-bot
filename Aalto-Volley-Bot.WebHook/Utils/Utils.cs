public class Utils
{
    public static async Task<bool> TryActionAsync(Task action)
    {
        try
        {
            await action;
            return true;
        }
        catch
        {
            return false;
        }
    }
}