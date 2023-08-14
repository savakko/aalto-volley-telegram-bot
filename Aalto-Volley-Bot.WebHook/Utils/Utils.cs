public class Utils
{
    public static (string path, Dictionary<string, string> queryParams) ParseCallbackQuery(string? queryData)
    {
        var parameters = ParseQueryParams(queryData);

        if (string.IsNullOrEmpty(queryData))
            return ("", parameters);

        var path = queryData.Split('?').First();

        return (path, parameters);
    }

    public static Dictionary<string, string> ParseQueryParams(string? queryData)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(queryData))
            return result;

        var queryParams = queryData.Split('?').Last().Split('&');

        foreach (var query in queryParams)
        {
            var partition = query.Split('=');
            if (partition.Length == 2)
                result.Add(partition[0], partition[1]);
        }

        return result;
    }

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