CREATE DATABASE ERP_Restaurant
ON PRIMARY 
( NAME = N'ERP_Restaurant', FILENAME = N'D:\ERP-Restaurant\ERP_Restaurant.mdf', SIZE = 20MB, MAXSIZE = UNLIMITED, FILEGROWTH = 10MB )
LOG ON 
( NAME = N'ERP_Restaurant_log', FILENAME = N'D:\ERP-Restaurant\ERP_Restaurant_log.ldf', SIZE = 10MB, MAXSIZE = 2048GB, FILEGROWTH = 10MB );
GO
USE ERP_Restaurant;
GO

CREATE TABLE Empresas (Id INT IDENTITY(1,1) PRIMARY KEY, RazonSocial NVARCHAR(150) NOT NULL, RUC_NIT NVARCHAR(20) UNIQUE, MonedaBase CHAR(3) DEFAULT 'PEN');
CREATE TABLE Monedas (Codigo CHAR(3) PRIMARY KEY, Nombre NVARCHAR(50) NOT NULL, Simbolo NVARCHAR(5) NOT NULL, TipoCambioVenta DECIMAL(12,4));
CREATE TABLE FormasPago (Id INT IDENTITY(1,1) PRIMARY KEY, Nombre NVARCHAR(50) NOT NULL, Codigo NVARCHAR(20) UNIQUE);
CREATE TABLE Zonas (Id INT IDENTITY(1,1) PRIMARY KEY, Nombre NVARCHAR(50) NOT NULL);
CREATE TABLE Mesas (Id INT IDENTITY(1,1) PRIMARY KEY, ZonaId INT REFERENCES Zonas(Id), Numero NVARCHAR(10) NOT NULL, QrUrl NVARCHAR(500), Estado NVARCHAR(20) DEFAULT 'LIBRE');

CREATE TABLE Productos (
    Id INT IDENTITY(1,1) PRIMARY KEY, Codigo NVARCHAR(30) UNIQUE NOT NULL, Nombre NVARCHAR(150) NOT NULL, 
    Tipo NVARCHAR(20) CHECK (Tipo IN ('INSUMO','PLATO','BEBIDA','OTRO')), UnidadBase NVARCHAR(20) NOT NULL, 
    CostoPromedio DECIMAL(12,4) DEFAULT 0, PrecioVenta DECIMAL(12,4) DEFAULT 0, Moneda CHAR(3) DEFAULT 'PEN', 
    StockActual DECIMAL(12,4) DEFAULT 0, EsReceta BIT DEFAULT 0, Activo BIT DEFAULT 1
);

CREATE TABLE RecetaDetalle (
    Id INT IDENTITY(1,1) PRIMARY KEY, PlatoId INT REFERENCES Productos(Id) ON DELETE CASCADE, 
    InsumoId INT REFERENCES Productos(Id), CantidadRequerida DECIMAL(12,4) NOT NULL, 
    CostoUnitario DECIMAL(12,4), CostoTotal AS (CantidadRequerida * CostoUnitario) PERSISTED
);

CREATE TABLE Ventas (
    Id INT IDENTITY(1,1) PRIMARY KEY, NumeroComprobante NVARCHAR(30) UNIQUE, MesaId INT REFERENCES Mesas(Id), 
    MeseroId INT, Fecha DATETIME2 DEFAULT SYSDATETIME(), Subtotal DECIMAL(12,2), Impuestos DECIMAL(12,2), 
    Total DECIMAL(12,2) NOT NULL, Moneda CHAR(3) DEFAULT 'PEN', TipoCambio DECIMAL(12,4) DEFAULT 1, 
    Estado NVARCHAR(20) DEFAULT 'ABIERTA' CHECK (Estado IN ('ABIERTA','CERRADA','ANULADA','PAGADA'))
);

CREATE TABLE VentaDetalles (
    Id INT IDENTITY(1,1) PRIMARY KEY, VentaId INT REFERENCES Ventas(Id), ProductoId INT REFERENCES Productos(Id), 
    Cantidad DECIMAL(12,3) NOT NULL, PrecioUnitario DECIMAL(12,4), Descuento DECIMAL(12,2) DEFAULT 0, 
    Subtotal AS (Cantidad * PrecioUnitario - Descuento) PERSISTED
);

CREATE TABLE VentaPagos (
    Id INT IDENTITY(1,1) PRIMARY KEY, VentaId INT REFERENCES Ventas(Id), FormaPagoId INT REFERENCES FormasPago(Id), 
    Monto DECIMAL(12,2) NOT NULL, Moneda CHAR(3) DEFAULT 'PEN', TipoCambio DECIMAL(12,4) DEFAULT 1, Referencia NVARCHAR(100)
);

CREATE TABLE InventarioMovimientos (
    Id INT IDENTITY(1,1) PRIMARY KEY, ProductoId INT REFERENCES Productos(Id), 
    Tipo NVARCHAR(20) CHECK (Tipo IN ('ENTRA','SALE','AJUSTE','MERMA')), Cantidad DECIMAL(12,4) NOT NULL, 
    CostoUnitario DECIMAL(12,4), Fecha DATETIME2 DEFAULT SYSDATETIME()
);
GO

CREATE OR ALTER TRIGGER trg_ActualizarStock ON InventarioMovimientos AFTER INSERT AS
BEGIN
    UPDATE p SET StockActual = StockActual + CASE WHEN i.Tipo = 'ENTRA' THEN i.Cantidad ELSE -i.Cantidad END
    FROM Productos p INNER JOIN inserted i ON p.Id = i.ProductoId;
END;
GO

-- Datos semilla
INSERT INTO Monedas (Codigo, Nombre, Simbolo, TipoCambioVenta) VALUES ('PEN', 'Sol Peruano', 'S/', 1.00), ('USD', 'Dólar', '$', 3.75);
INSERT INTO FormasPago (Nombre, Codigo) VALUES ('Efectivo', 'EFEC'), ('Tarjeta', 'TARJ'), ('Yape/Plin', 'YAPE');
INSERT INTO Zonas (Nombre) VALUES ('Salón Principal'), ('Terraza');
INSERT INTO Mesas (ZonaId, Numero, Estado) VALUES (1, '01', 'LIBRE'), (1, '02', 'LIBRE'), (2, 'T1', 'LIBRE');
INSERT INTO Productos (Codigo, Nombre, Tipo, UnidadBase, PrecioVenta, StockActual) VALUES 
('P001', 'Lomo Saltado', 'PLATO', 'UNIDAD', 35.00, 100),
('P002', 'Ceviche Clásico', 'PLATO', 'UNIDAD', 40.00, 100),
('P003', 'Inca Kola 500ml', 'BEBIDA', 'UNIDAD', 8.00, 50);