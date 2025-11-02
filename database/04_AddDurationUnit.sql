-- Migration: Add DurationUnit column to Services table
-- This allows storing the unit type (minutes, hours, days, months) along with the duration value

USE AvailabilityAppDB;
GO

-- Add DurationUnit column
ALTER TABLE Services
ADD DurationUnit NVARCHAR(20) NULL;
GO

-- Set default value for existing records (assume minutes)
UPDATE Services
SET DurationUnit = 'minutes'
WHERE DurationUnit IS NULL;
GO

-- Make DurationUnit NOT NULL after setting defaults
ALTER TABLE Services
ALTER COLUMN DurationUnit NVARCHAR(20) NOT NULL;
GO

-- Add check constraint to ensure valid duration units
ALTER TABLE Services
ADD CONSTRAINT CK_Services_DurationUnit 
CHECK (DurationUnit IN ('minutes', 'hours', 'days', 'months'));
GO

PRINT 'Successfully added DurationUnit column to Services table';
