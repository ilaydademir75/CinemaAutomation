USE CinemaAutomationDb; -- Switch context to the CinemaAutomation database
GO

-- Stores system roles such as Admin, Customer, Company
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY, -- Unique identifier for each role
    Name NVARCHAR(50) NOT NULL UNIQUE  -- Role name (must be unique)
);
GO

-- Stores application users and authentication data
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,  -- Unique user identifier
    FullName NVARCHAR(100) NOT NULL, -- User's full name
    Email NVARCHAR(100) NOT NULL UNIQUE, -- User email (unique)
    PasswordHash NVARCHAR(255) NOT NULL, -- Hashed password
    RoleId INT NOT NULL, -- Reference to user role
    IsActive BIT NOT NULL DEFAULT 1, -- Indicates whether user is active
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- Account creation date

    CONSTRAINT FK_Users_Role FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) -- Foreign key constraint name and Link to Roles table
);
GO

-- Stores cinema operating companies/firms
CREATE TABLE Companies (
    CompanyId INT IDENTITY(1,1) PRIMARY KEY, -- Unique company identifier
    Name NVARCHAR(100) NOT NULL, -- Company name
    ContactEmail NVARCHAR(100), -- Contact email address
    Phone NVARCHAR(30), -- Contact phone number
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME() -- Record creation date
);
GO

-- Stores cinema halls/auditoriums
CREATE TABLE Halls (
    HallId INT IDENTITY(1,1) PRIMARY KEY, -- Unique hall identifier
    CompanyId INT NOT NULL, -- Company that owns the hall
    Name NVARCHAR(100) NOT NULL, -- Hall name
    TotalRows INT NOT NULL, -- Number of seat rows
    TotalColumns INT NOT NULL, -- Number of seats per row
    IsActive BIT NOT NULL DEFAULT 1, -- Hall availability status

    CONSTRAINT FK_Halls_Company FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)  -- Foreign key constraint and Link to Companies
);
GO

-- Stores physical seats inside halls
CREATE TABLE Seats (
    SeatId INT IDENTITY(1,1) PRIMARY KEY, -- Unique seat identifier
    HallId INT NOT NULL, -- Hall to which the seat belongs
    RowNumber INT NOT NULL, -- Seat row number
    SeatNumber INT NOT NULL, -- Seat number within the row
    IsActive BIT NOT NULL DEFAULT 1, -- Seat availability status

    CONSTRAINT FK_Seats_Hall FOREIGN KEY (HallId) REFERENCES Halls(HallId), -- Foreign key constraint and Link to Halls
    CONSTRAINT UQ_Seats_Hall_Row_Seat UNIQUE (HallId, RowNumber, SeatNumber) -- Unique constraint for seat layout
);
GO

-- Stores movie catalog information
CREATE TABLE Movies (
    MovieId INT IDENTITY(1,1) PRIMARY KEY, -- Unique movie identifier
    Title NVARCHAR(100) NOT NULL, -- Movie title
    Description NVARCHAR(1000), -- Movie description
    DurationMinutes INT NOT NULL, -- Movie duration in minutes
    PosterPath NVARCHAR(255), -- Path to movie poster image
    IsActive BIT NOT NULL DEFAULT 1, -- Movie availability status
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME() -- Record creation date
);
GO

-- Stores movie screening sessions
CREATE TABLE Showtimes (
    ShowtimeId INT IDENTITY(1,1) PRIMARY KEY, -- Unique showtime identifier
    MovieId INT NOT NULL, -- Movie being screened
    HallId INT NOT NULL, -- Hall where screening takes place
    StartTime DATETIME2 NOT NULL, -- Screening start date and time
    TicketBasePrice DECIMAL(10,2) NOT NULL, -- Base ticket price
    IsActive BIT NOT NULL DEFAULT 1, -- Showtime availability

    CONSTRAINT FK_Showtimes_Movie FOREIGN KEY (MovieId) REFERENCES Movies(MovieId),  -- Foreign key to Movies
    CONSTRAINT FK_Showtimes_Hall FOREIGN KEY (HallId) REFERENCES Halls(HallId) -- Foreign key to Halls
);
GO

-- Stores seat status per showtime for concurrency control
CREATE TABLE ShowtimeSeats (
    ShowtimeSeatId INT IDENTITY(1,1) PRIMARY KEY, -- Unique record identifier
    ShowtimeId INT NOT NULL, -- Related showtime
    SeatId INT NOT NULL, -- Related physical seat
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available', -- Seat status
    ReservedUntil DATETIME2 NULL, -- Reservation expiration time
    RowVersion ROWVERSION, -- Optimistic concurrency token

    CONSTRAINT FK_ShowtimeSeats_Showtime FOREIGN KEY (ShowtimeId) REFERENCES Showtimes(ShowtimeId), -- Foreign key to Showtimes
    CONSTRAINT FK_ShowtimeSeats_Seat FOREIGN KEY (SeatId) REFERENCES Seats(SeatId), -- Foreign key to Seats
    CONSTRAINT UQ_ShowtimeSeats UNIQUE (ShowtimeId, SeatId) -- Unique seat per showtime
);
GO

-- Stores ticket bookings made by users
CREATE TABLE Bookings (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,  -- Unique booking identifier
    UserId INT NOT NULL, -- User who made the booking
    ShowtimeId INT NOT NULL, -- Related showtime
    BookingStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Booking status
    TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0, -- Total booking price
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- Booking date
    PaidAt DATETIME2 NULL, -- Payment date

    CONSTRAINT FK_Bookings_User FOREIGN KEY (UserId) REFERENCES Users(UserId), -- Foreign key to Users
    CONSTRAINT FK_Bookings_Showtime FOREIGN KEY (ShowtimeId) REFERENCES Showtimes(ShowtimeId) -- Foreign key to Showtimes
);
GO

-- Stores individual seat tickets inside a booking
CREATE TABLE BookingSeats (
    BookingSeatId INT IDENTITY(1,1) PRIMARY KEY, -- Unique booking seat identifier
    BookingId INT NOT NULL,  -- Related booking
    ShowtimeSeatId INT NOT NULL, -- Related showtime seat
    TicketType NVARCHAR(10) NOT NULL, -- Ticket type (Student / Full)
    UnitPrice DECIMAL(10,2) NOT NULL, -- Price per seat

    CONSTRAINT FK_BookingSeats_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId), -- Foreign key to Bookings
    CONSTRAINT FK_BookingSeats_ShowtimeSeat FOREIGN KEY (ShowtimeSeatId) REFERENCES ShowtimeSeats(ShowtimeSeatId), -- Foreign key to ShowtimeSeats
    CONSTRAINT UQ_BookingSeats UNIQUE (BookingId, ShowtimeSeatId) -- Prevent duplicate seats
);
GO

-- Stores buffet/snack products
CREATE TABLE Snacks (
    SnackId INT IDENTITY(1,1) PRIMARY KEY, -- Unique snack identifier
    Name NVARCHAR(100) NOT NULL, -- Snack name
    UnitPrice DECIMAL(10,2) NOT NULL, -- Snack price
    StockQuantity INT NOT NULL, -- Available stock
    IsActive BIT NOT NULL DEFAULT 1 -- Availability status
);
GO

-- Stores snack purchase orders
CREATE TABLE SnackOrders (
    SnackOrderId INT IDENTITY(1,1) PRIMARY KEY, -- Unique order identifier
    UserId INT NOT NULL, -- User who placed the order
    OrderDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- Order date
    TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0, -- Total order price

    CONSTRAINT FK_SnackOrders_User FOREIGN KEY (UserId) REFERENCES Users(UserId) -- Foreign key to Users
);
GO

-- Stores individual items inside snack orders
CREATE TABLE SnackOrderItems (
    SnackOrderItemId INT IDENTITY(1,1) PRIMARY KEY, -- Unique item identifier
    SnackOrderId INT NOT NULL, -- Related snack order
    SnackId INT NOT NULL, -- Related snack
    Quantity INT NOT NULL, -- Quantity ordered
    UnitPrice DECIMAL(10,2) NOT NULL, -- Price per unit

    CONSTRAINT FK_SnackOrderItems_Order FOREIGN KEY (SnackOrderId) REFERENCES SnackOrders(SnackOrderId), -- Foreign key to SnackOrders
    CONSTRAINT FK_SnackOrderItems_Snack FOREIGN KEY (SnackId) REFERENCES Snacks(SnackId) -- Foreign key to Snacks
);
GO

-- Stores audit trail for system operations
CREATE TABLE AuditLogs (
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY, -- Unique log identifier
    EntityName NVARCHAR(100) NOT NULL, -- Affected entity/table
    ActionType NVARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    ActionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- Action date
    UserId INT NULL, -- User who performed the action
    KeyValue NVARCHAR(100) NULL, -- Primary key value
    OldValue NVARCHAR(MAX) NULL, -- Old data snapshot
    NewValue NVARCHAR(MAX) NULL -- New data snapshot
);
GO

-- Indexes
-- Improve query performance
----------------------------

-- Index for seat availability by showtime
CREATE INDEX IX_ShowtimeSeats_Showtime_Status
    ON ShowtimeSeats(ShowtimeId, Status);

-- Index for booking queries by showtime and status
CREATE INDEX IX_Bookings_Showtime_Status
    ON Bookings(ShowtimeId, BookingStatus);

-- Index for active snack filtering
CREATE INDEX IX_Snacks_IsActive
    ON Snacks(IsActive);
GO