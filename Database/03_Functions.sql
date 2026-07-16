-- Function: fn_CalculateTicketPrice
-- Purpose : Calculates final ticket price based on whether the customer is a student

CREATE OR ALTER FUNCTION dbo.fn_CalculateTicketPrice
(
    @BasePrice DECIMAL(10,2), -- Base ticket price defined in the showtime
    @IsStudent BIT -- Indicates if the customer is a student (1 = yes, 0 = no)
)
RETURNS DECIMAL(10,2) -- Function returns the final ticket price
AS
BEGIN
    DECLARE @FinalPrice DECIMAL(10,2); -- Declare a variable to store the calculated price

    IF @IsStudent = 1 -- Check if the customer is a student
        SET @FinalPrice = @BasePrice * 0.50;  -- Apply 50% discount for student tickets
    ELSE
        SET @FinalPrice = @BasePrice; -- No discount applied for full tickets

    RETURN @FinalPrice; -- Return the calculated final price
END
GO


-- Function: fn_GetSeatStatus
-- Purpose : Returns the current status of a specific seat for a given showtime

CREATE OR ALTER FUNCTION fn_GetSeatStatus
(
    @ShowtimeId INT, -- Identifier of the showtime
    @SeatId INT -- Identifier of the seat
)
RETURNS NVARCHAR(20) -- Function returns seat status as text
AS
BEGIN
    DECLARE @Status NVARCHAR(20); -- Declare a variable to store the seat status

    -- Retrieve the seat status from ShowtimeSeats table
    SELECT @Status = Status
    FROM ShowtimeSeats
    WHERE ShowtimeId = @ShowtimeId -- Match the given showtime
      AND SeatId = @SeatId; -- Match the given seat

    RETURN ISNULL(@Status, 'Unknown'); -- Return the seat status, or 'Unknown' if no record is found
END;
GO


-- Function: fn_GetAvailableSeatCount
-- Purpose : Returns the number of available seats for a given showtime

CREATE OR ALTER FUNCTION fn_GetAvailableSeatCount
(
    @ShowtimeId INT -- Identifier of the showtime
)
RETURNS INT -- Function returns an integer count
AS
BEGIN
    DECLARE @Count INT; -- Declare a variable to store the seat count

    -- Count seats that are marked as 'Available'
    SELECT @Count = COUNT(*)
    FROM ShowtimeSeats
    WHERE ShowtimeId = @ShowtimeId -- Filter by showtime
      AND Status = 'Available'; -- Only available seats

    RETURN ISNULL(@Count, 0); -- Return the count, or 0 if no seats are found
END;
GO