# SubastaYa

Plataforma web de subastas en tiempo real y comercio electrónico, desarrollada como Trabajo Práctico de Proyecto de Software I.

## Integrantes

- Agustín Nahuel Acosta Izquierdo
- Leila Nahir Peso Abdala Bello

Repositorio: https://github.com/agustin-acosta/subastaya

## Descripción del proyecto

SubastaYa busca resolver dos problemas típicos de las plataformas de subastas online:

1. **Confianza y solvencia económica**: toda puja debe estar respaldada por saldo real, congelado en garantía (Escrow) dentro de la billetera virtual del usuario. No se permite pujar sin fondos disponibles.
2. **Juego limpio (anti-sniping)**: si una oferta válida ingresa dentro de los últimos 60 segundos antes del cierre, la subasta se extiende automáticamente 2 minutos, para que nadie pueda ganar simplemente ofertando en el último milisegundo.

## Arquitectura

El backend está organizado en capas, siguiendo el principio de que la lógica de negocio no dependa de detalles de infraestructura (bases de datos, HTTP, SignalR):

backend/
├── Api/ # Punto de entrada de la aplicación: Controllers, Hub de SignalR, middleware
├── Application/ # Casos de uso (Commands y Queries), interfaces, DTOs — el "qué hace" el sistema
├── Application.Tests/ # Tests unitarios (xUnit + Moq) sobre los casos de uso
├── Domain/ # Entidades del negocio (Subasta, Puja, Billetera, Usuario, etc.)
├── Infrastructure/ # Implementación real: DbContext, repositorios, seguridad, Worker en segundo plano
└── PruebaConcurrencia/ # Programa de consola para demostrar el control de concurrencia
frontend/ # Aplicación React (Vite) que consume la API

`Application` define interfaces (por ejemplo `ISubastaRepository`, `INotificadorSubastas`) sin saber nada de SQL Server ni de SignalR; `Infrastructure` y `Api` son quienes implementan esas interfaces con tecnología concreta. Esto permite, por ejemplo, testear toda la lógica de negocio con dependencias simuladas (mocks), sin necesitar una base de datos real ni un servidor corriendo.

## Tecnologías utilizadas

- **Backend**: C# / .NET 8, ASP.NET Core Web API, Entity Framework Core (Code-First + Migraciones), JWT Bearer Authentication, SignalR (comunicación en tiempo real).
- **Base de datos**: SQL Server (LocalDB para desarrollo).
- **Frontend**: React + Vite.
- **Testing**: xUnit + Moq.
- **Documentación de API**: Swagger / OpenAPI.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Node.js (probado con v24.21.0)
- SQL Server LocalDB (se instala junto con Visual Studio, o por separado como parte de SQL Server Express)
- Visual Studio 2022 (recomendado para el backend) o cualquier editor con soporte para C#

## Configuración

El archivo `backend/Api/appsettings.json` ya trae una configuración lista para desarrollo local, sin necesidad de modificar nada:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SubastaYaDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Esto le indica al proyecto que use la instancia de SQL Server LocalDB local, con una base llamada `SubastaYaDb`. El emisor y la audiencia del JWT también vienen cargados en ese mismo archivo.

### Clave de firma del JWT (una sola vez por máquina)

La clave con la que se firman los tokens JWT **no** está en `appsettings.json` a propósito: es un secreto, y ese archivo se sube al repositorio. En su lugar, cada persona que clone el proyecto tiene que configurarla una sola vez en su propia máquina usando [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) de .NET, que guarda el valor en una carpeta fuera del proyecto (así nunca termina en Git).

Parado en `backend/Api`, ejecutar una sola vez:

```dotnet user-secrets set "Jwt:Key" "cualquier-texto-largo-y-aleatorio-que-elijas"```


Después de esto, se debe hacer un clean + rebuild para que quede embebido en el ejecutable.

No hace falta que sea el mismo valor en todas las máquinas: cada quien corre su propia instancia local del backend, y esa instancia firma y valida sus propios tokens con su propia clave. Si te olvidás de este paso, el backend no arranca y tira el error `"Falta configurar Jwt:Key."` — es un aviso a propósito, para notar el problema apenas arranca en vez de que falle más adelante de forma confusa.

## Base de datos: migraciones

El esquema de la base de datos se genera con **Code-First**: el código en `Domain` e `Infrastructure` define cómo son las tablas, y las migraciones de Entity Framework Core son las que efectivamente crean (o actualizan) la base de datos a partir de ese código.

Parado en la carpeta `backend`, para crear la base de datos y aplicar todas las migraciones:

dotnet ef database update --project Infrastructure --startup-project Api

Si en algún momento necesitás reiniciar la base de datos completamente desde cero (por ejemplo, si se llenó de datos de prueba manuales y querés volver al estado limpio del seed):

dotnet ef database drop --project Infrastructure --startup-project Api
dotnet ef database update --project Infrastructure --startup-project Api

## Datos de prueba (Seed)

Al arrancar el backend por primera vez sobre una base de datos vacía, se cargan automáticamente los siguientes datos (ver `Infrastructure/DbSeeder.cs`):

**Usuarios** (contraseña para todos: `password123`):

| Email | Rol | Saldo total | Saldo retenido |
|---|---|---:|---:|
| vendedor@test.com | Creador de Publicaciones | $0 | $0 |
| comprador1@test.com | Postor Líder | $150.000 | $45.000 |
| comprador2@test.com | Postor Habilitado | $200.000 | $0 |
| sinfondos@test.com | Usuario Sin Fondos | $500 | $0 |

**Categorías**: Tecnología, Coleccionables, Indumentaria, Vehículos.

**Subastas**:

1. *Notebook Gamer* (Tecnología) — Activa, puja actual $45.000, con historial de dos ofertas previas.
2. *Figura de colección edición limitada* (Coleccionables) — Activa, próxima a vencer (pensada para probar el anti-sniping).
3. *Campera de cuero vintage* (Indumentaria) — Programada (todavía no arrancó).
4. *Bicicleta rodado 29* (Vehículos) — Finalizada con ganador (comprador1, $95.000).
5. *Teclado mecánico usado* (Tecnología) — Desierta (venció sin ofertas).

También se registran los movimientos de Ledger correspondientes (depósitos iniciales y la retención de $45.000 sobre la primera subasta).

## Cómo ejecutar el proyecto

### Backend

cd backend/Api
dotnet run

Queda escuchando en `https://localhost:7094` y `http://localhost:5122`.

### Frontend

cd frontend
npm install
npm run dev

Disponible en `http://localhost:5173`.

### Compilar y testear toda la solución

Parado en `backend`:

dotnet build
dotnet test

Al momento de este commit, la suite completa pasa 15/15 tests.

## Documentación de la API (Swagger)

Con el backend corriendo, entrá a:

https://localhost:7094/swagger


Ahí se pueden ver y probar todos los endpoints disponibles. Los que requieren estar logueado (marcados con un candado) necesitan un token: primero hacé `POST /api/auth/login` con alguno de los usuarios del seed, copiá el valor de `token` de la respuesta, y pegalo en el botón **Authorize** (arriba a la derecha) con el formato `Bearer {token}`.

## Funcionalidades principales

- Gestión de subastas (crear, listar con filtros, modificar, eliminar) con estados: Programada, Activa, Finalizada, Desierta.
- Pujas con validación de incremento mínimo, vigencia de la subasta y fondos disponibles.
- Billetera con saldo total, retenido y disponible (mecanismo de Escrow).
- **Anti-sniping**: si una oferta válida entra dentro de los últimos 60 segundos antes del cierre, la subasta se extiende automáticamente 2 minutos.
- **Auditoría**: quedan registrados los cambios de estado de subastas, las extensiones por anti-sniping y los rechazos de pujas por concurrencia.
- **Comunicación en tiempo real** vía SignalR (hub en `/hubs/subastas`): todos los usuarios viendo el detalle de una subasta reciben en vivo las nuevas pujas, las extensiones de tiempo y los cambios de estado, sin necesidad de recargar la página.
- **Liquidación automática**: un proceso en segundo plano (`LiquidacionWorker`) revisa periódicamente las subastas vencidas, transfiere el saldo al vendedor cuando hay ganador, y marca como Desierta a las que no recibieron ofertas.

## Concurrencia: Optimistic Locking

Cuando dos usuarios pujan casi al mismo tiempo sobre la misma subasta, solo una de las dos pujas puede aceptarse — de lo contrario, se podrían perder datos o generar retenciones de saldo inconsistentes. Para resolver esto, `Subasta` tiene un campo `Version` que Entity Framework Core usa para detectar automáticamente si dos operaciones intentaron modificar la misma fila al mismo tiempo. Cuando eso pasa, se lanza una excepción de concurrencia que el sistema traduce a una respuesta HTTP **409 Conflict**.

### Cómo reproducir la prueba

En `backend/PruebaConcurrencia` hay un programa de consola que inicia sesión con dos compradores reales del seed (`comprador1@test.com` y `comprador2@test.com`) y les hace ofertar el mismo monto sobre la misma subasta exactamente al mismo tiempo (usando `Task.WhenAll`).

1. Levantar el backend: `cd backend/Api && dotnet run`.
2. En otra terminal: `cd backend/PruebaConcurrencia && dotnet run`.

### Resultado real obtenido

Iniciando sesion con los dos compradores...

Sesiones iniciadas. Disparando las dos ofertas al mismo tiempo...

comprador1@test.com -> HTTP 201 Created | {"pujaId":4}
comprador2@test.com -> HTTP 409 Conflict | {"error":"La subasta fue modificada por otra oferta. Intentá de nuevo."}

Una de las dos ofertas se acepta y crea la puja (201 Created); la otra es rechazada porque, al llegar el turno de guardarla, la versión de la subasta que tenía en memoria ya estaba desactualizada (409 Conflict).