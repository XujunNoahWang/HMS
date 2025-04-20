using HMS.Data;
using HMS.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// To manage visit records and payments
public class RecordService
{
    private readonly DatabaseConnection _dbConnection;

    public RecordService(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<long> AddVisitRecordAsync(int patientId, VisitRecord record, int doctorId, string visitType)
    {
        using var connection = _dbConnection.GetConnection();
        await connection.OpenAsync();
        MySqlTransaction transaction = connection.BeginTransaction();

        try
        {
            // Insert visit record
            string visitQuery = @"
                INSERT INTO VisitRecords (patient_id, visit_date, symptoms, examinations, diagnosis, disease_type, doctor_id)
                VALUES (@PatientId, @VisitDate, @Symptoms, @Examinations, @Diagnosis, @DiseaseType, @DoctorId);
                SELECT LAST_INSERT_ID();";
            using var visitCommand = new MySqlCommand(visitQuery, connection, transaction);
            visitCommand.Parameters.AddWithValue("@PatientId", patientId);
            visitCommand.Parameters.AddWithValue("@VisitDate", record.VisitDate);
            visitCommand.Parameters.AddWithValue("@Symptoms", record.Symptoms);
            visitCommand.Parameters.AddWithValue("@Examinations", (object)record.Examinations ?? DBNull.Value);
            visitCommand.Parameters.AddWithValue("@Diagnosis", record.Diagnosis);
            visitCommand.Parameters.AddWithValue("@DiseaseType", record.DiseaseType);
            visitCommand.Parameters.AddWithValue("@DoctorId", doctorId);
            long visitId = Convert.ToInt64(await visitCommand.ExecuteScalarAsync());

            // Calculate total amount (only prescriptions)
            decimal totalAmount = 0.00m; 
            var paymentItems = new List<(string Type, string Description, int Quantity, decimal Amount)>(); 

            foreach (var prescription in record.Prescriptions)
            {
                if (string.IsNullOrEmpty(prescription.Type) || string.IsNullOrEmpty(prescription.Name) || prescription.Quantity <= 0 || prescription.Price < 0)
                {
                    throw new Exception($"Invalid prescription data for {prescription.Type}: {prescription.Name}.");
                }

                string prescriptionQuery = @"
                    INSERT INTO Prescriptions (visit_id, type, name, quantity, price)
                    VALUES (@VisitId, @Type, @Name, @Quantity, @Price);
                    SELECT LAST_INSERT_ID();";
                using var prescriptionCommand = new MySqlCommand(prescriptionQuery, connection, transaction);
                prescriptionCommand.Parameters.AddWithValue("@VisitId", visitId);
                prescriptionCommand.Parameters.AddWithValue("@Type", prescription.Type);
                prescriptionCommand.Parameters.AddWithValue("@Name", prescription.Name);
                prescriptionCommand.Parameters.AddWithValue("@Quantity", prescription.Quantity);
                prescriptionCommand.Parameters.AddWithValue("@Price", prescription.Price);
                await prescriptionCommand.ExecuteScalarAsync();

                // Update inventory
                string tableName = prescription.Type switch
                {
                    "Medicine" => "Medicines",
                    "Tests" => "Tests",
                    "Surgery" => "Surgeries",
                    "Equipment" => "Equipment",
                    _ => throw new Exception($"Invalid prescription type: {prescription.Type}")
                };
                string inventoryQuery = $@"
                    UPDATE {tableName}
                    SET quantity = quantity - @Quantity
                    WHERE name = @Name AND quantity >= @Quantity;";
                using var inventoryCommand = new MySqlCommand(inventoryQuery, connection, transaction);
                inventoryCommand.Parameters.AddWithValue("@Quantity", prescription.Quantity);
                inventoryCommand.Parameters.AddWithValue("@Name", prescription.Name);
                int rowsAffected = await inventoryCommand.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new Exception($"Insufficient inventory for {prescription.Type}: {prescription.Name}. Available quantity is less than {prescription.Quantity}.");
                }

                // Calculate prescription cost (price * quantity)
                decimal prescriptionCost = prescription.Price * prescription.Quantity;
                string paymentItemType = prescription.Type == "Tests" ? "Test" : prescription.Type;
                paymentItems.Add((paymentItemType, prescription.Name, prescription.Quantity, prescription.Price));
                totalAmount += prescriptionCost;
                                
            }

           

            // Insert payment (only if there are prescriptions, otherwise totalAmount is 0)
            if (totalAmount > 0)
            {
                string paymentQuery = @"
                    INSERT INTO Payments (patient_id, visit_id, amount, status, visit_type, created_date)
                    VALUES (@PatientId, @VisitId, @Amount, 'Unpaid', @VisitType, @CreatedDate);
                    SELECT LAST_INSERT_ID();";
                using var paymentCommand = new MySqlCommand(paymentQuery, connection, transaction);
                paymentCommand.Parameters.AddWithValue("@PatientId", patientId);
                paymentCommand.Parameters.AddWithValue("@VisitId", visitId);
                paymentCommand.Parameters.AddWithValue("@Amount", totalAmount);
                paymentCommand.Parameters.AddWithValue("@VisitType", visitType);
                paymentCommand.Parameters.AddWithValue("@CreatedDate", record.VisitDate.Date);
                long paymentId = Convert.ToInt64(await paymentCommand.ExecuteScalarAsync());

                // Insert payment items
                foreach (var item in paymentItems)
                {
                    string paymentItemQuery = @"
                        INSERT INTO PaymentItems (payment_id, type, description, quantity, amount)
                        VALUES (@PaymentId, @Type, @Description, @Quantity, @Amount);";
                    using var paymentItemCommand = new MySqlCommand(paymentItemQuery, connection, transaction);
                    paymentItemCommand.Parameters.AddWithValue("@PaymentId", paymentId);
                    paymentItemCommand.Parameters.AddWithValue("@Type", item.Type);
                    paymentItemCommand.Parameters.AddWithValue("@Description", item.Description);
                    paymentItemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                    paymentItemCommand.Parameters.AddWithValue("@Amount", item.Amount);
                    await paymentItemCommand.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
            return visitId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Dictionary<string, List<(string Name, decimal Price)>>> LoadItemDataAsync()
    {
        var itemData = new Dictionary<string, List<(string Name, decimal Price)>>
        {
            { "Medicine", new List<(string, decimal)>() },
            { "Tests", new List<(string, decimal)>() },
            { "Surgery", new List<(string, decimal)>() },
            { "Equipment", new List<(string, decimal)>() }
        };

        using var connection = _dbConnection.GetConnection();
        await connection.OpenAsync();

        foreach (var type in itemData.Keys)
        {
            string tableName = type switch
            {
                "Medicine" => "Medicines",
                "Tests" => "Tests",
                "Surgery" => "Surgeries",
                "Equipment" => "Equipment",
                _ => throw new ArgumentException($"Invalid type: {type}")
            };
            string query = $"SELECT name, price FROM {tableName}";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                itemData[type].Add((reader.GetString("name"), reader.GetDecimal("price")));
            }
        }

        return itemData;
    }
}