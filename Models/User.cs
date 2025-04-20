namespace HMS.Models
{
    // This file contains the User class and its derived classes for different roles in the system.
    // Each class has properties and methods specific to that role.
    // I did not create sepate files for each child class to keep the code organized and easy to manage.
    public enum Role { Patient, Doctor, Nurse, Cashier, Admin }
    public enum Status { Active, Inactive }
    public enum Gender { Male, Female }
    // I used enum instead of string, so that the app can use the enum values in the database and avoid string comparison issues.

    // User info in common
    public abstract class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public long Phone { get; set; }
        public string Password { get; set; }
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public Role Role { get; set; }
        public Status Status { get; set; }

        // Abstract methods to be implemented by derived classes
        public abstract string GetDashboardRoute();
        public abstract bool CanAccessPatientRecords();
        public abstract string GetRoleDisplayName();
    }

    public class Patient : User
    {
        public Patient()
        {
            Role = Role.Patient;
        }

        public override string GetDashboardRoute() => "/dashboard/Patient";
        public override bool CanAccessPatientRecords() => true; 
        public override string GetRoleDisplayName() => "Patient";
    }

    // There are some lambdas in the following classes to return simple values, the code looks cleaner and more readable. :)
    public class Doctor : User
    {
        public string Specialty { get; set; }
        public string Level { get; set; }
        public DateTime HireDate { get; set; }

        public Doctor()
        {
            Role = Role.Doctor;
        }

        public override string GetDashboardRoute() => "/dashboard/Doctor";
        public override bool CanAccessPatientRecords() => true; 
        public override string GetRoleDisplayName() => "Doctor";
    }

    public class Nurse : User
    {
        public string Level { get; set; }
        public DateTime HireDate { get; set; }

        public Nurse()
        {
            Role = Role.Nurse;
        }

        public override string GetDashboardRoute() => "/dashboard/Nurse";
        public override bool CanAccessPatientRecords() => true; 
        public override string GetRoleDisplayName() => "Nurse";
    }

    public class Cashier : User
    {
        public DateTime HireDate { get; set; }

        public Cashier()
        {
            Role = Role.Cashier;
        }

        public override string GetDashboardRoute() => "/dashboard/Cashier";
        public override bool CanAccessPatientRecords() => false; 
        public override string GetRoleDisplayName() => "Cashier";
    }

    public class Admin : User
    {
        public Admin()
        {
            Role = Role.Admin;
        }

        public override string GetDashboardRoute() => "/dashboard/Admin";
        public override bool CanAccessPatientRecords() => false; 
        public override string GetRoleDisplayName() => "Admin";
    }
}