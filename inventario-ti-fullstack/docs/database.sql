/* ============================================================
   CREAR BASE DE DATOS (si no existe)
   ============================================================ */
IF DB_ID('InventarioTI') IS NULL
BEGIN
    CREATE DATABASE InventarioTI;
END;
GO

USE InventarioTI;
GO

/* ============================================================
   LIMPIEZA OPCIONAL (BORRAR TABLAS SI EXISTEN)
   ==> Si es una BD nueva, puedes omitir esta sección.
   ============================================================ */
IF OBJECT_ID('dbo.HistorialAsignacion','U') IS NOT NULL DROP TABLE dbo.HistorialAsignacion;
IF OBJECT_ID('dbo.NecesidadesPorRol','U')      IS NOT NULL DROP TABLE dbo.NecesidadesPorRol;
IF OBJECT_ID('dbo.DetallesSolicitud','U')      IS NOT NULL DROP TABLE dbo.DetallesSolicitud;
IF OBJECT_ID('dbo.SolicitudesEquipamiento','U') IS NOT NULL DROP TABLE dbo.SolicitudesEquipamiento;
IF OBJECT_ID('dbo.Equipos','U')                IS NOT NULL DROP TABLE dbo.Equipos;
IF OBJECT_ID('dbo.Empleados','U')              IS NOT NULL DROP TABLE dbo.Empleados;
IF OBJECT_ID('dbo.Roles','U')                  IS NOT NULL DROP TABLE dbo.Roles;
GO

/* ============================================================
   TABLA: Roles
   ============================================================ */
CREATE TABLE Roles (
    Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NombreRol  NVARCHAR(100)     NOT NULL
);
GO

/* ============================================================
   TABLA: Empleados
   ============================================================ */
CREATE TABLE Empleados (
    Id             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NombreCompleto NVARCHAR(200)     NOT NULL,
    RolId          INT               NOT NULL,
    RolActual      NVARCHAR(100)     NULL,
    EstaActivo     BIT               NOT NULL DEFAULT(1),
    EstaDisponible BIT               NOT NULL DEFAULT(1),
    CONSTRAINT FK_Empleados_Roles FOREIGN KEY (RolId)
        REFERENCES Roles(Id)
);
GO

/* ============================================================
   TABLA: Equipos
   ============================================================ */
CREATE TABLE Equipos (
    Id                 INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TipoEquipo         NVARCHAR(50)      NOT NULL,
    Modelo             NVARCHAR(200)     NOT NULL,
    NumeroSerie        NVARCHAR(100)     NOT NULL,
    Costo              DECIMAL(18,2)     NOT NULL,
    Especificaciones   NVARCHAR(500)     NULL,
    Estado             NVARCHAR(20)      NOT NULL, -- 'disponible', 'asignado', etc.
    EmpleadoAsignadoId INT               NULL,

    CONSTRAINT UQ_Equipos_NumeroSerie UNIQUE (NumeroSerie),
    CONSTRAINT FK_Equipos_Empleados FOREIGN KEY (EmpleadoAsignadoId)
        REFERENCES Empleados(Id)
);
GO

/* ============================================================
   TABLA: SolicitudesEquipamiento
   ============================================================ */
CREATE TABLE SolicitudesEquipamiento (
    Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NombreSolicitud  NVARCHAR(200)     NOT NULL,
    Fecha            DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    Estado           NVARCHAR(20)      NOT NULL DEFAULT('pendiente')  -- 'pendiente', 'resuelta'
);
GO

/* ============================================================
   TABLA: DetallesSolicitud
   ============================================================ */
CREATE TABLE DetallesSolicitud (
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SolicitudId     INT               NOT NULL,
    RolId           INT               NOT NULL,
    CantidadPuestos INT               NOT NULL,

    CONSTRAINT FK_DetallesSolicitud_Solicitud FOREIGN KEY (SolicitudId)
        REFERENCES SolicitudesEquipamiento(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DetallesSolicitud_Rol FOREIGN KEY (RolId)
        REFERENCES Roles(Id)
);
GO

/* ============================================================
   TABLA: NecesidadesPorRol
   (Reglas de negocio: qué equipo necesita cada rol)
   ============================================================ */
CREATE TABLE NecesidadesPorRol (
    Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RolId            INT               NOT NULL,
    TipoEquipo       NVARCHAR(50)      NOT NULL,
    CantidadPorPuesto INT              NOT NULL,

    CONSTRAINT FK_NecesidadesPorRol_Rol FOREIGN KEY (RolId)
        REFERENCES Roles(Id)
);
GO

/* ============================================================
   TABLA: HistorialAsignacion
   (Log inmutable de asignaciones)
   ============================================================ */
CREATE TABLE HistorialAsignacion (
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EquipoId        INT               NOT NULL,
    EmpleadoId      INT               NOT NULL,
    SolicitudId     INT               NOT NULL,
    FechaAsignacion DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    Comentario      NVARCHAR(500)     NULL,

    CONSTRAINT FK_Historial_Equipo FOREIGN KEY (EquipoId)
        REFERENCES Equipos(Id),
    CONSTRAINT FK_Historial_Empleado FOREIGN KEY (EmpleadoId)
        REFERENCES Empleados(Id),
    CONSTRAINT FK_Historial_Solicitud FOREIGN KEY (SolicitudId)
        REFERENCES SolicitudesEquipamiento(Id)
);
GO

/* ============================================================
   DATOS INICIALES
   ============================================================ */

-- ROLES
INSERT INTO Roles (NombreRol) VALUES
(N'Diseñador'),
(N'Desarrollador'),
(N'Soporte'),
(N'Analista'),
(N'Project Manager');
GO

DECLARE @RolDisenadorId       INT = (SELECT Id FROM Roles WHERE NombreRol = N'Diseñador');
DECLARE @RolDesarrolladorId   INT = (SELECT Id FROM Roles WHERE NombreRol = N'Desarrollador');
DECLARE @RolSoporteId         INT = (SELECT Id FROM Roles WHERE NombreRol = N'Soporte');
DECLARE @RolAnalistaId        INT = (SELECT Id FROM Roles WHERE NombreRol = N'Analista');
DECLARE @RolPMId              INT = (SELECT Id FROM Roles WHERE NombreRol = N'Project Manager');

-- EMPLEADOS
INSERT INTO Empleados (NombreCompleto, RolId, RolActual, EstaActivo, EstaDisponible) VALUES
(N'Ana López',       @RolDisenadorId,     N'Diseñador',     1, 1),
(N'Carlos Pérez',    @RolDisenadorId,     N'Diseñador',     1, 1),

(N'Luis García',     @RolDesarrolladorId, N'Desarrollador', 1, 1),
(N'María Torres',    @RolDesarrolladorId, N'Desarrollador', 1, 1),

(N'Jorge Ruiz',      @RolSoporteId,       N'Soporte',       1, 1),
(N'Elena Díaz',      @RolSoporteId,       N'Soporte',       1, 1),

(N'Pedro Hernández', @RolAnalistaId,      N'Analista',      1, 1),
(N'Sofía Romero',    @RolPMId,            N'Project Manager', 1, 1);
GO

-- NECESIDADES POR ROL
-- Diseñador → 1 Laptop, 1 Monitor
INSERT INTO NecesidadesPorRol (RolId, TipoEquipo, CantidadPorPuesto) VALUES
(@RolDisenadorId,     N'Laptop',  1),
(@RolDisenadorId,     N'Monitor', 1),

-- Desarrollador → 1 Laptop, 2 Monitores
(@RolDesarrolladorId, N'Laptop',  1),
(@RolDesarrolladorId, N'Monitor', 2),

-- Soporte → 1 Desktop, 1 Monitor
(@RolSoporteId,       N'Desktop', 1),
(@RolSoporteId,       N'Monitor', 1),

-- Analista → 1 Laptop, 2 Monitores
(@RolAnalistaId,      N'Laptop',  1),
(@RolAnalistaId,      N'Monitor', 2),

-- Project Manager → 1 Laptop
(@RolPMId,            N'Laptop',  1);
GO

-- EQUIPOS INICIALES
INSERT INTO Equipos (TipoEquipo, Modelo, NumeroSerie, Costo, Especificaciones, Estado, EmpleadoAsignadoId) VALUES
(N'Laptop',  N'Dell Latitude 5420',     N'LAP-001', 20000.00, N'16GB RAM, 512GB SSD',      N'disponible', NULL),
(N'Laptop',  N'HP EliteBook 840',       N'LAP-002', 19500.00, N'16GB RAM, 512GB SSD',      N'disponible', NULL),
(N'Laptop',  N'Lenovo ThinkPad T14',    N'LAP-003', 21000.00, N'16GB RAM, 1TB SSD',        N'disponible', NULL),
(N'Laptop',  N'Dell XPS 13',            N'LAP-004', 23000.00, N'16GB RAM, 512GB SSD',      N'disponible', NULL),

(N'Desktop', N'Dell OptiPlex 7090',     N'DESK-001', 15000.00, N'16GB RAM, 512GB SSD',     N'disponible', NULL),
(N'Desktop', N'HP ProDesk 600',         N'DESK-002', 14500.00, N'16GB RAM, 512GB SSD',     N'disponible', NULL),

(N'Monitor', N'Dell 24"',               N'MON-001',  3500.00,  N'24 pulgadas',             N'disponible', NULL),
(N'Monitor', N'Dell 24"',               N'MON-002',  3500.00,  N'24 pulgadas',             N'disponible', NULL),
(N'Monitor', N'LG 27"',                 N'MON-003',  4500.00,  N'27 pulgadas',             N'disponible', NULL),
(N'Monitor', N'LG 27"',                 N'MON-004',  4500.00,  N'27 pulgadas',             N'disponible', NULL);
GO
