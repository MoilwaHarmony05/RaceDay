# RaceDay Application - Part 3

A full-stack ASP.NET Core application for managing sporting events.
Organisers create and manage events; participants browse events and enrol.
Event banner images are stored in Azure Blob Storage.

## Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 (or later)
- SQL Server LocalDB
- Azure Storage Account (for banner images)
- Docker Desktop (optional, for containerisation)

## Project Structure
- `RaceDay.Api` – REST API (EF Core, sessions, Azure Blob Storage)
- `RaceDay.Web` – MVC web application that consumes the API
- `docs` – ERD, API plan, SQL script (Part 1)

## How to Run Locally

### 1. Start the API
Open **Terminal/Window PowerShell** and run:

```bash
cd RaceDay.Api
dotnet run
