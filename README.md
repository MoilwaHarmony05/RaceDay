# RaceDay – Event Management System

RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community.

The platform allows **Event Organisers** to create and manage events, categories, and participant results, while **Participants** can browse upcoming events, enter events, track their performance history, and prepare for race day.

---

## Project Structure (Part 1)

This repository currently contains **Part 1 – System Planning and Database**.

---

## Roles

### Organiser
- Create, edit, and delete events
- Manage event categories
- Capture participant results
- View all event enrolments

### Participant
- Create an account
- Browse upcoming events
- Enter an event by selecting a category
- View personal enrolments and results

---

## Part 1 Deliverables

| Section | Deliverable | File |
|---------|-------------|------|
| A | Entity Relationship Diagram | `docs/RaceDay_ERD.png` |
| B | API Endpoint Plan | `docs/API_Endpoint_Plan.md` |
| C | SQL Database Script | `docs/RaceDay_Schema.sql` |

---

## Database Overview

The system uses the following main entities:

- **User** – Stores both Organisers and Participants
- **Event** – Race / walk / cycle events
- **Category** – Distance or age categories within an event
- **Enrolment** – Participant registration for a category
- **Result** – Race results captured by organisers
- **EventImage** – Event images (prepared for Azure Blob Storage)

---

## How to run the SQL Script

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Open the file: `docs/RaceDay_Schema.sql`
4. Execute the script
5. The script will create the `RaceDayDB` database, all tables, constraints, and sample data

---

## GitHub Actions

A GitHub Actions workflow is included to validate that the `/docs` folder contains the required Part 1 files.

---
## YouTube video presentation link
https://youtu.be/wEaA0o9FUl4 .


## Future Parts

- **Part 2**: Build the RESTful API in C#
- **Part 3**: Build the MVC web application, integrate Azure Blob Storage, and containerise the solution with Docker
