-- ========================================================
-- RACEDAY DATABASE SCHEMATIC (PART 1)
-- Target DBMS: Microsoft SQL Server (SSMS-ready)
-- This script initializes the database container, builds tables,
-- establishes relationships, and populates initial seed data.
-- ========================================================

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO
-- User table houses account records for Organisers and Participants [5, 6].
-- Role column is strictly validated via CK_User_Role check constraint [7, 8].
-- Email addresses are strictly enforced as UNIQUE to prevent duplicate logins [9, 10].

CREATE TABLE [User]
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,

    Email NVARCHAR(255) NOT NULL UNIQUE,

    PasswordHash NVARCHAR(255) NOT NULL,

    FirstName NVARCHAR(100) NOT NULL,

    LastName NVARCHAR(100) NOT NULL,

    Role NVARCHAR(20) NOT NULL,

    Phone NVARCHAR(20) NULL,

    DateOfBirth DATE NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_User_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO

-- Event table models core race/walk/cycle events created by Organisers [5, 6].
-- Foreign key FK_Event_Organiser maps back to the User table [11-13].
-- RegistrationDeadline must precede or match EventDate via validation check [9, 14].

CREATE TABLE Event
(
    EventId INT IDENTITY(1,1) PRIMARY KEY,

    OrganiserId INT NOT NULL,

    Name NVARCHAR(200) NOT NULL,

    Description NVARCHAR(MAX) NULL,

    Location NVARCHAR(300) NOT NULL,

    EventDate DATETIME2 NOT NULL,

    RegistrationDeadline DATETIME2 NOT NULL,

    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Event_Organiser
        FOREIGN KEY (OrganiserId)
        REFERENCES [User](UserId),

    CONSTRAINT CK_Event_Status
        CHECK (Status IN ('Draft', 'Published', 'Closed', 'Completed')),

    CONSTRAINT CK_Event_RegistrationDeadline
        CHECK (RegistrationDeadline <= EventDate)
);
GO

/*
========================================================
3. CATEGORY TABLE
Stores race categories belonging to an event
========================================================
*/


CREATE TABLE Category
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,

    EventId INT NOT NULL,

    Name NVARCHAR(100) NOT NULL,

    DistanceKm DECIMAL(6,2) NOT NULL,

    MaxParticipants INT NULL,

    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,

    StartTime TIME NOT NULL,

    CONSTRAINT FK_Category_Event
        FOREIGN KEY (EventId)
        REFERENCES Event(EventId),

    CONSTRAINT UQ_Category_Event_Name
        UNIQUE (EventId, Name),

    CONSTRAINT CK_Category_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Category_MaxParticipants
        CHECK (MaxParticipants IS NULL OR MaxParticipants > 0),

    CONSTRAINT CK_Category_EntryFee
        CHECK (EntryFee >= 0)
);
GO


/*
========================================================
4. ENROLMENT TABLE
Stores participant registrations for race categories
========================================================
*/


CREATE TABLE Enrolment
(
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,

    ParticipantId INT NOT NULL,

    CategoryId INT NOT NULL,

    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',

    BibNumber INT NOT NULL,

    EmergencyContact NVARCHAR(100) NULL,

    MedicalNotes NVARCHAR(500) NULL,

    CONSTRAINT FK_Enrolment_Participant
        FOREIGN KEY (ParticipantId)
        REFERENCES [User](UserId),

    CONSTRAINT FK_Enrolment_Category
        FOREIGN KEY (CategoryId)
        REFERENCES Category(CategoryId),

    CONSTRAINT CK_Enrolment_Status
        CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')),

    CONSTRAINT CK_Enrolment_BibNumber
        CHECK (BibNumber > 0),

    CONSTRAINT UQ_Enrolment_BibNumber
        UNIQUE (BibNumber)
);
GO

-- Result holds finish times and positions captured by an Organiser [5, 6].
-- UQ_Result_Enrolment enforces a 1:0..1 relationship with the Enrolment table [12, 18].
-- Status values are restricted to 'Finished', 'DNF', or 'DNS' via CK_Result_Status [19].


CREATE TABLE Result
(
    ResultId INT IDENTITY(1,1) PRIMARY KEY,

    EnrolmentId INT NOT NULL,

    FinishTime TIME NULL,

    PositionOverall INT NULL,

    PositionCategory INT NULL,

    Status NVARCHAR(20) NOT NULL DEFAULT 'Finished',

    CapturedBy INT NOT NULL,

    CapturedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    Notes NVARCHAR(300) NULL,

    CONSTRAINT FK_Result_Enrolment
        FOREIGN KEY (EnrolmentId)
        REFERENCES Enrolment(EnrolmentId),

    CONSTRAINT FK_Result_CapturedBy
        FOREIGN KEY (CapturedBy)
        REFERENCES [User](UserId),

    CONSTRAINT UQ_Result_Enrolment
        UNIQUE (EnrolmentId),

    CONSTRAINT CK_Result_Status
        CHECK (Status IN ('Finished', 'DNF', 'DNS')),

    CONSTRAINT CK_Result_PositionOverall
        CHECK (PositionOverall IS NULL OR PositionOverall > 0),

    CONSTRAINT CK_Result_PositionCategory
        CHECK (PositionCategory IS NULL OR PositionCategory > 0)
);
GO

/*
========================================================
6. EVENT IMAGE TABLE
Stores images associated with RaceDay events
========================================================
*/

CREATE TABLE EventImage
(
    ImageId INT IDENTITY(1,1) PRIMARY KEY,

    EventId INT NOT NULL,

    BlobPath NVARCHAR(500) NOT NULL,

    FileName NVARCHAR(255) NOT NULL,

    Caption NVARCHAR(200) NULL,

    IsPrimary BIT NOT NULL DEFAULT 0,

    ContentType NVARCHAR(100) NULL,

    UploadedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_EventImage_Event
        FOREIGN KEY (EventId)
        REFERENCES Event(EventId)
);
GO

/*
========================================================
7. SEED DATA - USERS
Adds sample organisers and participants
========================================================
*/

INSERT INTO [User]
    (Email, PasswordHash, FirstName, LastName, Role, Phone, DateOfBirth)
VALUES
    ('organiser1@raceday.com', 'DEMO_HASH_ORGANISER_001',
     'Thabo', 'Mokoena', 'Organiser', '0711111111', '1985-04-12'),

    ('organiser2@raceday.com', 'DEMO_HASH_ORGANISER_002',
     'Lerato', 'Molefe', 'Organiser', '0722222222', '1990-08-25'),

    ('participant1@raceday.com', 'DEMO_HASH_PARTICIPANT_001',
     'Keabetswe', 'Moilwa', 'Participant', '0733333333', '2002-06-15'),

    ('participant2@raceday.com', 'DEMO_HASH_PARTICIPANT_002',
     'Naledi', 'Mokoena', 'Participant', '0744444444', '2001-11-20');
GO

/*
========================================================
8. SEED DATA - EVENTS
Adds three sample RaceDay events
========================================================
*/

INSERT INTO Event
    (OrganiserId, Name, Description, Location,
     EventDate, RegistrationDeadline, Status)
VALUES
    (
        (SELECT UserId
         FROM [User]
         WHERE Email = 'organiser1@raceday.com'),

        'Pretoria City Marathon',

        'A major road running event through Pretoria.',

        'Pretoria, Gauteng',

        '2026-10-18 07:00:00',

        '2026-10-10 23:59:59',

        'Published'
    ),

    (
        (SELECT UserId
         FROM [User]
         WHERE Email = 'organiser2@raceday.com'),

        'Hartbeespoort Dam Run',

        'A scenic running event around Hartbeespoort Dam.',

        'Hartbeespoort, North West',

        '2026-11-08 06:30:00',

        '2026-11-01 23:59:59',

        'Published'
    ),

    (
        (SELECT UserId
         FROM [User]
         WHERE Email = 'organiser1@raceday.com'),

        'Pretoria Spring Fun Run',

        'A community fun run suitable for runners of different abilities.',

        'Pretoria, Gauteng',

        '2026-11-29 08:00:00',

        '2026-11-22 23:59:59',

        'Draft'
    );
GO

/*
========================================================
9. SEED DATA - CATEGORIES
Adds race categories for each event
========================================================
*/

INSERT INTO Category
    (EventId, Name, DistanceKm, MaxParticipants, EntryFee, StartTime)
VALUES

    (
        (SELECT EventId
         FROM Event
         WHERE Name = 'Pretoria City Marathon'),

        '10 KM',
        10.00,
        500,
        150.00,
        '07:30:00'
    ),

    (
        (SELECT EventId
         FROM Event
         WHERE Name = 'Pretoria City Marathon'),

        '21 KM',
        21.00,
        800,
        250.00,
        '07:00:00'
    ),

    (
        (SELECT EventId
         FROM Event
         WHERE Name = 'Hartbeespoort Dam Run'),

        '5 KM',
        5.00,
        300,
        100.00,
        '07:00:00'
    ),

    (
        (SELECT EventId
         FROM Event
         WHERE Name = 'Hartbeespoort Dam Run'),

        '10 KM',
        10.00,
        500,
        150.00,
        '06:30:00'
    ),

    (
        (SELECT EventId
         FROM Event
         WHERE Name = 'Pretoria Spring Fun Run'),

        '5 KM',
        5.00,
        400,
        80.00,
        '08:30:00'
    ),

    (
        (SELECT EventId
         FROM Event
         WHERE Name = 'Pretoria Spring Fun Run'),

        '10 KM',
        10.00,
        500,
        120.00,
        '08:00:00'
    );
GO


/*
========================================================
10. SEED DATA - ENROLMENTS
Adds sample participant registrations
========================================================
*/

INSERT INTO Enrolment
    (ParticipantId, CategoryId, Status, BibNumber,
     EmergencyContact, MedicalNotes)
VALUES

    (
        (SELECT UserId
         FROM [User]
         WHERE Email = 'participant1@raceday.com'),

        (SELECT CategoryId
         FROM Category c
         INNER JOIN Event e
             ON c.EventId = e.EventId
         WHERE e.Name = 'Pretoria City Marathon'
           AND c.Name = '10 KM'),

        'Confirmed',
        101,
        '0700000001',
        'No known medical conditions'
    ),

    (
        (SELECT UserId
         FROM [User]
         WHERE Email = 'participant2@raceday.com'),

        (SELECT CategoryId
         FROM Category c
         INNER JOIN Event e
             ON c.EventId = e.EventId
         WHERE e.Name = 'Pretoria City Marathon'
           AND c.Name = '21 KM'),

        'Confirmed',
        102,
        '0700000002',
        'Asthma'
    ),

    (
        (SELECT UserId
         FROM [User]
         WHERE Email = 'participant1@raceday.com'),

        (SELECT CategoryId
         FROM Category c
         INNER JOIN Event e
             ON c.EventId = e.EventId
         WHERE e.Name = 'Hartbeespoort Dam Run'
           AND c.Name = '5 KM'),

        'Completed',
        201,
        '0700000001',
        NULL
    ),

    (
        (SELECT UserId
         FROM [User]
         WHERE Email = 'participant2@raceday.com'),

        (SELECT CategoryId
         FROM Category c
         INNER JOIN Event e
             ON c.EventId = e.EventId
         WHERE e.Name = 'Hartbeespoort Dam Run'
           AND c.Name = '10 KM'),

        'Confirmed',
        202,
        '0700000002',
        NULL
    );
GO

/*
========================================================
11. SEED DATA - RESULTS
Adds sample race results
========================================================
*/

INSERT INTO Result
    (EnrolmentId, FinishTime, PositionOverall,
     PositionCategory, Status, CapturedBy, Notes)
VALUES

    (
        (
            SELECT en.EnrolmentId
            FROM Enrolment en
            INNER JOIN [User] u
                ON en.ParticipantId = u.UserId
            INNER JOIN Category c
                ON en.CategoryId = c.CategoryId
            INNER JOIN Event e
                ON c.EventId = e.EventId
            WHERE u.Email = 'participant1@raceday.com'
              AND e.Name = 'Pretoria City Marathon'
              AND c.Name = '10 KM'
        ),

        '00:48:32',
        15,
        3,
        'Finished',

        (
            SELECT UserId
            FROM [User]
            WHERE Email = 'organiser1@raceday.com'
        ),

        'Strong finish'
    ),

    (
        (
            SELECT en.EnrolmentId
            FROM Enrolment en
            INNER JOIN [User] u
                ON en.ParticipantId = u.UserId
            INNER JOIN Category c
                ON en.CategoryId = c.CategoryId
            INNER JOIN Event e
                ON c.EventId = e.EventId
            WHERE u.Email = 'participant2@raceday.com'
              AND e.Name = 'Pretoria City Marathon'
              AND c.Name = '21 KM'
        ),

        '01:52:18',
        42,
        8,
        'Finished',

        (
            SELECT UserId
            FROM [User]
            WHERE Email = 'organiser1@raceday.com'
        ),

        'Completed successfully'
    ),

    (
        (
            SELECT en.EnrolmentId
            FROM Enrolment en
            INNER JOIN [User] u
                ON en.ParticipantId = u.UserId
            INNER JOIN Category c
                ON en.CategoryId = c.CategoryId
            INNER JOIN Event e
                ON c.EventId = e.EventId
            WHERE u.Email = 'participant1@raceday.com'
              AND e.Name = 'Hartbeespoort Dam Run'
              AND c.Name = '5 KM'
        ),

        '00:25:41',
        8,
        2,
        'Finished',

        (
            SELECT UserId
            FROM [User]
            WHERE Email = 'organiser2@raceday.com'
        ),

        'Excellent performance'
    );
GO

/*
========================================================
12. VERIFY SEED DATA
========================================================
*/

SELECT * FROM [User];

SELECT * FROM Event;

SELECT * FROM Category;

SELECT * FROM Enrolment;

SELECT * FROM Result;

SELECT * FROM EventImage;
GO