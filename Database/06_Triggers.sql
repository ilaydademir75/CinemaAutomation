-- Switch context to the CinemaAutomation database
USE CinemaAutomationDb;
GO

-- Trigger: trg_DecrementSnackStockOnOrder
-- Purpose: Automatically decreases snack stock when a snack order item is inserted

CREATE OR ALTER TRIGGER trg_DecrementSnackStockOnOrder
ON dbo.SnackOrderItems -- Trigger fires on snack order items
AFTER INSERT -- Trigger runs after insert operation
AS
BEGIN
    SET NOCOUNT ON; -- Prevent extra result sets

    -- Update snack stock quantities
    UPDATE s
    SET s.StockQuantity = s.StockQuantity - i.Quantity -- Decrease stock by ordered quantity
    FROM dbo.Snacks s -- Target Snacks table
    INNER JOIN inserted i ON i.SnackId = s.SnackId; -- Inserted pseudo-table and Match snack records
END;
GO


-- Trigger: trg_LogUserInsert
-- Log user INSERT operations into AuditLogs
CREATE OR ALTER TRIGGER trg_LogUserInsert
ON dbo.Users
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLogs
    (
        EntityName,
        ActionType,
        ActionDate,
        KeyValue,
        NewValue
    )
    SELECT
        'Users',
        'INSERT',
        GETDATE(),
        CAST(i.UserId AS NVARCHAR(50)),
        CONCAT('FullName=', i.FullName, '; Email=', i.Email)
    FROM inserted i;
END;
GO


-- Trigger: trg_LogSnackUpdate
-- Log snack UPDATE operations into AuditLogs
CREATE OR ALTER TRIGGER trg_LogSnackUpdate
ON dbo.Snacks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLogs
    (
        EntityName,
        ActionType,
        ActionDate,
        KeyValue,
        OldValue,
        NewValue
    )
    SELECT
        'Snacks',
        'UPDATE',
        GETDATE(),
        CAST(i.SnackId AS NVARCHAR(50)),
        CONCAT('Price=', d.UnitPrice, '; Stock=', d.StockQuantity),
        CONCAT('Price=', i.UnitPrice, '; Stock=', i.StockQuantity)
    FROM inserted i
    INNER JOIN deleted d ON d.SnackId = i.SnackId
    WHERE d.UnitPrice <> i.UnitPrice
       OR d.StockQuantity <> i.StockQuantity;
END;
GO


-- Trigger: trg_PreventNegativeSnackStock
-- Purpose: Prevents snack stock quantity from becoming negative after update operations

CREATE TRIGGER trg_PreventNegativeSnackStock
ON dbo.Snacks -- Trigger attached to Snacks table
AFTER UPDATE -- Trigger fires after update
AS
BEGIN
    SET NOCOUNT ON; -- Prevent extra result sets

    -- Check if any updated stock value is below zero
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.StockQuantity < 0 -- Negative stock detected
    )
    BEGIN
       -- Reset negative stock values back to zero
        UPDATE s
        SET s.StockQuantity = 0 -- Enforce minimum stock = 0
        FROM Snacks s
        INNER JOIN inserted i ON s.SnackId = i.SnackId;

        PRINT 'Stock dropped below zero, set to 0.'; -- Informational message for debugging/admin awareness
    END
END
GO