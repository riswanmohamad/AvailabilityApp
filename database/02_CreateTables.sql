-- Create Tables for AvailabilityApp
USE AvailabilityApp;
GO

-- Users table (Service Providers)
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(255) NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    BusinessName NVARCHAR(255) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Services/Items table
CREATE TABLE Services (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Duration INT NULL, -- Duration in minutes
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Availability Patterns table
CREATE TABLE AvailabilityPatterns (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    SlotType NVARCHAR(20) NOT NULL, -- 'Minute', 'Hour', 'Day', 'Week', 'Month'
    SlotDuration INT NOT NULL, -- Duration of each slot in minutes
    StartTime TIME NULL, -- For time-based slots
    EndTime TIME NULL, -- For time-based slots
    DaysOfWeek NVARCHAR(20) NULL, -- Comma-separated: '1,2,3,4,5' for Mon-Fri
    StartDate DATE NOT NULL,
    EndDate DATE NULL, -- NULL means no end date (ongoing)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

-- Available Slots table (generated from patterns)
CREATE TABLE AvailableSlots (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    PatternId UNIQUEIDENTIFIER NOT NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    SlotType NVARCHAR(20) NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE NO ACTION,
    FOREIGN KEY (PatternId) REFERENCES AvailabilityPatterns(Id) ON DELETE CASCADE
);

-- Exceptions table (unavailability overrides)
CREATE TABLE Exceptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    ExceptionType NVARCHAR(20) NOT NULL, -- 'Unavailable', 'Holiday', 'Break', 'Maintenance'
    RecurringYearly BIT NOT NULL DEFAULT 0, -- For annual holidays
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

-- Sharable Links table
CREATE TABLE SharableLinks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(100) NOT NULL UNIQUE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

-- Service Images table
CREATE TABLE ServiceImages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    BlobName NVARCHAR(255) NOT NULL, -- Unique blob name in Azure Storage
    OriginalFileName NVARCHAR(255) NOT NULL, -- Original file name from upload
    ContentType NVARCHAR(100) NOT NULL, -- MIME type (image/jpeg, image/png, etc.)
    FileSize BIGINT NOT NULL, -- File size in bytes
    DisplayOrder INT NOT NULL DEFAULT 0, -- Order for displaying multiple images
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IX_Services_UserId ON Services(UserId);
CREATE INDEX IX_AvailabilityPatterns_ServiceId ON AvailabilityPatterns(ServiceId);
CREATE INDEX IX_AvailableSlots_ServiceId ON AvailableSlots(ServiceId);
CREATE INDEX IX_AvailableSlots_StartDateTime ON AvailableSlots(StartDateTime);
CREATE INDEX IX_Exceptions_ServiceId ON Exceptions(ServiceId);
CREATE INDEX IX_Exceptions_StartDateTime ON Exceptions(StartDateTime);
CREATE INDEX IX_SharableLinks_Token ON SharableLinks(Token);
CREATE INDEX IX_SharableLinks_ServiceId ON SharableLinks(ServiceId);
CREATE INDEX IX_ServiceImages_ServiceId ON ServiceImages(ServiceId);
CREATE INDEX IX_ServiceImages_DisplayOrder ON ServiceImages(ServiceId, DisplayOrder);

GO