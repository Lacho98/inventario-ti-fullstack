# Inventario TI – Instrucciones de Ejecución

Este proyecto incluye un backend en .NET 8, un frontend en React + Vite y una base de datos SQL Server.  
Puede ejecutarse tanto de forma local como mediante Docker Compose.

---

## Requerimientos Previos

- **Docker Desktop** (con Docker Compose habilitado)  
  Necesario para ejecutar los servicios del proyecto vía contenedores.

- **Git**  
  Requerido para clonar y gestionar el repositorio.

- **.NET SDK 8**  
  Solo si se ejecuta el backend localmente, sin Docker.

- **Node.js + npm**  
  Solo si se ejecuta el frontend localmente, sin Docker.

---

## EJECUCIÓN CON DOCKER COMPOSE (Recomendado)
Desde la raíz del proyecto:(Donde se encuentra **docker-compose.yml**)

Ejecutar el siguiente comando:
docker compose up --build

## Verificar en Docker Desktop que esta corriendo:
-Frontend
-Backend
-Sql

Servicios iniciados:
- Frontend: http://localhost:3000
- Backend: http://localhost:8080/swagger
- SQL Server: localhost,1433



---


## EJECUCIÓN EN ENTORNO LOCAL

> Estos pasos solo aplican si **no se usa Docker**.

## Instalación de Dependencias

### Backend (.NET 8)

1. Abrir una terminal en:
backend/Backend
2. Ejecutar:
dotnet restore

### Frontend (React + Vite)

1. Abrir una terminal en:
frontend
2. Ejecutar:
npm install


---

## Variables de Entorno

### Backend

Las variables se configuran en `appsettings.json` o mediante Docker Compose.  
Para ejecución local, ajustar la cadena de conexión en:
ConnectionStrings:DefaultConnection

**Ejemplo SQL local:**
Server=localhost;Database=InventarioTI;Trusted_Connection=True;TrustServerCertificate=True;

**Ejemplo SQL en Docker:**
Server=sql;Database=InventarioTI;User=sa;Password=YourSecretPassw0rd!;TrustServerCertificate=True;

### Base de datos
El script SQL para crear las tablas, relaciones y datos iniciales se encuentra en:

/backend/Backend/docs/database.sql

Puede ejecutarse directamente en SQL Server Management Studio para preparar la base de datos en entorno local.

### Frontend
El frontend utiliza la variable:
VITE_API_BASE_URL

---

### Iniciar backend:
cd backend/Backend

dotnet run

Disponible en:
http://localhost:<puerto>/swagger

### Iniciar frontend:
cd frontend

npm run dev

Disponible en:
http://localhost:5173

---


## ESTADO DEL PROYECTO
Aplicación lista para ejecutarse localmente o mediante Docker.