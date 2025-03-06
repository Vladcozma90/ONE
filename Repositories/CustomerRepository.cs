using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

public static class CustomerRepository
{
    public static List<Customer> GetCustomersFiltered(string region, string cluster, string team, string esa, string customer)
    {
        var customers = new List<Customer>();
        using var connection = DatabaseHelper.GetConnection();

        string query = @"SELECT Id, Customer_name, Incoterm, Bill_to, D_code, Destination, Ship_to, Freight_cost, Country, Customer_contact FROM Customer_info WHERE 1=1";
        using var command = new SQLiteCommand(connection);

        ApplyFilters(command, ref query, region, cluster, team, esa, customer);

        command.CommandText = query;
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            customers.Add(new Customer
            {
                Id = reader.GetInt32(0),
                Name = reader.SafeGetString(1),
                Incoterm = reader.SafeGetString(2),
                BillTo = reader.SafeGetInt(3),
                DCode = reader.SafeGetString(4),
                Destination = reader.SafeGetString(5),
                ShipTo = reader.SafeGetInt(6),
                FreightCost = reader.SafeGetString(7),
                Country = reader.SafeGetString(8),
                Contact = reader.SafeGetString(9),
            });

        }
        return customers;
    }

    public static Dictionary<string, List<string>> GetFilteredUniqueValues(string region, string cluster, string team, string esa, string customer)
    {
        var filters = new Dictionary<string, List<string>>()
        {
            {"region", new() },
            {"cluster", new() },
            {"team", new() },
            {"esa", new() },
            {"customer", new() }
        };
        using var connection = DatabaseHelper.GetConnection();

        string query = "SELECT DISTINCT region, cluster, team, CAST(Esa AS TEXT), Customer_name FROM Customer_info WHERE 1=1";
        using var command = new SQLiteCommand(connection);

        ApplyFilters(command, ref query, region, cluster, team, esa, customer);

        command.CommandText = query;
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            filters["region"].Add(reader.SafeGetString(0));
            filters["cluster"].Add(reader.SafeGetString(1));
            filters["team"].Add(reader.SafeGetString(2));
            filters["esa"].Add(reader.SafeGetString(3));
            filters["customer"].Add(reader.SafeGetString(4));
        }
        return filters.ToDictionary(k => k.Key, v => v.Value.Distinct().OrderBy(x => x).ToList());
    }

    public static string GetCustomerNotes(string customer)
    {
        using var connection = DatabaseHelper.GetConnection();
        using var command = new SQLiteCommand(@"SELECT Specific_notes FROM User_input_data WHERE Id = (SELECT Id FROM Customer_info WHERE Customer_name = @Customer_name)", connection);
        command.Parameters.AddWithValue("@Customer_name", customer);
        return command.ExecuteScalar()?.ToString() ?? "";

    }

    public static void SaveCustomerNotes(string customer, string notes)
    {
        using var connection = DatabaseHelper.GetConnection();
        string sql = @"
        INSERT INTO User_input_data (Id, specific_notes)
        VALUES ((SELECT Id FROM Customer_info WHERE Customer_name = @Customer_name), @specific_notes)
        ON CONFLICT(Id) DO UPDATE SET specific_notes = excluded.specific_notes;";

        using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@specific_notes", notes);
        command.Parameters.AddWithValue("@Customer_name", customer);
        command.ExecuteNonQuery();
    }


    private static void ApplyFilters(SQLiteCommand command, ref string query, string region, string cluster, string team, string esa, string customer)
    {
        var filters = new Dictionary<string, string>
        {
            { "region", region },
            { "cluster", cluster },
            { "team", team },
            { "esa", esa },
            { "customer_name", customer }
        };

        foreach (var filter in filters)
        {
            if (!string.IsNullOrEmpty(filter.Value) && filter.Value != "All")
            {
                query += $" AND {filter.Key} = @{filter.Key}";
                command.Parameters.AddWithValue($"@{filter.Key}", filter.Value);
            }
        }

    }
}
