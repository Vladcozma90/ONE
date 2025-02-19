using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Newtonsoft.Json;

public class Shipment
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string PONumber { get; set; }
    public string CountryDeparture { get; set; }
    public string Incoterm { get; set; }
    public string BookingReference { get; set; }
    public string LoadingDate { get; set; }
    public string ETD { get; set; }
    public string ETA { get; set; }
    public string UCNumber { get; set; }
    public string POL { get; set; }
    public string POD { get; set; }
    public int Load { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new Dictionary<string, string>();
}

public static class ShipmentsRepository
{
    public static List<Shipment> GetAllShipments()
    {
        List<Shipment> shipments = new List<Shipment>();

        using (var connection = DatabaseHelper.GetConnection())
        {
            connection.Open();
            string query = "SELECT * FROM shipments";

            using (var command = new SQLiteCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    shipments.Add(new Shipment
                    {
                        Id = reader.GetInt32(0),
                        CustomerName = reader.GetString(1),
                        PONumber = reader.GetString(2),
                        CountryDeparture = reader.GetString(3),
                        Incoterm = reader.GetString(4),
                        BookingReference = reader.GetString(5),
                        LoadingDate = reader.GetString(6),
                        ETD = reader.GetString(7),
                        ETA = reader.GetString(8),
                        UCNumber = reader.GetString(9),
                        POL = reader.GetString(10),
                        POD = reader.GetString(11),
                        Load = reader.GetInt32(12),
                        CustomFields = reader.IsDBNull(13) ? new Dictionary<string, string>() :
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.GetString(13))
                    });
                }
            }
        }

        return shipments;
    }

    public static void SaveShipment(Shipment shipment)
    {
        using (var connection = DatabaseHelper.GetConnection())
        {
            connection.Open();
            string query = @"
                INSERT INTO shipments (Customer_name, PO_number, Country_departure, Incoterm, Booking_reference,
                    Loading_date, ETD, ETA, UC_number, POL, POD, Load, CustomFields)
                VALUES (@CustomerName, @PONumber, @CountryDeparture, @Incoterm, @BookingReference,
                    @LoadingDate, @ETD, @ETA, @UCNumber, @POL, @POD, @Load, @CustomFields)
                ON CONFLICT(Id) DO UPDATE SET
                    Customer_name = excluded.Customer_name,
                    PO_number = excluded.PO_number,
                    Country_departure = excluded.Country_departure,
                    Incoterm = excluded.Incoterm,
                    Booking_reference = excluded.Booking_reference,
                    Loading_date = excluded.Loading_date,
                    ETD = excluded.ETD,
                    ETA = excluded.ETA,
                    UC_number = excluded.UC_number,
                    POL = excluded.POL,
                    POD = excluded.POD,
                    Load = excluded.Load,
                    CustomFields = excluded.CustomFields;";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerName", shipment.CustomerName);
                command.Parameters.AddWithValue("@PONumber", shipment.PONumber);
                command.Parameters.AddWithValue("@CountryDeparture", shipment.CountryDeparture);
                command.Parameters.AddWithValue("@Incoterm", shipment.Incoterm);
                command.Parameters.AddWithValue("@BookingReference", shipment.BookingReference);
                command.Parameters.AddWithValue("@LoadingDate", shipment.LoadingDate);
                command.Parameters.AddWithValue("@ETD", shipment.ETD);
                command.Parameters.AddWithValue("@ETA", shipment.ETA);
                command.Parameters.AddWithValue("@UCNumber", shipment.UCNumber);
                command.Parameters.AddWithValue("@POL", shipment.POL);
                command.Parameters.AddWithValue("@POD", shipment.POD);
                command.Parameters.AddWithValue("@Load", shipment.Load);
                command.Parameters.AddWithValue("@CustomFields", JsonConvert.SerializeObject(shipment.CustomFields));

                command.ExecuteNonQuery();
            }
        }
    }
}
