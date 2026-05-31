# StudyGroups Platform - 3rd Semester Project

## Overview

StudyGroups is a distributed 3-tier platform designed to help students organize, join, and manage study groups and study sessions.

The system was developed as part of a 3rd semester System Development project with focus on:

- Distributed systems
- Agile development
- Scrum
- Software architecture
- Concurrency handling
- Authentication and authorization
- Realtime communication

The platform replaces fragmented communication and manual coordination with a centralized and scalable solution for collaborative studying.

The system supports:

- Study session management
- User registration and authentication
- Join/leave functionality
- Live study rooms
- Private messaging
- Friend requests and social features
- Admin moderation
- Realtime notifications

The solution follows modern architectural principles with strong separation of concerns and modular design.

---

# System Architecture

## 3-Tier Distributed Architecture

The platform is structured into three primary layers:

### Client Layer

- Web application (ASP.NET MVC / Razor)
- Native desktop application (WinForms)

### Application Layer

- ASP.NET Core Web API
- Business logic
- Authentication and authorization
- Realtime communication integration

### Data Layer

- SQL Server database

---

# Layer Responsibilities

## Client Layer (Web + Desktop)

Responsible for:

- User interaction and presentation
- Sending HTTP requests to the API
- Rendering study sessions, messaging and social features
- Session handling and authentication flow

The clients never access the database directly.

---

## Application Layer (API)

Responsible for:

- REST API endpoints
- Business logic
- Validation
- Authentication and authorization
- Realtime token generation
- Concurrency handling
- Database access

The API acts as the central communication layer between all clients and the database.

---

## Data Layer (Database)

Responsible for:

- Persisting users
- Study sessions
- Session participants
- Friendships
- Messages
- Categories
- Notifications

The database is accessed exclusively through the API and Infrastructure layer.

---

# Internal Layering per Project

## API

The API follows layered architecture principles:

- Controllers (Presentation Layer)
- Services (Business Logic Layer)
- Infrastructure/Data Access Layer
- SQL Server persistence

---

## Web and Desktop Clients

The clients contain:

- UI Layer
- Controllers/ViewModels
- Service Layer for API communication

The previous direct data access approach was replaced with API-driven communication to support distributed architecture principles.

---

# Shared Libraries

## StudyGroups.Core

Contains shared domain logic:

- Domain models
- Interfaces
- Business rules
- Validation logic
- Shared utilities

Examples:

- StudySession
- SessionParticipant
- User
- Friendship
- Message

---

## StudyGroups.Contracts

Defines communication contracts between clients and API:

- DTOs
- Request models
- Response models
- Authentication contracts
- API-facing structures

---

# Solution Structure

```text
StudyGroups.sln

StudyGroups.Core
StudyGroups.Contracts
StudyGroups.Infrastructure
StudyGroups.API
StudyGroups.Web
StudyGroups.Client
```

---

# Data Flow

## Desktop Client -> API -> Database

The WinForms application communicates with the API through HTTP requests and JSON serialization.

---

## Web Client -> API -> Database

The web platform communicates with the API through REST endpoints.

---

## Realtime Communication

Realtime communication is handled through LiveKit and WebRTC:

```text
Client -> LiveKit -> Other Clients
```

This enables:

- Live video rooms
- Audio communication
- Realtime notifications
- Live chat synchronization

---

# Technology Stack

## Backend

- C#
- .NET
- ASP.NET Core Web API

---

## Frontend

### Web

- ASP.NET MVC
- Razor Views
- Bootstrap

### Desktop

- WinForms

---

## Data Access

- Dapper (Micro ORM)
- SQL Server

---

## Communication

- REST API
- JSON
- WebRTC
- LiveKit

---

## Authentication

- JWT Bearer tokens
- Session-based authentication
- Role-based authorization

---

# Authentication and Security

The platform implements secure authentication and authorization mechanisms.

## Authentication Flow

1. User submits login credentials
2. Password is verified using BCrypt hashing
3. JWT token/session is generated
4. Token is validated on protected endpoints

---

## Authorization

Authorization is implemented using:

- Roles
- Claims
- Protected API endpoints
- Authorization middleware

Examples:

- Admin-only endpoints
- User-specific session access
- Protected moderation actions

---

## Security Features

- BCrypt password hashing
- JWT validation
- HTTPS support
- Parameterized SQL queries
- CORS configuration
- Role-based access control

---

# Core Functionality

The platform includes the following major features:

## Study Sessions

- Create sessions
- Join sessions
- Leave sessions
- Filter/search sessions
- Session categories

---

## Social Features

- Friend requests
- Friendships
- Private messaging
- Notification system

---

## Realtime Features

- Live study rooms
- Video/audio communication
- Realtime chat
- Presence synchronization

---

## Administration

- Category management
- User moderation
- Session administration
- Dashboard statistics

---

# Concurrency and Data Integrity

Concurrency handling was a major technical focus of the project.

To prevent oversubscription during simultaneous joins, session updates are handled atomically using SQL transactions and database constraints.

Example:

```sql
BEGIN TRANSACTION

IF CurrentParticipants < MaxParticipants
BEGIN
    INSERT INTO SessionParticipants (...)
    UPDATE StudySessions
    SET CurrentParticipants = CurrentParticipants + 1
END

COMMIT
```

The system also uses:

```sql
CONSTRAINT UQ_SessionUser UNIQUE (SessionId, UserId)
```

to prevent duplicate participation records.

This ensures:

- Data consistency
- Atomic operations
- Race condition prevention
- Concurrent safety

---

# API Endpoints (Examples)

## Sessions

```http
GET /api/sessions
GET /api/sessions/{id}
POST /api/sessions
POST /api/sessions/{id}/join
DELETE /api/sessions/{id}/leave
```

---

## Authentication

```http
POST /api/auth/login
POST /api/auth/register
```

---

## Friends

```http
POST /api/friends/request
POST /api/friends/accept
GET /api/friends
```

---

## Messages

```http
GET /api/messages
POST /api/messages
```

---

# Design Principles

The project follows several core architectural and software engineering principles:

- Separation of concerns
- Layered architecture
- Low coupling / high cohesion
- Modular design
- Stateless API design
- Reusable shared libraries
- Scalability
- Maintainability
- Distributed system principles

---

# Agile Development

The project was developed iteratively using Scrum.

The development process included:

- Sprint planning
- Daily Scrum
- Sprint reviews
- Sprint retrospectives
- Product backlog refinement

The project evolved significantly during development, including a full domain pivot from the original JaTakTilbud concept to StudyGroups.

This demonstrated the flexibility and adaptability of agile development methods.

---

# Summary

StudyGroups demonstrates:

- Distributed 3-tier architecture
- REST-based communication
- Realtime WebRTC integration
- Authentication and authorization
- Concurrency-safe database operations
- Modular software architecture
- Agile Scrum-based development

The project provides a scalable and maintainable foundation for collaborative student learning platforms with both traditional and realtime communication capabilities.