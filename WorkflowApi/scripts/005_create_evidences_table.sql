-- Evidences table: id, unitId, typeEvidence, location (GPS), date/time, image URL
-- MySQL 8.0

USE WorkflowDb;

CREATE TABLE IF NOT EXISTS Evidences (
    Id            CHAR(36)        NOT NULL,
    UnitId        CHAR(36)        NOT NULL,
    TypeEvidence  VARCHAR(50)     NOT NULL,
    Latitude      DECIMAL(10,7)   NULL,
    Longitude     DECIMAL(10,7)   NULL,
    RecordedAtUtc DATETIME(6)     NOT NULL,
    ImageUrl      VARCHAR(500)    NULL,
    CreatedAtUtc  DATETIME(6)     NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_Evidences_UnitId (UnitId),
    CONSTRAINT FK_Evidences_Units_UnitId
        FOREIGN KEY (UnitId) REFERENCES Units (Id) ON DELETE CASCADE
)
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
