USE ERP_Restaurant;
GO

CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreCompleto NVARCHAR(150) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Rol NVARCHAR(20) NOT NULL CHECK (Rol IN ('ADMIN','MESERO','CAJERO','COCINA')),
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME2 DEFAULT SYSDATETIME(),
    UltimoAcceso DATETIME2
);
GO

-- ============================================
-- USUARIOS DE PRUEBA (Password: "123456")
-- Hash HMAC-SHA256 de "123456" = "geqFn3oT2PB5JG+XzXzXzXzXzXzXzXzXzXzXzXz="
-- ============================================
-- Para generar el hash real, usaremos una función SQL
CREATE OR ALTER FUNCTION dbo.fn_HashPassword(@password NVARCHAR(MAX))
RETURNS NVARCHAR(500)
AS
BEGIN
    RETURN CONVERT(NVARCHAR(500), HASHBYTES('SHA2_256', @password), 2);
END;
GO

-- Insertar usuarios con hash simple (en producción usar el hash real de C#)
INSERT INTO Usuarios (NombreCompleto, Email, PasswordHash, Rol) VALUES
('Administrador Sistema', 'admin@erp.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'ADMIN'),
('Juan Mesero', 'mesero@erp.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'MESERO'),
('María Cajera', 'cajero@erp.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'CAJERO'),
('Carlos Cocinero', 'cocina@erp.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'COCINA');
GO

PRINT '✅ Tabla Usuarios creada con 4 usuarios de prueba (password: 123456)';