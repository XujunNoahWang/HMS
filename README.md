# Hospital Management System (HMS) - README

## Project Overview

The Hospital Management System (HMS) is a desktop application built with .NET MAUI, designed for medium-sized hospitals (NOT clinics) to streamline operations and enhance healthcare delivery. It supports five user roles: patients, doctors, nurses, cashiers, and administrators, offering features like user authentication, medical records and prescription management, appointment scheduling, billing, and administrative tasks. HMS uses the Blazor framework for the frontend, C# for backend logic, and a MySQL database for centralized data management, leveraging object-oriented programming for maintainability.

The system has been tested with approximately 2,000 records (193 users, 10 departments, 500 medical records, 300 appointments, etc.), capable of handling the daily operations of a medium-sized hospital, such as managing 100 patients and 80 healthcare staff.

## Project Motivation

HMS was developed to tackle the challenges of complex system design, as hospital management involves multi-role interactions and zero-tolerance data accuracy. By building HMS, the developer, I, aimed to master .NET MAUI, C#, MySQL database integration, and object-oriented programming, delivering an efficient solution for medium-sized hospitals. This project was chosen for its high complexity, surpassing simpler systems (e.g., payroll or employee management), to demonstrate technical expertise and real-world problem-solving capabilities.

## Functionality Description

HMS provides the following core features, optimized for different user roles:

1. **User Authentication and Role Management**:

   - Supports five roles: Patient, Doctor, Nurse, Cashier, and Administrator.
   - Users log in with a 9-digit ID and password (e.g., Doctor ID `198906061`, Password `Password123`), validated by `AuthService` and managed by `UserStateService`.
   - Role-based permissions ensure data security, e.g., administrators access staff management, patients view personal records.

2. **Medical Records and Prescription Management**:

   - Doctors create and edit medical records (`NewRecord.razor`, stored in `VisitRecords` table), including symptoms, examinations, diagnoses, and prescriptions (`Prescriptions` table, supporting medicine, tests, surgery, equipment); nurses have read-only access (`NursePatients.razor`); patients view their records (`PatientRecords.razor`).
   - Prescriptions automatically deduct inventory (`Medicines`, `Tests`, `Surgeries`, `Equipment` tables) and generate bills (`Payments`, `PaymentItems` tables).
   - Example: A doctor searches a patient ID, adds a record with a medication prescription, the system updates inventory and creates an unpaid bill, and the patient views the record.

3. **Appointment Scheduling**:

   - Patients and doctors create appointments (`NewAppointment.razor`, `PatientAddAppointment.razor`), selecting 15-minute slots from 8:00 to 17:00, with automatic conflict checks.
   - Nurses view all appointments (`NurseSchedule.razor`), doctors manage their schedules (`DoctorSchedule.razor`).
   - Example: A patient selects a doctor and time, submits, and receives confirmation; the doctor sees the new appointment.

4. **Billing and Payments**:

   - Cashiers view unpaid bills via `CashierPayments.razor`, confirming payments to update status (`Payments` table).
   - Bills are generated from prescriptions, detailing item type, description, quantity, and amount.
   - Example: A cashier selects a patient¡¯s bill, confirms payment, and marks it as Paid.

5. **Administrative Management**:

   - Administrators add or edit staff (doctors, nurses, cashiers) via `AddStaff.razor` and `editStaff.razor`, managing departments (`Departments`), levels, and personal details.
   - Departments are managed via `AddDepartment.razor` and `EditDepartment.razor`, supporting closure.
   - Features password reset and staff dismissal (updates `Users` and role tables).
   - Example: An administrator adds a new doctor, assigns a department and level, and updates the database.

## Database Structure

HMS uses a MySQL database with 16 tables, supporting role management, medical records, and inventory:

- **User-Related**: `Users` (193 records, stores all user info), `Patients` (100), `Doctors` (30), `Nurses` (50), `Cashiers` (10), `Admins` (3) linked via foreign keys.
- **Medical Records**: `VisitRecords` (500, stores medical records), `Prescriptions` (302, stores prescriptions).
- **Inventory Management**: `Medicines`, `Tests`, `Surgeries`, `Equipment` (30 each, store inventory and prices).
- **Appointments**: `Appointments` (300, supports time and status management).
- **Payments**: `Payments` (171, stores bills), `PaymentItems` (250, stores bill details).
- **Departments**: `Departments` (10, stores department info).

Foreign key constraints (`ON DELETE CASCADE`) and triggers (enforcing 9-digit IDs) ensure data integrity, with indexes for query performance.

## Problems Solved

HMS addresses the following key challenges in medium-sized hospitals:

1. **Inefficient Manual Processes**:

   - Replaces paper-based medical cards and manual scheduling, reducing errors and speeding up data access.
   - Automates record creation, prescription generation, and billing, easing administrative loads.

2. **Role-Specific Needs**:

   - Provides tailored interfaces, e.g., doctors edit records (`NewRecord.razor`), cashiers handle payments (`CashierPayments.razor`).
   - Permissions restrict access to authorized functions.

3. **Data Centralization**:

   - MySQL database centralizes \~2,000 records, enabling quick retrieval of records, appointments, and bills.
   - Foreign keys and triggers maintain data consistency.

4. **Time Management**:

   - Automatic conflict checks (`AppointmentBase.cs`) optimize scheduling for doctors and patients.
   - Schedule views (`DoctorSchedule.razor`, `NurseSchedule.razor`) aid daily task management.

5. **Inventory Management Efficiency**:

   - Prescriptions auto-deduct inventory (`RecordService.cs`), preventing overselling.
   - Inventory tables (`Medicines`, etc.) support real-time updates.

6. **Data Integrity**:

   - Transactions (`RecordService.cs`, `editStaff.razor`) ensure atomic database operations.
   - Input validation and parameterized queries prevent errors and SQL injection.

## Setup Instructions

To run the HMS project, follow these steps:

1. **Environment Requirements**:

   - .NET 8.0 or higher
   - MySQL 8.0 or higher
   - Visual Studio or an IDE supporting MAUI

2. **Database Configuration**:

   - Create a MySQL database named `hms` and run `HMS_CREATETABLE.sql` to create 16 tables.
   - Update the connection string in `DatabaseConnection.cs` (`Server`, `UserID`, `Password`).
   - Import test data (\~2,000 records) or use default data (193 users, 10 departments, etc.).

3. **Project Setup**:

   - Clone the repository, containing 40 files.
   - Verify service registration in `MauiProgram.cs` (`DatabaseConnection`, `IAuthService`, `UserStateService`, `RecordService`).
   - Run the project using `dotnet run` or an IDE.

4. **Test Login**:

   - Use default credentials stored in the `Users` table:
     - Doctor: ID `198906061`, Password `Password123`
     - Nurse: ID `198906077`, Password `Password123`
     - Cashier: ID `198906126`, Password `Password123`
     - Administrator: ID `198906042`, Password `Password123`
     - Patient: ID `198906149`, Password `Password123`

## Conclusion

HMS is a robust hospital management system that enhances medium-sized hospital efficiency through automation, role-specific features, and data centralization. Supporting \~2,000 records, it handles operations for 100 patients and 80 staff, addressing manual processes, data fragmentation, inventory management, and data integrity issues. Built with .NET MAUI, Blazor, and MySQL, HMS offers a modern, reliable platform for hospital staff and patients.