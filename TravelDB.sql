-- =============================================================
--  Travel Manager - SQL Server database
--  You do not have to run this file: the app creates the database
--  and tables by itself on first start. It is here for SSMS users
--  and for the project report.
-- =============================================================

IF DB_ID(N'TravelDB') IS NULL
    CREATE DATABASE TravelDB;
GO

USE TravelDB;
GO

-- Registered users (password is stored as a SHA-256 hash)
IF OBJECT_ID(N'dbo.Signup', N'U') IS NULL
CREATE TABLE dbo.Signup
(
    ID        INT IDENTITY(1,1) PRIMARY KEY,
    Name      NVARCHAR(50)  NOT NULL UNIQUE,
    Email     NVARCHAR(100) NOT NULL UNIQUE,
    Address   NVARCHAR(255) NOT NULL,
    ContactNo NVARCHAR(15)  NOT NULL,
    Password  NVARCHAR(255) NOT NULL
);
GO

-- Travel packages
IF OBJECT_ID(N'dbo.Package', N'U') IS NULL
CREATE TABLE dbo.Package
(
    PID          INT IDENTITY(1,1) PRIMARY KEY,
    PName        NVARCHAR(255) NOT NULL,
    PDescription NVARCHAR(255) NOT NULL,
    PPrice       INT NOT NULL CHECK (PPrice > 0),
    PDuration    NVARCHAR(255) NOT NULL,
    PDestination NVARCHAR(255)
);
GO

-- Bookings (BPackage holds the package name)
IF OBJECT_ID(N'dbo.Booking', N'U') IS NULL
CREATE TABLE dbo.Booking
(
    B_ID          INT IDENTITY(1,1) PRIMARY KEY,
    BPackage      NVARCHAR(255) NOT NULL,
    CustomerName  NVARCHAR(255) NOT NULL,
    BookingDate   DATETIME NOT NULL DEFAULT GETDATE(),
    BookingStatus NVARCHAR(255) NOT NULL,
    TravelDate    DATETIME NOT NULL,
    NoOfPeople    INT NOT NULL CHECK (NoOfPeople > 0)
);
GO

-- Payments for a booking (deleted together with the booking)
IF OBJECT_ID(N'dbo.Payment', N'U') IS NULL
CREATE TABLE dbo.Payment
(
    PI_ID         INT IDENTITY(1,1) PRIMARY KEY,
    BookingID     INT NOT NULL REFERENCES dbo.Booking(B_ID) ON DELETE CASCADE,
    PaymentAmount INT NOT NULL CHECK (PaymentAmount > 0),
    PaymentDate   DATE NOT NULL DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(255) NOT NULL,
    PaymentStatus NVARCHAR(255) NOT NULL
);
GO

-- Starter packages
IF NOT EXISTS (SELECT 1 FROM dbo.Package)
INSERT INTO dbo.Package (PName, PDescription, PPrice, PDuration, PDestination) VALUES
 (N'Beach Getaway',    N'Sun, sea and relaxing beach resorts',  150000, N'7 Days',  N'Dubai, United Arab Emirates'),
 (N'Adventure Tours',  N'Hiking, camping and mountain views',    85000, N'10 Days', N'Hunza, Pakistan'),
 (N'Romantic Escapes', N'Candle-light dinners and city walks',  250000, N'7 Days',  N'Paris, France'),
 (N'Family Vacation',  N'Fun-filled trip for the whole family',  60000, N'3 Days',  N'Murree, Pakistan'),
 (N'City Break',       N'Historic sights, food and shopping',   180000, N'7 Days',  N'Istanbul, Turkey');
GO
