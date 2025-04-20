-- Create database if not exists
CREATE DATABASE IF NOT EXISTS hms;
USE hms;

-- Disable foreign key checks to allow dropping tables with data
SET FOREIGN_KEY_CHECKS=0;

-- Drop existing tables
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Patients;
DROP TABLE IF EXISTS Doctors;
DROP TABLE IF EXISTS Nurses;
DROP TABLE IF EXISTS Cashiers;
DROP TABLE IF EXISTS Admins;
DROP TABLE IF EXISTS VisitRecords;
DROP TABLE IF EXISTS Appointments;
DROP TABLE IF EXISTS Payments;
DROP TABLE IF EXISTS PaymentItems;
DROP TABLE IF EXISTS Departments;
DROP TABLE IF EXISTS Prescriptions;
DROP TABLE IF EXISTS Medicines;
DROP TABLE IF EXISTS Tests;
DROP TABLE IF EXISTS Surgeries;
DROP TABLE IF EXISTS Equipment;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS=1;

-- Users table
CREATE TABLE Users (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL CHECK (email REGEXP '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'),
    phone BIGINT NOT NULL CHECK (phone BETWEEN 1000000000 AND 9999999999),
    password VARCHAR(100) NOT NULL,
    gender ENUM('Male', 'Female') NOT NULL,
    birth_date DATE NOT NULL,
    role ENUM('Patient', 'Doctor', 'Nurse', 'Cashier', 'Admin') NOT NULL,
    status ENUM('Active', 'Inactive') NOT NULL DEFAULT 'Active'
);

-- Set id starting value
ALTER TABLE Users AUTO_INCREMENT = 198906041;

-- Trigger for Users.id (9-digit constraint)
DELIMITER //
CREATE TRIGGER before_insert_users_id
BEFORE INSERT ON Users
FOR EACH ROW
BEGIN
    IF NEW.id < 100000000 OR NEW.id > 999999999 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'User ID must be a 9-digit number';
    END IF;
END //
DELIMITER ;

-- Patients table
CREATE TABLE Patients (
    user_id INT UNSIGNED PRIMARY KEY,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

-- Doctors table
CREATE TABLE Doctors (
    user_id INT UNSIGNED PRIMARY KEY,
    specialty VARCHAR(50),
    level ENUM('Intern', 'Assistant', 'Junior', 'General', 'Senior', 'Lead', 'Chief'),
    hire_date DATE NOT NULL,
    termination_date DATE,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

-- Nurses table
CREATE TABLE Nurses (
    user_id INT UNSIGNED PRIMARY KEY,
    level ENUM('Intern', 'Assistant', 'Junior', 'General', 'Senior', 'Lead', 'Chief'),
    hire_date DATE NOT NULL,
    termination_date DATE,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

-- Cashiers table
CREATE TABLE Cashiers (
    user_id INT UNSIGNED PRIMARY KEY,
    hire_date DATE NOT NULL,
    termination_date DATE,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

-- Admins table
CREATE TABLE Admins (
    user_id INT UNSIGNED PRIMARY KEY,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

-- VisitRecords table
CREATE TABLE VisitRecords (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    patient_id INT UNSIGNED NOT NULL,
    visit_date DATETIME NOT NULL,
    symptoms TEXT NOT NULL,
    examinations TEXT,
    diagnosis TEXT NOT NULL,
    disease_type VARCHAR(50) NOT NULL,
    doctor_id INT UNSIGNED NOT NULL,
    FOREIGN KEY (patient_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (doctor_id) REFERENCES Users(id) ON DELETE CASCADE,
    INDEX idx_disease_type (disease_type)
);

-- Set id starting value
ALTER TABLE VisitRecords AUTO_INCREMENT = 6489648964;

-- Appointments table
CREATE TABLE Appointments (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    patient_id INT UNSIGNED NOT NULL,
    doctor_id INT UNSIGNED NOT NULL,
    time DATETIME NOT NULL,
    status ENUM('Booked', 'Cancelled', 'CheckedIn', 'NoShow') DEFAULT 'Booked',
    INDEX idx_time (time),
    FOREIGN KEY (patient_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (doctor_id) REFERENCES Users(id) ON DELETE CASCADE
);

-- Set id starting value
ALTER TABLE Appointments AUTO_INCREMENT = 2025416345;

-- Payments table
CREATE TABLE Payments (
    id INT PRIMARY KEY AUTO_INCREMENT,
    patient_id INT UNSIGNED NOT NULL,
    visit_id BIGINT NOT NULL,
    amount DECIMAL(10,2) NOT NULL CHECK (amount >= 0),
    status ENUM('Unpaid', 'Paid') DEFAULT 'Unpaid',
    visit_type ENUM('Outpatient', 'Inpatient') NOT NULL,
    created_date DATE NOT NULL,
    payment_completion_date DATE,
    FOREIGN KEY (patient_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (visit_id) REFERENCES VisitRecords(id) ON DELETE CASCADE
);

-- PaymentItems table
CREATE TABLE PaymentItems (
    id INT PRIMARY KEY AUTO_INCREMENT,
    payment_id INT NOT NULL,
    type ENUM('Medicine', 'Test', 'Surgery', 'Equipment', 'Appointment') NOT NULL,
    description VARCHAR(200) NOT NULL,
    quantity INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    amount DECIMAL(10,2) NOT NULL CHECK (amount >= 0),
    FOREIGN KEY (payment_id) REFERENCES Payments(id) ON DELETE CASCADE
);

-- Departments table
CREATE TABLE Departments (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(50) NOT NULL,
    description TEXT,
    establishment_date DATE NOT NULL,
    closure_date DATE
);

-- Prescriptions table (added name, quantity, price)
CREATE TABLE Prescriptions (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    visit_id BIGINT NOT NULL,
    type ENUM('Medicine', 'Tests', 'Surgery', 'Equipment') NOT NULL,
    name VARCHAR(100) NOT NULL,
    quantity INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    FOREIGN KEY (visit_id) REFERENCES VisitRecords(id) ON DELETE CASCADE
);

-- Medicines table (removed prescription_id, made name unique)
CREATE TABLE Medicines (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL UNIQUE,
    quantity INT NOT NULL DEFAULT 999 CHECK (quantity > 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0)
);

-- Tests table (removed prescription_id, made name unique)
CREATE TABLE Tests (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL UNIQUE,
    quantity INT NOT NULL DEFAULT 999 CHECK (quantity > 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0)
);

-- Equipment table (removed prescription_id, made name unique)
CREATE TABLE Equipment (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL UNIQUE,
    quantity INT NOT NULL DEFAULT 999 CHECK (quantity > 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0)
);

-- Surgeries table (removed prescription_id, made name unique)
CREATE TABLE Surgeries (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL UNIQUE,
    quantity INT NOT NULL DEFAULT 999 CHECK (quantity > 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0)
);