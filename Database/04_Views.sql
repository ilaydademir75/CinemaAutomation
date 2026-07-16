-- View: vw_ShowtimeBookingSummary
-- Purpose: Provides a summary of each showtime including total sold seats and total revenue

-- Switch context to the CinemaAutomation database
USE CinemaAutomationDb;
GO

CREATE OR ALTER VIEW vw_ShowtimeBookingSummary
AS
-- Select summary information for each showtime
SELECT
    s.ShowtimeId, -- Unique identifier of the showtime
    m.Title AS MovieTitle, -- Movie title for the showtime
    h.Name  AS HallName, -- Hall name where the movie is screened
    s.StartTime, -- Start date and time of the showtime
    -- Count distinct sold seats for confirmed bookings only
    COUNT(DISTINCT CASE WHEN b.BookingStatus = 'Confirmed' THEN bs.BookingSeatId END) AS SoldSeatCount,
    -- Calculate total revenue from confirmed bookings only
    SUM(CASE WHEN b.BookingStatus = 'Confirmed' THEN bs.UnitPrice ELSE 0 END) AS TotalRevenue
FROM Showtimes s -- Source table: showtimes
INNER JOIN Movies m ON m.MovieId = s.MovieId -- Join movie information for each showtime
INNER JOIN Halls h ON h.HallId = s.HallId -- Join hall information for each showtime
LEFT JOIN Bookings b ON b.ShowtimeId = s.ShowtimeId -- Left join bookings to include showtimes with no bookings
LEFT JOIN BookingSeats bs ON bs.BookingId = b.BookingId -- Left join booking seats to calculate sold seats and revenue
GROUP BY s.ShowtimeId, m.Title, h.Name, s.StartTime; -- Group results by showtime and related descriptive fields
GO