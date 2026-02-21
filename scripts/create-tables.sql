-- Route Evidence - Create Tables Script (MariaDB/MySQL)
-- Run: mysql -u root -p < scripts/create-tables.sql
-- Or Docker: docker exec -i $(docker compose ps -q mariadb) mysql -uroot -proouteevidence2025 < scripts/create-tables.sql

CREATE DATABASE IF NOT EXISTS RouteEvidence
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE RouteEvidence;

-- Evidence Catalog (evidence types)
CREATE TABLE IF NOT EXISTS EvidenceCatalog (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  `Type` VARCHAR(100) NOT NULL,
  `Description` VARCHAR(255) NULL,
  Active TINYINT(1) NOT NULL DEFAULT 1,
  Ocr TINYINT(1) NOT NULL DEFAULT 0,
  CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UNIQUE KEY IX_EvidenceCatalog_Type (`Type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Units (vehicles)
CREATE TABLE IF NOT EXISTS Units (
  Id CHAR(36) NOT NULL PRIMARY KEY,
  Plate VARCHAR(20) NOT NULL,
  EconomicNumber VARCHAR(50) NULL,
  Brand VARCHAR(100) NULL,
  Model VARCHAR(100) NULL,
  `Year` VARCHAR(50) NULL,
  Active TINYINT(1) NOT NULL DEFAULT 1,
  CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdatedAt DATETIME(6) NULL,
  UNIQUE KEY IX_Units_Plate (Plate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Evidence (route evidence with GCS reference)
CREATE TABLE IF NOT EXISTS Evidence (
  Id CHAR(36) NOT NULL PRIMARY KEY,
  GcsBucket VARCHAR(100) NOT NULL,
  GcsObjectKey VARCHAR(500) NOT NULL,
  `DateTime` DATETIME(6) NOT NULL,
  Latitude DOUBLE NOT NULL,
  Longitude DOUBLE NOT NULL,
  EvidenceType VARCHAR(100) NOT NULL,
  IsSynced TINYINT(1) NOT NULL DEFAULT 0,
  CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UnitId CHAR(36) NOT NULL,
  TotalWeight DOUBLE NULL,
  Tara DOUBLE NULL,
  NetWeight DOUBLE NULL,
  OcrText VARCHAR(8000) NULL,
  KEY IX_Evidence_UnitId (UnitId),
  KEY IX_Evidence_DateTime (`DateTime`),
  KEY IX_Evidence_IsSynced (IsSynced),
  CONSTRAINT FK_Evidence_Units_UnitId FOREIGN KEY (UnitId)
    REFERENCES Units (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
