USE master;
GO

IF DB_ID(N'BookingDDD') IS NULL
BEGIN
    CREATE DATABASE BookingDDD;
END;
GO

USE BookingDDD;
GO

IF OBJECT_ID(N'dbo.Resources', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Resources
    (
        Id uniqueidentifier NOT NULL,
        Name nvarchar(200) NOT NULL,
        OpensAtHour tinyint NOT NULL,
        ClosesAtHour tinyint NOT NULL,
        CONSTRAINT PK_Resources PRIMARY KEY (Id),
        CONSTRAINT CK_Resources_OpeningHours CHECK
            (OpensAtHour < ClosesAtHour AND ClosesAtHour <= 23)
    );
END;
GO

IF OBJECT_ID(N'dbo.Bookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bookings
    (
        Id uniqueidentifier NOT NULL,
        ResourceId uniqueidentifier NOT NULL,
        StartTime datetime2(0) NOT NULL,
        EndTime datetime2(0) NOT NULL,
        Status tinyint NOT NULL,
        CONSTRAINT PK_Bookings PRIMARY KEY (Id),
        CONSTRAINT FK_Bookings_Resources FOREIGN KEY (ResourceId)
            REFERENCES dbo.Resources(Id),
        CONSTRAINT CK_Bookings_Period CHECK (StartTime < EndTime),
        CONSTRAINT CK_Bookings_Status CHECK (Status IN (1, 2))
    );

    CREATE INDEX IX_Bookings_ResourceId_StartTime
        ON dbo.Bookings(ResourceId, StartTime)
        INCLUDE (EndTime, Status);
END;
GO

IF OBJECT_ID(N'dbo.AuditLogEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogEntries
    (
        Id uniqueidentifier NOT NULL,
        EventName nvarchar(100) NOT NULL,
        BookingId uniqueidentifier NOT NULL,
        ResourceId uniqueidentifier NOT NULL,
        OccurredAtUtc datetime2(3) NOT NULL,
        CONSTRAINT PK_AuditLogEntries PRIMARY KEY (Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.CalendarEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CalendarEntries
    (
        BookingId uniqueidentifier NOT NULL,
        ResourceId uniqueidentifier NOT NULL,
        StartTime datetime2(0) NOT NULL,
        EndTime datetime2(0) NOT NULL,
        CONSTRAINT PK_CalendarEntries PRIMARY KEY (BookingId),
        CONSTRAINT CK_CalendarEntries_Period CHECK (StartTime < EndTime)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Resources
    WHERE Id = '00000000-0000-0000-0000-000000000000'
)
BEGIN
    INSERT INTO dbo.Resources (Id, Name, OpensAtHour, ClosesAtHour)
    VALUES
    (
        '00000000-0000-0000-0000-000000000000',
        N'Meeting room A',
        8,
        16
    );
END;
GO
