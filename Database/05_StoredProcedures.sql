-- Procedure: sp_AddMovieWithPoster
-- Purpose  : Inserts a new movie record including poster path and returns the new MovieId

-- Switch context to the CinemaAutomation database
USE CinemaAutomationDb;
GO

CREATE OR ALTER PROCEDURE sp_AddMovieWithPoster
    @Title           NVARCHAR(100), -- Movie title
    @Description     NVARCHAR(1000), -- Movie description
    @DurationMinutes INT, -- Movie duration in minutes
    @PosterPath      NVARCHAR(255) -- Path to movie poster image
AS
BEGIN
    SET NOCOUNT ON; -- Prevent extra result sets from interfering with output

    -- Insert a new movie record into Movies table
    INSERT INTO Movies (Title, Description, DurationMinutes, PosterPath)
    VALUES (@Title, @Description, @DurationMinutes, @PosterPath);

    -- Return the newly created MovieId for UI usage
    SELECT SCOPE_IDENTITY() AS NewMovieId;
END;
GO

-- Procedure: sp_CreateBooking
-- Purpose  : Creates a new booking, reserves seats transactionally and calculates prices

CREATE OR ALTER PROCEDURE sp_CreateBooking
    @UserId        INT, -- User who makes the booking
    @ShowtimeId    INT, -- Showtime being booked
    @SeatIdsCsv    NVARCHAR(400), -- Comma-separated seat IDs
    @TicketType    NVARCHAR(10)  -- Ticket type: Student or Full
AS
BEGIN
    SET NOCOUNT ON; -- Prevent extra result sets

    DECLARE @BasePrice DECIMAL(10,2); -- Declare variable to store base ticket price

    -- Retrieve base ticket price from showtime
    SELECT @BasePrice = TicketBasePrice
    FROM Showtimes
    WHERE ShowtimeId = @ShowtimeId;

    -- If showtime does not exist, raise error
    IF @BasePrice IS NULL
    BEGIN
        RAISERROR('Showtime not found.', 16, 1);
        RETURN;
    END

    BEGIN TRAN; -- Start database transaction

    BEGIN TRY
        -- Insert booking header with Pending status
        INSERT INTO Bookings (UserId, ShowtimeId, BookingStatus)
        VALUES (@UserId, @ShowtimeId, 'Pending');

        DECLARE @BookingId INT = SCOPE_IDENTITY(); -- Store newly created BookingId

        -- Declare variables for seat processing
        DECLARE @SeatIdInt INT;
        DECLARE @Price DECIMAL(10,2);

        -- Declare cursor to iterate through seat list
        DECLARE seat_cursor CURSOR FOR
        SELECT CAST(value AS INT) -- Convert CSV values to integers
        FROM STRING_SPLIT(@SeatIdsCsv, ',') -- Split comma-separated values
        WHERE LTRIM(RTRIM(value)) <> ''; -- Ignore empty values

        OPEN seat_cursor; -- Open the cursor
        FETCH NEXT FROM seat_cursor INTO @SeatIdInt; -- Fetch first seat ID

        -- Loop through all seats
        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @ShowtimeSeatId INT; -- Declare variable for ShowtimeSeatId

            -- Lock and retrieve available seat for this showtime
            SELECT TOP 1 @ShowtimeSeatId = ShowtimeSeatId
            FROM ShowtimeSeats WITH (UPDLOCK, ROWLOCK)
            WHERE ShowtimeId = @ShowtimeId
              AND SeatId = @SeatIdInt
              AND Status = 'Available';
         
            -- If seat is not available, rollback transaction
            IF @ShowtimeSeatId IS NULL
            BEGIN
                RAISERROR('Seat %d is not available.', 16, 1, @SeatIdInt);
                ROLLBACK TRAN;
                CLOSE seat_cursor;
                DEALLOCATE seat_cursor;
                RETURN;
            END

            -- Calculate ticket price using discount function
            SET @Price = dbo.fn_CalculateTicketPrice(@BasePrice, CASE WHEN @TicketType = 'Student' THEN 1 ELSE 0 END);

            -- Reserve the seat for 10 minutes
            UPDATE ShowtimeSeats
            SET Status = 'Reserved',
                ReservedUntil = DATEADD(MINUTE, 10, SYSUTCDATETIME())
            WHERE ShowtimeSeatId = @ShowtimeSeatId;

            -- Insert seat-level booking record
            INSERT INTO BookingSeats (BookingId, ShowtimeSeatId, TicketType, UnitPrice)
            VALUES (@BookingId, @ShowtimeSeatId, @TicketType, @Price);

            FETCH NEXT FROM seat_cursor INTO @SeatIdInt; -- Fetch next seat
        END

        -- Close and deallocate cursor
        CLOSE seat_cursor;
        DEALLOCATE seat_cursor;

         -- Update total booking amount
        UPDATE b
        SET TotalAmount = (
            SELECT SUM(UnitPrice)  -- Sum seat prices
            FROM BookingSeats bs
            WHERE bs.BookingId = b.BookingId
        )
        FROM Bookings b
        WHERE b.BookingId = @BookingId;

        COMMIT TRAN; -- Commit transaction

        SELECT @BookingId AS NewBookingId; -- Return newly created BookingId
    END TRY
    BEGIN CATCH
        -- Rollback transaction if an error occurs
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(); -- Capture error message
        RAISERROR(@ErrMsg, 16, 1); -- Re-throw error
    END CATCH
END;
GO

-- Procedure: sp_ConfirmPayment
-- Purpose  : Confirms payment and marks seats as Sold

CREATE OR ALTER PROCEDURE sp_ConfirmPayment
    @BookingId     INT, -- Booking to be confirmed
    @PaymentAmount DECIMAL(10,2) -- Paid amount
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Total DECIMAL(10,2); -- Declare variable to store expected total

    -- Retrieve booking total amount
    SELECT @Total = TotalAmount
    FROM Bookings
    WHERE BookingId = @BookingId;

    -- Validate booking existence
    IF @Total IS NULL
    BEGIN
        RAISERROR('Booking not found.', 16, 1);
        RETURN;
    END

    -- Validate payment amount
    IF @PaymentAmount < @Total
    BEGIN
        RAISERROR('Payment is not sufficient.', 16, 1);
        RETURN;
    END

    BEGIN TRAN; -- Start transaction

    BEGIN TRY
        -- Update booking status to Confirmed
        UPDATE Bookings
        SET BookingStatus = 'Confirmed',
            PaidAt = SYSUTCDATETIME()
        WHERE BookingId = @BookingId;

        -- Mark related seats as Sold
        UPDATE ss
        SET ss.Status = 'Sold',
            ss.ReservedUntil = NULL
        FROM ShowtimeSeats ss
        INNER JOIN BookingSeats bs ON bs.ShowtimeSeatId = ss.ShowtimeSeatId
        WHERE bs.BookingId = @BookingId;

        COMMIT TRAN; -- Commit transaction
    END TRY
    BEGIN CATCH
        -- Rollback on error
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        -- Raise error message
        DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Err, 16, 1);
    END CATCH
END;
GO


-- Procedure: sp_ReleaseExpiredReservations
-- Purpose  : Releases expired seat reservations and cancels related pending bookings

CREATE OR ALTER PROCEDURE sp_ReleaseExpiredReservations
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRAN; -- Start transaction

    BEGIN TRY
        DECLARE @Now DATETIME2 = SYSUTCDATETIME(); -- Store current UTC time

        -- Cancel pending bookings with expired reservations
        UPDATE b
        SET BookingStatus = 'Cancelled'
        FROM Bookings b
        WHERE b.BookingStatus = 'Pending'
          AND EXISTS (
              SELECT 1
              FROM BookingSeats bs
              INNER JOIN ShowtimeSeats ss ON ss.ShowtimeSeatId = bs.ShowtimeSeatId
              WHERE bs.BookingId = b.BookingId
                AND ss.Status = 'Reserved'
                AND ss.ReservedUntil < @Now
          );

        -- Release expired reserved seats
        UPDATE ShowtimeSeats
        SET Status = 'Available',
            ReservedUntil = NULL
        WHERE Status = 'Reserved'
          AND ReservedUntil < @Now;

        COMMIT TRAN; -- Commit transaction
    END TRY
    BEGIN CATCH
        -- Rollback on error
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        -- Raise error message
        DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Err, 16, 1);
    END CATCH
END;
GO

-- Procedure: sp_UpdateSnackStock
-- Purpose  : Increases or decreases snack stock quantity

CREATE OR ALTER PROCEDURE sp_UpdateSnackStock
    @SnackId INT, -- Snack identifier
    @Delta   INT -- Stock change amount (positive or negative)
AS
BEGIN
    SET NOCOUNT ON;

    -- Update snack stock quantity
    UPDATE Snacks
    SET StockQuantity = StockQuantity + @Delta
    WHERE SnackId = @SnackId;

    -- Raise error if snack does not exist
    IF @@ROWCOUNT = 0
        RAISERROR('Snack not found.', 16, 1);
END;
GO
