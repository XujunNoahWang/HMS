using HMS.Services;
using HMS.Models;
using HMS.Data;
using Microsoft.AspNetCore.Components;
using MySqlConnector;
using System;
using System.Collections.Generic;

// To get the schedule information
public abstract class ScheduleBase : ComponentBase
{
    [Inject]
    protected UserStateService UserStateService { get; set; }

    [Inject]
    protected NavigationManager Navigation { get; set; }

    [Inject]
    protected DatabaseConnection DatabaseConnection { get; set; }

    protected string PatientId { get; set; }
    protected List<ScheduleBase.AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();
    protected string ErrorMessage { get; set; }

    // Abstract property: Subclasses specify the role
    protected abstract Role ExpectedRole { get; }

    // Virtual property: Subclasses can override the patient link prefix
    protected virtual string PatientLinkPrefix => $"/{ExpectedRole.ToString().ToLower()}/patients";

    // Virtual method: Subclasses can override to customize query conditions
    protected virtual string GetQueryConditions()
    {
        return string.Empty;
    }

    // Virtual method: Subclasses can override to add additional parameters
    protected virtual void AddQueryParameters(MySqlCommand command)
    {
    }

    // Virtual method: Subclasses can override to control doctor_id filter
    protected virtual bool ApplyDoctorIdFilter()
    {
        return true; // Default: Apply doctor_id filter
    }

    protected override void OnInitialized()
    {
        var currentUser = UserStateService.GetCurrentUser();
        if (currentUser == null || currentUser.Role != ExpectedRole)
        {
            Navigation.NavigateTo("/", forceLoad: true);
            return;
        }
        LoadAppointments(null);
    }

    protected void HandleAccess()
    {
        // Clear previous error message
        ErrorMessage = null;

        if (!string.IsNullOrWhiteSpace(PatientId))
        {
            if (!int.TryParse(PatientId, out int patientId) || PatientId.Length != 9)
            {
                ErrorMessage = "Please enter a valid 9-digit numeric Patient ID.";
                Appointments.Clear();
                return;
            }
        }

        LoadAppointments(string.IsNullOrWhiteSpace(PatientId) ? null : int.Parse(PatientId));
    }

    protected void HandleViewAll()
    {
        // Clear PatientId and load all appointments
        PatientId = null;
        LoadAppointments(null);
    }

    protected virtual void LoadAppointments(int? patientId)
    {
        try
        {
            var currentUser = UserStateService.GetCurrentUser();
            if (currentUser == null)
            {
                ErrorMessage = $"{ExpectedRole} not logged in.";
                return;
            }

            using var connection = DatabaseConnection.GetConnection();
            connection.Open();

            string query = @"
                SELECT a.*, u.first_name, u.last_name, u.phone, u.gender, u.birth_date
                FROM Appointments a
                JOIN Users u ON a.patient_id = u.id";

            // Apply conditions dynamically
            var conditions = new List<string>();
            if (ApplyDoctorIdFilter())
            {
                conditions.Add("a.doctor_id = @DoctorId");
            }
            var customConditions = GetQueryConditions();
            if (!string.IsNullOrEmpty(customConditions))
            {
                conditions.Add(customConditions.TrimStart(" AND ".ToCharArray()));
            }
            if (patientId.HasValue)
            {
                conditions.Add("a.patient_id = @PatientId");
            }

            if (conditions.Any())
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }

            query += " ORDER BY a.time ASC";

            using var command = new MySqlCommand(query, connection);
            if (ApplyDoctorIdFilter())
            {
                command.Parameters.AddWithValue("@DoctorId", currentUser.Id);
            }
            if (patientId.HasValue)
            {
                command.Parameters.AddWithValue("@PatientId", patientId.Value);
            }
            AddQueryParameters(command);

            using var reader = command.ExecuteReader();
            Appointments.Clear();
            while (reader.Read())
            {
                var birthDate = reader.GetDateTime("birth_date");
                var age = CalculateAge(birthDate);

                var appointment = new AppointmentViewModel
                {
                    Id = reader.GetInt64("id"),
                    PatientId = reader.GetInt32("patient_id"),
                    DoctorId = reader.GetInt32("doctor_id"),
                    Time = reader.GetDateTime("time"),
                    Status = (AppointmentStatus)Enum.Parse(typeof(AppointmentStatus), reader.GetString("status")),
                    PatientName = $"{reader.GetString("first_name")} {reader.GetString("last_name")}",
                    PatientPhone = reader.GetInt64("phone"),
                    Gender = (Gender)Enum.Parse(typeof(Gender), reader.GetString("gender")),
                    Age = age
                };
                Appointments.Add(appointment);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load appointments: {ex.Message}";
            Appointments.Clear();
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

    protected class AppointmentViewModel
    {
        public long Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime Time { get; set; }
        public AppointmentStatus Status { get; set; }
        public string PatientName { get; set; }
        public long PatientPhone { get; set; }
        public Gender Gender { get; set; }
        public int Age { get; set; }

        public string FormattedPhone
        {
            get
            {
                // Format phone number as (XXX) XXX-XXXX
                var phoneStr = PatientPhone.ToString();
                if (string.IsNullOrEmpty(phoneStr) || phoneStr.Length != 10) return phoneStr;
                return $"({phoneStr.Substring(0, 3)}) {phoneStr.Substring(3, 3)}-{phoneStr.Substring(6, 4)}";
            }
        }
    }

    protected enum AppointmentStatus
    {
        Booked,
        Cancelled,
        CheckedIn,
        NoShow
    }
}