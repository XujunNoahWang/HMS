using HMS.Services;
using HMS.Models;
using HMS.Data;
using Microsoft.AspNetCore.Components;
using MySqlConnector;
using System;
using System.Collections.Generic;

// To get the patient information
public abstract class PatientsBase : RecordsBase
{
    [Inject]
    protected DatabaseConnection DatabaseConnection { get; set; }

    [SupplyParameterFromQuery]
    public string PatientId { get; set; }

    protected User Patient { get; set; }

    // Virtual method: Subclasses can override to add extra permissions check
    protected virtual bool CanAccess()
    {
        var currentUser = UserStateService.GetCurrentUser();
        return currentUser != null && currentUser.Role == ExpectedRole;
    }

    // Override to customize empty records message
    protected override string EmptyRecordsMessage => "No visit records found for this patient.";

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(PatientId))
        {
            HandleAccess();
        }
    }

    protected void HandleAccess()
    {
        ErrorMessage = null;
        Patient = null;
        VisitRecords.Clear();

        if (string.IsNullOrWhiteSpace(PatientId) || !int.TryParse(PatientId, out int patientId) || PatientId.Length != 9)
        {
            ErrorMessage = "Please enter a valid 9-digit numeric Patient ID.";
            return;
        }

        if (!CanAccess())
        {
            ErrorMessage = "You do not have permission to access this page.";
            return;
        }

        try
        {
            using var connection = DatabaseConnection.GetConnection();
            connection.Open();

            // Query patient information
            string patientQuery = @"
                SELECT u.*
                FROM Users u
                JOIN Patients p ON u.id = p.user_id
                WHERE u.id = @PatientId AND u.role = 'Patient' AND u.status = 'Active'";
            using var patientCommand = new MySqlCommand(patientQuery, connection);
            patientCommand.Parameters.AddWithValue("@PatientId", patientId);
            using var reader = patientCommand.ExecuteReader();
            if (reader.Read())
            {
                Patient = new Patient
                {
                    Id = reader.GetInt32("id"),
                    FirstName = reader.GetString("first_name"),
                    LastName = reader.GetString("last_name"),
                    Email = reader.GetString("email"),
                    Phone = reader.GetInt64("phone"),
                    Gender = (Gender)Enum.Parse(typeof(Gender), reader.GetString("gender")),
                    BirthDate = reader.GetDateTime("birth_date"),
                    Role = (Role)Enum.Parse(typeof(Role), reader.GetString("role")),
                    Status = (Status)Enum.Parse(typeof(Status), reader.GetString("status"))
                };
            }
            else
            {
                ErrorMessage = "Patient not found.";
                return;
            }
            reader.Close();

            // Load visit records
            LoadRecords(patientId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load patient data: {ex.Message}";
            Patient = null;
            VisitRecords.Clear();
        }
    }

    protected override void LoadRecords(int patientId)
    {
        try
        {
            using var connection = DatabaseConnection.GetConnection();
            connection.Open();

            string recordsQuery = @"
                SELECT vr.id, vr.patient_id, vr.visit_date, vr.symptoms, vr.examinations, vr.disease_type, vr.diagnosis, vr.doctor_id,
                    GROUP_CONCAT(
                        CONCAT(p.type, ': ', p.name, ', Qty: ', p.quantity, ', $', p.price)
                        SEPARATOR '\n'
                    ) AS prescription
                FROM VisitRecords vr
                LEFT JOIN Prescriptions p ON vr.id = p.visit_id
                WHERE vr.patient_id = @PatientId
                GROUP BY vr.id";
            using var recordsCommand = new MySqlCommand(recordsQuery, connection);
            recordsCommand.Parameters.AddWithValue("@PatientId", patientId);
            using var recordsReader = recordsCommand.ExecuteReader();
            VisitRecords.Clear();
            while (recordsReader.Read())
            {
                VisitRecords.Add(new VisitRecord
                {
                    Id = recordsReader.GetInt64("id"),
                    PatientId = recordsReader.GetInt32("patient_id"),
                    VisitDate = recordsReader.GetDateTime("visit_date"),
                    Symptoms = recordsReader.GetString("symptoms"),
                    Examinations = recordsReader.IsDBNull(recordsReader.GetOrdinal("examinations")) ? null : recordsReader.GetString("examinations"),
                    DiseaseType = recordsReader.GetString("disease_type"),
                    Diagnosis = recordsReader.GetString("diagnosis"),
                    DoctorId = recordsReader.GetInt32("doctor_id"),
                    Prescription = recordsReader.IsDBNull(recordsReader.GetOrdinal("prescription")) ? null : recordsReader.GetString("prescription"),
                    Prescriptions = new List<Prescription>() // Empty list for queried records
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load records: {ex.Message}";
            VisitRecords.Clear();
        }
    }

    protected int CalculateAge(DateTime birthDate)
    {
        // Calculate age based on birth date
        var today = DateTime.Today;
        int age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}