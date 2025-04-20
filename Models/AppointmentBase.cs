using HMS.Services;
using HMS.Models;
using HMS.Data;
using Microsoft.AspNetCore.Components;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class AppointmentBase : ComponentBase
{
    [Inject]
    protected UserStateService UserStateService { get; set; }

    [Inject]
    protected NavigationManager Navigation { get; set; }

    [Inject]
    protected DatabaseConnection DatabaseConnection { get; set; }

    protected string ErrorMessage { get; set; }
    protected List<string> TimeSlots { get; set; } = new List<string>();
    protected DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);
    protected string AppointmentTime { get; set; } = ""; // Default to empty for --Select Time--
    protected DateTime PreviousDate { get; set; }
    protected string PreviousTime { get; set; }

    // Abstract property: Subclasses specify the expected role
    protected abstract Role ExpectedRole { get; }

    // Abstract method: Subclasses specify the navigation target after booking
    protected abstract string SuccessNavigationUrl { get; }

    protected override void OnInitialized()
    {
        var currentUser = UserStateService.GetCurrentUser();
        if (currentUser == null || currentUser.Role != ExpectedRole)
        {
            Navigation.NavigateTo("/", forceLoad: true);
            return;
        }

        PreviousDate = AppointmentDate;
        PreviousTime = AppointmentTime;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        // Update PreviousDate and PreviousTime after render to ensure we detect changes
        PreviousDate = AppointmentDate;
        PreviousTime = AppointmentTime;
    }

    protected virtual void UpdateTimeSlots(int doctorId)
    {
        TimeSlots.Clear();

        var startTime = new TimeSpan(8, 0, 0);
        var endTime = new TimeSpan(17, 0, 0);
        var interval = TimeSpan.FromMinutes(15);

        var allTimeSlots = new List<string>();
        for (var time = startTime; time <= endTime; time = time.Add(interval))
        {
            allTimeSlots.Add(time.ToString(@"hh\:mm"));
        }

        try
        {
            using var connection = DatabaseConnection.GetConnection();
            connection.Open();

            string query = @"
                SELECT time
                FROM Appointments
                WHERE DATE(time) = @AppointmentDate
                AND doctor_id = @DoctorId
                AND status IN ('Booked', 'CheckedIn')"; // Ignore Cancelled status
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentDate", AppointmentDate.Date);
            command.Parameters.AddWithValue("@DoctorId", doctorId);

            var bookedTimes = new List<DateTime>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    bookedTimes.Add(reader.GetDateTime("time"));
                }
            }

            foreach (var timeStr in allTimeSlots)
            {
                if (TimeSpan.TryParse(timeStr, out var timeSpan))
                {
                    var slotDateTime = AppointmentDate.Date.Add(timeSpan);
                    if (!bookedTimes.Any(bt => bt == slotDateTime))
                    {
                        TimeSlots.Add(timeStr);
                    }
                }
            }

            if (!TimeSlots.Any())
            {
                ErrorMessage = "No available time slots for the selected date.";
            }

            
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load available time slots: {ex.Message}";
        }
    }

    protected async Task<bool> CheckAppointmentConflictAsync(int patientId, DateTime appointmentDateTime)
{
    try
    {
        using var connection = DatabaseConnection.GetConnection();
        await connection.OpenAsync();

        string conflictQuery = @"
                SELECT COUNT(*)
                FROM Appointments
                WHERE patient_id = @PatientId
                AND time = @Time
                AND status IN ('Booked', 'CheckedIn')";
        using var conflictCommand = new MySqlCommand(conflictQuery, connection);
        conflictCommand.Parameters.AddWithValue("@PatientId", patientId);
        conflictCommand.Parameters.AddWithValue("@Time", appointmentDateTime);
        long conflictCount = (long)await conflictCommand.ExecuteScalarAsync();
        return conflictCount > 0;
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Failed to check appointment conflict: {ex.Message}";
        return true; // Assume conflict on error to prevent booking
    }
}

protected async Task BookAppointmentAsync(int patientId, int doctorId, DateTime appointmentDateTime)
{
    try
    {
        // Check for conflicts
        if (await CheckAppointmentConflictAsync(patientId, appointmentDateTime))
        {
            ErrorMessage = "The patient already has an appointment at this time.";
            return;
        }

        using var connection = DatabaseConnection.GetConnection();
        await connection.OpenAsync();

        string insertQuery = @"
                INSERT INTO Appointments (patient_id, doctor_id, time, status)
                VALUES (@PatientId, @DoctorId, @Time, 'Booked')";
        using var command = new MySqlCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@PatientId", patientId);
        command.Parameters.AddWithValue("@DoctorId", doctorId);
        command.Parameters.AddWithValue("@Time", appointmentDateTime);

        await command.ExecuteNonQueryAsync();
        Navigation.NavigateTo(SuccessNavigationUrl, forceLoad: true);
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Failed to book appointment: {ex.Message}";
    }
}
}