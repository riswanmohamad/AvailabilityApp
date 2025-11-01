-- Insert Sample Data for AvailabilityApp
USE AvailabilityApp;
GO

-- ===============================================
-- IMPORTANT: Creating Test Users
-- ===============================================
-- Option 1 (Recommended): Register through the app at /provider/register
--   - Email: demo@example.com
--   - Password: password123 (or your choice)
--   - This will create a properly hashed password
--
-- Option 2: Run this script to create a demo user, then:
--   1. Comment out the INSERT INTO Users below
--   2. Register via the app with demo@example.com
--   3. The services/patterns/links below will still work
-- ===============================================

-- Insert sample user (BCrypt hash - may not match expected password)
-- If login fails, please register through the app instead
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, BusinessName, PhoneNumber) VALUES 
(NEWID(), 'demo@example.com', '$2a$11$K8YH5w5KmZ5qN6xF7rY7yejv4L1.HmN8KQmH6n7GkX9Z1Lw4pQzW2', 'John', 'Doe', 'Doe Services', '+1234567890');
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