A web-based service station queue handling application built with **ASP.NET Core MVC**, **Entity Framework Core** and **MySQL**.

The application manages customer tokens through three main stages:

**Waiting → Serving → Completed**

It is designed to handle concurrent queue operations safely using database transactions and row-level locking.

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-Database-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework%20Core-9-512BD4)](https://learn.microsoft.com/ef/core/)

## Live Application

**[Open the application](http://station.runasp.net/)**

## Features

### Customer Token Generation

Customers can request a token from the system.

- Generates a unique 6-digit token number.
- Places the token into the waiting queue.
- Prevents new tokens when the waiting queue reaches its configured limit.
- Uses a database transaction to keep queue operations consistent.

### Waiting Queue

The application maintains a FIFO-style waiting queue.

Tokens are ordered by:

1. Creation time
2. Database ID when two tokens have the same creation timestamp

This ensures deterministic ordering when customers are waiting to be served.

### Serving Customers

Staff can serve the next waiting customer.

The system:

- Checks the current number of customers being served.
- Enforces the configured serving capacity.
- Selects the oldest waiting customer.
- Changes the customer's status from `Waiting` to `Serving`.

### Complete Service

Staff can mark a currently served customer as completed.

The token moves through:

```text
Waiting
   ↓
Serving
   ↓
Completed
```

## Technology & Framework Features

### ASP.NET Core MVC

- HTTP / MVC
- Dependency Injection
- Model Binding
- ViewModels
- Razor Views
- Tag Helpers
- Anti-Forgery Tokens
- Cancellation Tokens
- `async` / `await`
- Configuration
- Static Files
- Middleware

### Entity Framework Core & MySQL

- `DbContext`
- Entity Models
- Migrations
- Indexes
- Asynchronous Database Operations
- Transactions
- Row-Level Locking
- FIFO Queue Queries
- MySQL

## Architecture

The application follows the **ASP.NET Core MVC architecture** with a separate service layer for queue management and **Entity Framework Core** for database access.

```text
┌──────────────────────┐
│       Browser        │
└──────────┬───────────┘
           │ HTTP
           ▼
┌──────────────────────┐
│      Controller      │
│   HomeController     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│     Service Layer    │
│     QueueService     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│   Entity Framework   │
│       DbContext      │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│        MySQL         │
└──────────────────────┘
```

## ASP.NET Core MVC

The application is built with **ASP.NET Core MVC** and uses the framework's built-in features for request handling, dependency injection, security, asynchronous operations and Razor-based UI rendering.

### ASP.NET Core Features

- **MVC Architecture**  
  Separates request handling, application logic, and presentation through controllers, services, models, and Razor views.

- **Dependency Injection**  
  Uses ASP.NET Core's built-in dependency injection to provide `DbContext` and `QueueService` to the parts of the application that require them.

- **Controller Actions**  
  Handles operations such as generating tokens, serving the next customer, and completing a service through controller actions.

- **HTTP GET and POST**  
  Uses GET requests for retrieving pages and POST requests for operations that modify queue state.

- **Model Binding**  
  Automatically binds form values and action parameters to controller action parameters and models.

- **ViewModels**  
  Uses dedicated view models to pass the required application data from controllers to Razor views.

- **Razor Views**  
  Uses Razor to generate the server-side HTML interface.

- **Tag Helpers**  
  Uses ASP.NET Core Tag Helpers such as `asp-controller`, `asp-action`, and `asp-for` for generating forms, links, and form fields.

- **Anti-Forgery Tokens**  
  Protects state-changing POST requests against Cross-Site Request Forgery (CSRF) using `ValidateAntiForgeryToken` and `@Html.AntiForgeryToken()`.

- **Cancellation Tokens**  
  Passes `CancellationToken` through asynchronous operations so request cancellation can be propagated to ongoing database operations when appropriate.

- **Asynchronous Request Handling**  
  Uses `async`/`await` throughout controller and service operations to perform database I/O asynchronously.

- **Configuration**  
  Uses ASP.NET Core configuration for application settings such as the database connection string.

- **Static Files**  
  Serves frontend assets such as CSS and JavaScript through ASP.NET Core's static-file handling.

- **Middleware Pipeline**  
  Uses ASP.NET Core's request-processing pipeline for application middleware and framework services.

## Entity Framework Core & MySQL

The application uses **Entity Framework Core** as the ORM and **MySQL** as the relational database. EF Core handles entity mapping, database queries, persistence, transactions and migrations while MySQL provides persistent storage for the queue.

### Entity Framework Core Features

- **DbContext**  
  Uses a custom `DbContext` to provide access to the application's database entities and manage database operations.

- **Entity Models**  
  Defines database entities such as `ServiceToken` and `QueueSettings`, which are mapped to their corresponding MySQL tables.

- **Entity-to-Table Mapping**  
  Uses EF Core model configuration to define how application properties and entities correspond to database columns and tables.

- **Dependency Injection**  
  Registers the `DbContext` with ASP.NET Core's dependency injection system so it can be injected into services that require database access.

- **LINQ Queries**  
  Uses LINQ to query and filter queue records without manually writing SQL for normal database operations.

- **Asynchronous Database Operations**  
  Uses asynchronous EF Core methods such as `CountAsync()`, `FirstOrDefaultAsync()`, `ToListAsync()`, and `SaveChangesAsync()` for database I/O.

- **Entity Tracking**  
  Uses EF Core's change tracking to detect modifications to entities and persist those changes to the database.

- **Adding and Updating Entities**  
  Uses EF Core methods such as `Add()` and tracked entity updates to create and modify customer token records.

- **Database Transactions**  
  Uses EF Core database transactions to group related queue operations into a single atomic operation.

- **Transaction Rollback and Commit**  
  Commits a queue operation only after all required database changes succeed and rolls it back when an operation fails.

- **Database Migrations**  
  Uses EF Core migrations to create and update the database schema while keeping schema changes version-controlled with the application.

- **Data Seeding**  
  Seeds the initial `QueueSettings` record with the default queue limits.

### MySQL Features

- **Relational Database**  
  Uses MySQL to persist customer tokens, queue states, timestamps, and queue configuration.

- **Primary Keys**  
  Uses primary keys to uniquely identify database records.

- **Indexes**  
  Uses indexes to improve queue queries, including retrieval of waiting customers in queue order.

- **Composite Indexing**  
  Uses an index involving `Status`, `CreatedAt`, and `Id` to efficiently locate the next waiting customer.

- **Row-Level Locking**  
  Uses MySQL's `SELECT ... FOR UPDATE` inside transactions to lock the `QueueSettings` row while a queue operation is being performed.

- **Concurrent Queue Operations**  
  Database locking prevents concurrent requests from reading and modifying the same queue state simultaneously.

- **FIFO Queue Ordering**  
  Waiting customers are ordered by `CreatedAt`, with `Id` used as a deterministic tie-breaker when timestamps are equal.

- **Database Constraints and Data Types**  
  Uses MySQL column types and database constraints through the EF Core model to maintain valid stored data.
  
## Concurrency

The queue operations are designed to handle multiple requests safely when users interact with the system concurrently.

Asynchronous programming with **`async`/`await`** enables non-blocking database operations while **cancellation tokens** support request cancellation where appropriate.

Queue-changing operations are executed inside a **database transaction**. Before modifying the queue, the `QueueSettings` row is locked using MySQL's `SELECT ... FOR UPDATE`.

```text
Request 1
    │
    ▼
Begin Transaction
    │
    ▼
Lock QueueSettings
    │
    ▼
Read Queue State
    │
    ▼
Perform Operation
    │
    ▼
Save Changes
    │
    ▼
Commit
    │
    ▼
Release Lock

Request 2
    │
    ▼
Attempts to Lock QueueSettings
    │
    ▼
Waits until Request 1 releases the lock
    │
    ▼
Acquires Lock
    │
    ▼
Continues with the operation
```


## Database

The application uses **MySQL** with **Entity Framework Core** to store customer tokens and queue configuration. The database consists of two main tables.

### ServiceTokens

Stores customer tokens and tracks their current status throughout the service process.

| Column | Description |
|---|---|
| `Id` | Primary key |
| `TokenNumber` | Unique 6-digit customer token |
| `Status` | Current status: Waiting, Serving, or Completed |
| `CreatedAt` | Token creation timestamp |

A composite index on `Status`, `CreatedAt`, and `Id` supports efficient FIFO queue queries.

Waiting customers are retrieved in ascending order of `CreatedAt`, with `Id` used as a tie-breaker when timestamps are identical.

### QueueSettings

Stores the configurable capacity limits of the service station.

| Column | Description |
|---|---|
| `Id` | Primary key |
| `MaxWaiting` | Maximum number of waiting customers |
| `MaxServing` | Maximum number of customers being served simultaneously |

The initial configuration is:

```text
MaxWaiting = 5
MaxServing = 2
```

The `QueueSettings` row also serves as the synchronization point for concurrent queue operations through MySQL's `SELECT ... FOR UPDATE`.

### Database Migrations

The database schema and initial configuration are managed using **Entity Framework Core migrations**, allowing the database to be created and updated consistently.

```bash
dotnet ef database update
```
