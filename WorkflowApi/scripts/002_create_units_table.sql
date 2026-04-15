-- Tabla Units (unidades de transporte)
-- Compatible con MySQL 8.0

USE WorkflowDb;

CREATE TABLE IF NOT EXISTS Units (
    Id          CHAR(36)        NOT NULL,
    NumberUnit  VARCHAR(50)     NOT NULL,
    Description VARCHAR(500)    NULL,
    IsActive    TINYINT(1)      NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME(6)    NOT NULL,
    UpdatedAtUtc DATETIME(6)    NULL,
    PRIMARY KEY (Id),
    UNIQUE INDEX IX_Units_NumberUnit (NumberUnit)
)
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
