namespace ApiSDD.Common;

/// <summary>
/// nv-04: RowVersion base64 codec shared by controllers and services.
/// </summary>
internal static class RowVersionCodec
{
    public static string Encode(byte[] rowVersion)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);
        return Convert.ToBase64String(rowVersion);
    }

    public static bool TryDecode(string? wire, out byte[] rowVersion)
    {
        rowVersion = [];
        if (string.IsNullOrWhiteSpace(wire))
        {
            return false;
        }

        try
        {
            rowVersion = Convert.FromBase64String(wire);
            return rowVersion.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
