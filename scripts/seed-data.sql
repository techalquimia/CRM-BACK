-- Route Evidence - Seed Data Script
-- Run after migrations: mysql -u root -p RouteEvidence < scripts/seed-data.sql
-- Or with Docker: docker exec -i crm-back-mariadb-1 mysql -uroot -proouteevidence2025 RouteEvidence < scripts/seed-data.sql

USE RouteEvidence;

-- Evidence Catalog (evidence types)
INSERT INTO EvidenceCatalog (`Type`, `Description`, `Active`, `Ocr`, `CreatedAt`) VALUES
('ticket', 'Weighing ticket / boleto de báscula', 1, 1, UTC_TIMESTAMP()),
('photo', 'General photo evidence', 1, 0, UTC_TIMESTAMP()),
('incident', 'Incident report', 1, 0, UTC_TIMESTAMP()),
('damage', 'Damage documentation', 1, 0, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE `Description` = VALUES(`Description`);

-- Units (sample vehicles)
INSERT IGNORE INTO Units (`Id`, `Plate`, `EconomicNumber`, `Brand`, `Model`, `Year`, `Active`, `CreatedAt`) VALUES
('11111111-1111-1111-1111-111111111101', 'ABC-1234', 'UNT-001', 'Freightliner', 'Cascadia', '2022', 1, UTC_TIMESTAMP()),
('11111111-1111-1111-1111-111111111102', 'XYZ-5678', 'UNT-002', 'Kenworth', 'T680', '2023', 1, UTC_TIMESTAMP()),
('11111111-1111-1111-1111-111111111103', 'DEF-9012', 'UNT-003', 'Volvo', 'VNL', '2021', 1, UTC_TIMESTAMP());
