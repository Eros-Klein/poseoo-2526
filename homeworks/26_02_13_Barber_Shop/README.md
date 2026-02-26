# Barber Shop Management System

## Introduction

Welcome to the Barber Shop Management System! You have just started your new job as the lead developer for "Gerrit's Cuts," a trendy barber shop with a unique approach to business. Your task is to complete the backend and frontend implementation of an appointment management system with a complex pricing model.

The following simplifications and assumptions apply:

- All appointments are walk-in or booked through this system. No external booking systems are considered.
- The barber shop only operates Friday through Sunday.
- All services and their base prices are predefined in the system.
- Currency is Euro (€) and prices are rounded to 2 decimal places.

The rules for calculating appointment prices are described in detail in [Price_Calculation.md](./Price_Calculation.md). The pricing model includes multiple surcharges, discounts, and requires database queries to determine loyalty tiers and group bookings.

## Functional Requirements

### View Appointment Dashboard

As a user, I want to see an overview of all scheduled appointments so that I can manage the barber shop's schedule.

**Acceptance criteria:**

- The dashboard must display all appointments in a table or list format.
- Each appointment entry must show:
  - Date and time
  - Customer name
  - Barber name
  - List of services
  - **Calculated total price** (computed server-side, not stored)
- The calculated price must reflect all pricing rules from [Price_Calculation.md](./Price_Calculation.md).
- There must be a way to navigate to creating a new appointment.
- There must be a way to delete an appointment.

### Create New Appointment

As a user, I want to create a new appointment by filling out a form so that I can schedule customers.

**Acceptance criteria:**

- The form must allow entering:
  - Customer name (required)
  - Appointment date (required, must be Friday-Sunday only)
  - Start time (required)
  - Duration in minutes (required)
  - Barber selection (Gerrit or Todd)
  - One or more services (required)
  - Beverage choice (optional)
  - VIP status (checkbox)
- The form must validate:
  - Date is Friday, Saturday, or Sunday only
  - Selected services don't conflict (e.g., CleanShaven + BeardShaped)
  - Duration meets minimum requirements for selected services
  - If Gerrit is selected, time must be during peak hours
  - No time conflict with existing appointments for the selected barber
- Validation errors must be displayed clearly to the user.
- (Optional) Show a real-time price estimate as the user selects options.
- After successful creation, navigate back to the dashboard.

### Import Legacy Data

As a system administrator, I want to import appointment data from a broken XML file so that historical data can be migrated into the new system.

**Acceptance criteria:**

- The system must be able to read and fix the broken XML format described in [Import_Logic.md](./Import_Logic.md).
- Successfully parsed appointments must be inserted into the database.
- The import process must handle errors gracefully and report which records failed.

## Quality Requirements

### Starter Code

You must use the starter code from the folder [starter](./starter/). The technologies included there must be used:

- **Backend**: ASP.NET Core 9, Entity Framework Core (SQLite), Minimal API
- **Frontend**: Angular 19+, Standalone Components
- **Testing**: xUnit, NSubstitute

**Important**: You must use the newest Angular standards:

- Standalone components (no NgModules)
- Signals for state management
- Control flow syntax (@if, @for)

The following things are already implemented in the starter code:

- **Data model** (`AppServices/Model.cs`):
  - `Appointment` and `AppointmentService` entities
  - `StyleReference` enum with 18 service types
  - `ServiceMetadata` static class with base prices, minimum durations, and helper methods
  - Entity Framework Core context

- **Database infrastructure**:
  - SQLite database configuration
  - Migrations support
  - Test infrastructure with in-memory database

- **Web API skeleton** (`WebApi/Program.cs`, `AppointmentEndpoints.cs`):
  - Basic API setup with CORS
  - Empty endpoint group for appointments

- **Frontend skeleton**:
  - Routing configuration
  - Dashboard component with complete HTML/CSS (no TypeScript logic)
  - Editor component with complete HTML/CSS (no TypeScript logic)

- **Importer project structure**:
  - File reader implementation
  - Project configuration

- **Unit tests**:
  - Database test infrastructure
  - Example tests for database access

### Code to be Added

You must implement the following components:

#### Backend: Price Calculation Service

**File**: Create `AppServices/PriceCalculationService.cs`

Implement a service class that calculates appointment prices according to [Price_Calculation.md](./Price_Calculation.md).

**Requirements**:

- Must implement all 12 calculation steps **in the exact order specified**
- Must implement all 5 validation rules (weekday restriction, service conflicts, duration validation, barber availability, time conflict detection)
- Must query the database for:
  - Customer's previous appointments (for loyalty tier calculation)
  - Existing appointments (for barber conflict detection and group booking detection)
- Must return appropriate HTTP error codes (400, 409) with meaningful messages for validation failures
- Must use `ServiceMetadata` methods for base prices and minimum durations

**⚠️ Important**: Ignore any calculation-related comments in the frontend HTML files. Only [Price_Calculation.md](./Price_Calculation.md) is the authoritative specification.

#### Backend: Web API Endpoints

**File**: `WebApi/AppointmentEndpoints.cs`

Implement the following RESTful endpoints:

- `GET /appointments`: Retrieve all appointments with calculated prices
- `GET /appointments/{id}`: Retrieve a specific appointment by ID (return 404 if not found)
- `POST /appointments`: Create a new appointment with full validation
- `DELETE /appointments/{id}`: Delete an appointment (return 404 if not found)

Beware: an update endpoint is not required.

**Requirements**:

- All GET requests must include the calculated price for each appointment
- POST requests must validate using all rules from `PriceCalculationService`
- Return appropriate HTTP status codes (200, 201, 400, 404, 409)
- Use async/await properly

#### Backend: Data Importer

**File**: `Importer/LegacyFileFixer.cs` (create new file)

Implement the logic to read and fix broken XML files as described in [Import_Logic.md](./Import_Logic.md).

**Requirements**:

- Read broken XML stream
- Fix XML structure issues
- Parse fixed XML into `Appointment` objects
- Insert appointments into database

#### Frontend: Dashboard Component

**File**: `Frontend/src/app/dashboard/dashboard.ts`

Implement the TypeScript logic for the dashboard component.

**Requirements**:

- Fetch appointments from `GET /appointments` API endpoint
- Display appointments using the provided HTML template
- Handle loading and error states
- Implement delete functionality calling `DELETE /appointments/{id}`
- Navigate to editor for creating new appointments

#### Frontend: Editor Component

**File**: `Frontend/src/app/editor/editor.ts`

Implement the TypeScript logic for the appointment editor.

**Requirements**:

- Create a reactive form for appointment creation
- Populate service options dynamically
- Submit form data to `POST /appointments` API endpoint
- Handle validation errors from the API and display them to the user
- Calculate and display real-time price estimate
- Navigate back to dashboard on success

### Unit Tests 

- All tests must be written in **C# using xUnit** (not Angular tests)
- Tests must use the provided test infrastructure (`TestInfrastructure` project)
- You must create at least **3 unit tests** for price calculation and **2 unit tests** for validation rules
- Tests should cover both happy paths and edge cases
- Use in-memory database for integration tests where needed

### Example Test Cases for Validation Rules

- Test weekday restriction (Mon-Thu should be rejected with 400)
- Test service conflict (CleanShaven + BeardShaped should be rejected)
- Test duration validation (insufficient duration should be rejected)
- Test barber availability (Gerrit outside peak hours should be rejected)
- Test time conflict detection (overlapping appointment should return 409)

### Example Test Cases for Price Calculation

- Test base price calculation
- Test service count premium (2 services = +5%, 3+ services = +10%)
- Test combo discounts (hair+beard, package deal)
- Test payday surcharge (15th of month = +25%)
- Test Sunday premium (+€20)
- Test time modifiers (peak +30%, happy -15%, off-peak -20%)
- Test barber markup (Gerrit +20%, Todd -€5)
- Test duration fee (€2.50 per 15min over required minimum)
- Test loyalty tier discount (mock DB query, 0-15% based on history)
- Test group booking discount (mock DB query, 10-20% based on overlaps)
- Test VIP multiplier (×1.5 final step)
- Test complete calculation matching Example 4 from Price_Calculation.md

### Prohibited Modifications

**Do NOT modify the starter code** except in the locations specified above. Specifically:

- Do not change `AppServices/Model.cs`
- Do not change `AppServices/DataContext.cs`
- Do not change the frontend HTML/CSS files
- Do not change the project structure or configuration

You are implementing the **missing business logic**, not rewriting the architecture.

## Getting Started

1. **Open the solution**: Navigate to `starter/` and open `sln.sln`
2. **Review the specifications**:
   - Read [Price_Calculation.md](./Price_Calculation.md) carefully
   - Read [Import_Logic.md](./Import_Logic.md)
3. **Run the application**:
   ```bash
   cd starter
   dotnet run --project AppHost
   ```
4. **Start with tests**: Begin by writing unit tests for the price calculation logic (TDD approach recommended)
5. **Implement business logic**: Create `PriceCalculationService.cs` with all validation and calculation logic
6. **Implement API endpoints**: Complete the CRUD operations in `AppointmentEndpoints.cs`
7. **Implement frontend**: Add TypeScript logic to dashboard and editor components
8. **Test thoroughly**: Verify all user stories work end-to-end

## Evaluation Criteria

Your solution will be evaluated based on:

1. **Correctness of price calculation** (25%): All 12 steps implemented in correct order with proper validation
2. **Correctness of data import** (15%): All 12 steps implemented in correct order with proper validation
3. **Code quality** (20%): Clean architecture, separation of concerns, proper error handling
4. **Test coverage** (20%): Comprehensive unit tests covering edge cases
5. **API design** (10%): RESTful endpoints with proper HTTP semantics
6. **Frontend implementation** (10%): Working Angular application using modern standards

**Note**: The price calculation is the most complex and important part of this exercise. It must be implemented correctly with all steps in the exact order specified.
