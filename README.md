# CinemaAutomation - ASP.NET Core MVC Cinema Automation Project

**Student:** İlayda Demir  
**Academic Year:** 2025-2026  

## Project Overview

CinemaAutomation is a web-based cinema automation system developed using **ASP.NET Core MVC** and **Microsoft SQL Server**.

The system provides:

- Movie and showtime management
- Seat selection and ticket booking (Student / Full Ticket)
- Snack shop ordering with stock tracking
- Admin, Company, and User roles
- Sales reporting and audit logging

## Technologies Used

- ASP.NET Core MVC (.NET 8)
- C#
- Entity Framework Core
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Bootstrap
- JavaScript / jQuery

## Prerequisites

Before running the project:

- Visual Studio 2022 (ASP.NET and Web Development workload)
- .NET 8 SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)

## Database Setup

This project uses a SQL Server database backup file (`.bak`).

Steps:

1. Open SQL Server Management Studio (SSMS).
2. Connect to your SQL Server instance.
3. Right-click **Databases → Restore Database**.
4. Select **Device** and browse.
5. Choose:

```
Database/CinemaAutomationDb.bak
```

6. Set database name:

```
CinemaAutomationDb
```

7. Restore the database.

> The database must be restored successfully before running the application.

## ER Diagram

The database ER diagram can be generated from SSMS:

1. Open `CinemaAutomationDb`.
2. Go to **Database Diagrams**.
3. Add all tables.
4. Arrange relationships.
5. Export as:

```
Database/ERDiagram.png
```

## Application Setup

1. Open:

```
Program/CinemaAutomation.Web.sln
```

with Visual Studio.

2. Update the connection string in:

```
CinemaAutomation.Web/appsettings.json
```

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=CinemaAutomationDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

3. Replace `YOUR_SERVER_NAME` with your SQL Server instance.

4. Build the solution.

5. Set:

```
CinemaAutomation.Web
```

as the startup project.

6. Run the application.

## Default Test Accounts

### Admin

```
Email: admin@cinema.com
Password: admin123
```

### Company

```
Email: company@cinema.com
Password: company123
```

## Database Objects

### Functions

- fn_CalculateTicketPrice
- fn_GetSeatStatus
- fn_GetAvailableSeatCount

### View

- vw_ShowtimeBookingSummary

### Stored Procedures

- sp_AddMovieWithPoster
- sp_CreateBooking
- sp_ConfirmPayment
- sp_ReleaseExpiredReservations
- sp_UpdateSnackStock

### Triggers

- trg_DecrementSnackStockOnOrder
- trg_LogUserInsert
- trg_PreventNegativeSnackStock
- trg_LogSnackUpdate

## Application Features

- Customer login
- Movie and showtime listing
- Seat selection
- Student/full ticket selection
- Payment confirmation
- Snack ordering
- Automatic stock updates
- Admin management panel
- Company reporting panel

## Project Documentation

- README.md
- ProjectReport.docx
