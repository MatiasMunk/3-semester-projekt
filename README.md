# JaTakTilbud System - 3rd Semester Project

## Overview

This project implements a distributed 3-tier system designed to manage and automate "Ja Tak" offer campaigns. The purpose of the system is to replace manual workflows with a structured, scalable, and concurrent-safe solution.

The system supports:

- Campaign management  
- Reservation handling  
- Inventory tracking  
- Secure user authentication  

The solution is built with a clear separation of concerns and follows modern architectural principles.

---

## System Architecture

### 3-Tier Distributed Architecture

The system is structured into three main layers:

- **Client Layer**
  - Web application (MVC)
  - Native desktop application (WinForms)

- **Application Layer**
  - REST API responsible for business logic and data access

- **Data Layer**
  - SQL Server database

---

## Layer Responsibilities

### Client Layer (Web + Desktop)

- Handles user interaction and presentation  
- Communicates with the API via HTTP (REST)  
- Contains a Service Layer for API communication  
- Does **not** access the database directly  

---

### Application Layer (API)

- Exposes REST endpoints  
- Implements business logic  
- Handles authentication and authorization  
- Contains the only **true Data Access Layer**  

---

### Data Layer (Database)

- Stores campaigns, reservations, and related data  
- Accessed exclusively through the API  

---

## Internal Layering per Project

Each application follows a layered structure, but with different responsibilities:

### API

- Controllers (Presentation)
- Services (Business Logic)
- Data Access Layer (Dapper -> SQL Server)

---

### Web and Client

- UI / Controllers
- Service Layer (calls API via HTTP)

The "Data Access Layer" in these projects has been replaced by a Service Layer after introducing the API.

---

## Shared Libraries

### JaTakTilbud.Core

Contains shared domain logic:

- Domain models (Campaign, Reservation, User)  
- Interfaces (e.g. `ICampaignService`)  
- Business rules  
- Enums and common utilities  

---

### JaTakTilbud.Contracts

Defines communication contracts:

- DTOs  
- Request/response models  
- API-facing data structures  

---

## Solution Structure

JaTakTilbud.sln

JaTakTilbud.Core  
JaTakTilbud.Contracts  
JaTakTilbud.Infrastructure  
JaTakTilbud.API  
JaTakTilbud.Web  
JaTakTilbud.Client  

---

## Data Flow

Client -> API -> Database  
Web    -> API -> Database  

---

## Technology Stack

### Backend

- C#  
- .NET  
- ASP.NET Core Web API  

### Frontend

- ASP.NET MVC  
- WinForms (C# desktop client)  

### Data Access

- Dapper (micro ORM)  
- SQL Server  

### Communication

- REST API  
- JSON  

### Authentication

- OpenID Connect  
- JWT Bearer tokens  

---

## Authentication and Security

Authentication is implemented using OpenID Connect.

- Web application uses OpenID Connect login flow  
- API uses JWT Bearer authentication  
- Tokens are validated by the API before allowing access  

Authorization is handled using policies based on claims and roles.

---

## Core Functionality

The system includes the following core features:

- Create, update, and manage campaigns  
- Reserve offers ("Ja Tak")  
- Track available stock  
- Prevent overselling through controlled updates  

---

## Concurrency and Data Integrity

To ensure correct behavior under concurrent usage, stock updates are handled atomically at the database level.

Example:

UPDATE Campaigns
SET Stock = Stock - 1
WHERE Id = @Id AND Stock > 0

---

## API Endpoints (Examples)

### Campaigns

GET    /api/campaigns  
GET    /api/campaigns/{id}  
POST   /api/campaigns  
PUT    /api/campaigns/{id}  
DELETE /api/campaigns/{id}  

---

### Reservations

POST   /api/reservations  
GET    /api/reservations/{userId}  

---

## Design Principles

- Separation of concerns  
- Single responsibility per layer  
- Centralized data access  
- Stateless API design  
- Scalability and maintainability  

---

## Summary

- Distributed 3-tier architecture  
- API is the only layer with database access  
- Client and Web communicate via REST  
- Shared Core and Contracts libraries ensure consistency  
- Dapper provides efficient and controlled data access  
- OpenID Connect secures authentication  

The solution provides a scalable and robust foundation for handling campaign-based reservation systems with proper concurrency control and modern architectural practices.
