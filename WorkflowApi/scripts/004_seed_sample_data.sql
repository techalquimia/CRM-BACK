-- Datos de ejemplo para pruebas (opcional)
-- Requiere que exista la tabla Units

USE WorkflowDb;

-- Unidad de prueba para login (NumberUnit: UNIT-001)
INSERT INTO Units (Id, NumberUnit, Description, IsActive, CreatedAtUtc, UpdatedAtUtc)
VALUES (UUID(), 'UNIT-001', 'Unidad de transporte de ejemplo', 1, NOW(6), NULL)
ON DUPLICATE KEY UPDATE Id = Id;
