-- Insert Sample Data for AvailabilityApp
USE AvailabilityApp;
GO

-- Insert sample user
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, BusinessName, PhoneNumber) VALUES 
(NEWID(), 'demo@example.com', '$2a$11$rOjNbvmrT0QNVr9ZXdNzKeJdSQpjt0XmYHvxT8ZMJE4VQ1cNvVzGC', 'John', 'Doe', 'Doe Services', '+1234567890');
-- Password is 'password123' (hashed with bcrypt)

DECLARE @UserId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'demo@example.com');

-- Insert sample service
DECLARE @ServiceId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Services (Id, UserId, Title, Description, Duration) VALUES 
(@ServiceId, @UserId, 'Personal Training Session', 'One-on-one fitness training session with certified trainer', 60);

-- Insert availability pattern (Monday to Friday, 9 AM to 5 PM, hourly slots)
DECLARE @PatternId UNIQUEIDENTIFIER = NEWID();
INSERT INTO AvailabilityPatterns (Id, ServiceId, SlotType, SlotDuration, StartTime, EndTime, DaysOfWeek, StartDate) VALUES 
(@PatternId, @ServiceId, 'Hour', 60, '09:00:00', '17:00:00', '1,2,3,4,5', CAST(GETDATE() AS DATE));

-- Insert some exceptions (lunch breaks)
INSERT INTO Exceptions (ServiceId, Title, Description, StartDateTime, EndDateTime, ExceptionType) VALUES 
(@ServiceId, 'Lunch Break', 'Daily lunch break', DATEADD(day, 0, CAST(CAST(GETDATE() AS DATE) AS DATETIME2) + CAST('12:00:00' AS TIME)), DATEADD(day, 0, CAST(CAST(GETDATE() AS DATE) AS DATETIME2) + CAST('13:00:00' AS TIME)), 'Break'),
(@ServiceId, 'Lunch Break', 'Daily lunch break', DATEADD(day, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2) + CAST('12:00:00' AS TIME)), DATEADD(day, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2) + CAST('13:00:00' AS TIME)), 'Break');

-- Insert sharable link
INSERT INTO SharableLinks (ServiceId, Token) VALUES 
(@ServiceId, REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''));

GO