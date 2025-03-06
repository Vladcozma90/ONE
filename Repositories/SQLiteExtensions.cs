using System;
using System.Data.SQLite;

public static class SQLiteExtensions
{
    public static string SafeGetString(this SQLiteDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
            return string.Empty;

        var type = reader.GetFieldType(index);
        if (type == typeof(string))
            return reader.GetString(index);
        else
            return reader[index]?.ToString() ?? string.Empty;  // ✅ Ensures conversion to string
    }

    public static int SafeGetInt(this SQLiteDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
            return 0;

        var type = reader.GetFieldType(index);
        if (type == typeof(int))
            return reader.GetInt32(index);
        else if (int.TryParse(reader[index]?.ToString(), out int result))
            return result;
        else
            return 0;  // ✅ Default fallback for invalid conversion
    }
}