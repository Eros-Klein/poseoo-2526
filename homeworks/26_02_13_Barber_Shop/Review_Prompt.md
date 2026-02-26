# Code Review Prompt: Barber Shop Management System

## Your Role

You are an expert code reviewer evaluating a student's solution to a full-stack programming exercise. Your task is to perform a thorough, constructive code review focusing on correctness, code quality, and best practices.

## Exercise Context

### What Was Provided (Starter Code)

Students received a partial ASP.NET Core + Angular application with:

1. **Data Model** (`AppServices/Model.cs`):
   - `Appointment` and `AppointmentService` entities
   - `StyleReference` enum (18 service types)
   - `ServiceMetadata` static class with base prices and minimum durations
   - EF Core `ApplicationDataContext`

2. **Empty Web API** (`WebApi/AppointmentEndpoints.cs`):
   - Skeleton with endpoint group setup
   - TODO comments for GET, POST, DELETE implementations

3. **Frontend Skeleton** (Angular):
   - Dashboard HTML with beautiful CSS (no TypeScript logic)
   - Editor form HTML with beautiful CSS (no TypeScript logic)
   - Routing structure

4. **Importer Project**:
   - `FileReader` implementation
   - Empty importer logic

### What Students Had to Implement

#### 1. Price Calculation Service (Backend) - **CRITICAL COMPONENT**

Complex 12-step price calculation algorithm with:

- **Validation rules** (5 different checks before calculation)
- **Database queries** for loyalty tier and group booking calculations
- **Service combination detection** (combo discounts and conflicts)
- **Time-based pricing modifiers** (peak hours, happy hours, off-peak)
- **Multi-step calculation** in specific order

See `Price_Calculation.md` for complete specification.

#### 2. Web API Endpoints (Backend)

- GET /appointments (retrieve all)
- GET /appointments/{id} (retrieve by ID)
- POST /appointments (create with validation)
- DELETE /appointments/{id} (delete)

Must include proper error handling and validation.

#### 3. Data Importer (Backend)

- Read broken XML files
- Fix XML structure issues
- Parse and import appointment data
- See `Import_Logic.md` for specification

#### 4. Frontend Integration (Angular)

- Implement TypeScript logic for Dashboard and Editor components
- Connect to backend API
- Display calculated prices
- Handle form submissions
- Use newest Angular standards (standalone components, signals, control flow)

---

## Code Review Quality Criteria

### Critical: Price Calculation Correctness

The price calculation is the **most complex and important** part of this exercise. Review with extreme scrutiny:

**✅ Must Have - Calculation Steps in EXACT Order:**

1. Base price calculation (sum of services)
2. Service count premium (+5% for 2, +10% for 3+)
3. Combo discounts (hair+beard -10% on cheapest beard, 3+ services -15%)
4. Payday surcharge (+25% on 15th)
5. Sunday premium (+€20)
6. Time-based modifiers (peak +30%, happy -15%, off-peak -20%)
7. Barber markup (Gerrit +20%, Todd -€5)
8. Duration fee (€2.50 per 15min over required minimum)
9. Beverage surcharge (+€8)
10. Loyalty tier discount (requires DB query: 0-15% based on history)
11. Group booking discount (requires DB query: 10-20% based on overlaps)
12. VIP multiplier (×1.5 final step)

**❌ Common Mistakes to Flag:**

- Steps out of order (e.g., applying VIP before discounts)
- Forgetting to query database for loyalty tier or group bookings
- Incorrect combo detection logic (not checking both haircut AND beard)
- Using wrong duration baseline (should use required minimum, not 30 min)
- Not handling edge cases (e.g., what if no services? zero duration?)
- Applying discounts before surcharges or vice versa

**✅ Must Have - Validation Rules:**

1. **Weekday restriction**: Reject Monday-Thursday (400 Bad Request)
2. **Service conflicts**: Reject CleanShaven+BeardShaped, multiple lengths (400)
3. **Duration validation**: Total duration >= sum of service minimums (400)
4. **Barber availability**: Gerrit only works peak hours (400)
5. **Time conflict detection**: No overlapping appointments for same barber (409 Conflict)

**❌ Common Mistakes:**

- Missing validation checks entirely
- Wrong HTTP status codes
- Poor error messages
- Not checking database for existing appointments

### Database Query Implementation

**✅ Good Code:**

- Correctly queries previous appointments for loyalty tier calculation
- Implements time overlap detection for group bookings and barber conflicts
- Uses async/await properly
- Handles edge cases (no previous appointments, no overlaps)

**❌ Bad Code:**

- Hardcoded loyalty tier instead of querying database
- Missing group booking calculation
- Synchronous database calls
- Not checking for null results

### Service Combination Logic

**✅ Good Code:**

- Uses `ServiceMetadata.IsHaircutService()` and `IsBeardService()`
- Correctly identifies cheapest beard service for discount
- Handles cases where combo doesn't apply

**❌ Bad Code:**

- Hardcoded service type checks instead of using metadata methods
- Always applying combo discount regardless of service types
- Not finding cheapest service for hair+beard combo

### Code Organization and Architecture

**✅ Good Code:**

- Price calculation in separate service class (`PriceCalculationService`)
- Validation logic separate from calculation logic
- Use of dependency injection
- Clear method names and responsibilities
- Proper separation of concerns

**❌ Bad Code:**

- All logic in controller methods
- God classes with too many responsibilities
- Mixing validation and calculation
- Business logic in entity models

### Error Handling

**✅ Good Code:**

- Meaningful error messages with specific details
- Correct HTTP status codes (400, 404, 409, etc.)
- Validation results returned to frontend
- No swallowed exceptions

**❌ Bad Code:**

- Generic error messages ("Invalid input")
- Wrong status codes (always 500)
- Try-catch blocks that hide problems
- No validation feedback

### Testing

**✅ Good Code:**

- Unit tests for price calculation covering all 12 steps
- Tests for edge cases (15th of month, Sunday, peak hours, etc.)
- Tests for validation rules
- Tests for service combinations
- Integration tests for API endpoints

**❌ Bad Code:**

- No tests
- Only happy-path tests
- Tests that don't verify calculation order
- Missing edge case coverage

### Frontend Implementation

**✅ Good Code:**

- Uses Angular 19+ standalone components
- Uses signals for state management
- Uses new control flow (@if, @for)
- Proper API integration with error handling
- Real-time price estimate in editor

**❌ Bad Code:**

- Uses NgModules (outdated)
- Uses RxJS Observables instead of signals
- Uses *ngIf/*ngFor (old syntax)
- Poor error handling
- No price estimate

### API Design

**✅ Good Code:**

- RESTful endpoint design
- Proper use of HTTP methods
- Returns calculated price in responses
- Consistent error format

**❌ Bad Code:**

- Non-RESTful endpoints (e.g., /getAppointment instead of GET /appointments/{id})
- Always using POST
- Missing calculated price in responses

---

## Code Review Structure

Provide your review in the following format:

### 1. Executive Summary

- Overall assessment (Excellent / Good / Acceptable / Needs Improvement / Unsatisfactory)
- Key strengths (2-3 bullet points)
- Critical issues (2-3 bullet points)
- Recommendation (Pass / Pass with Minor Revisions / Major Revisions Required / Fail)

### 2. Price Calculation Analysis (Most Important Section)

#### 2.1 Calculation Steps Verification

For EACH of the 12 steps:

- ✅ or ❌ Step implemented correctly
- Notes on implementation quality or issues

#### 2.2 Validation Rules Review

For EACH of the 5 validation rules:

- ✅ or ❌ Rule implemented
- HTTP status code used
- Error message quality

#### 2.3 Database Queries

- Loyalty tier calculation: Implementation quality
- Group booking detection: Implementation quality
- Time overlap logic: Correctness

#### 2.4 Edge Cases Handling

List which edge cases are handled (e.g., zero services, invalid dates, etc.)

### 3. Code Quality Assessment

#### 3.1 Architecture & Organization

- Separation of concerns rating (1-5)
- Use of dependency injection
- Service layer design

#### 3.2 Error Handling

- Error handling strategy
- HTTP status code usage
- Error message quality

#### 3.3 Code Readability

- Naming conventions
- Code comments where needed
- Complexity of methods

### 4. Testing Coverage

- Unit tests present: Yes/No
- Integration tests present: Yes/No
- Edge case coverage: Good/Fair/Poor
- Critical gaps in test coverage

### 5. Frontend Implementation

- Angular standards compliance (standalone, signals, control flow)
- API integration quality
- Error handling in UI
- User experience

### 6. Detailed Issues and Recommendations

For each significant issue:

#### Issue: [Title]

- **Severity**: Critical / Major / Minor
- **Location**: File and line numbers
- **Description**: What's wrong
- **Impact**: Why it matters
- **Recommendation**: How to fix
- **Example**: (if applicable) Code snippet showing correct implementation

### 7. Positive Highlights

List 3-5 things the student did particularly well.

### 8. Overall Recommendations

- Priority fixes (must do)
- Suggested improvements (should do)
- Nice-to-haves (could do)

---

## Important Notes for Reviewers

0. The solution will be evaluated on the following criteria, you must not follow the percentages exactly, but use them as a guide:

1. **Correctness of price calculation** (25%): All 12 steps implemented in correct order with proper validation
2. **Correctness of data import** (15%): All 12 steps implemented in correct order with proper validation
3. **Code quality** (20%): Clean architecture, separation of concerns, proper error handling
4. **Test coverage** (20%): Comprehensive unit tests covering edge cases
5. **API design** (10%): RESTful endpoints with proper HTTP semantics
6. **Frontend implementation** (10%): Working Angular application using modern standards

7. **Focus on correctness first**: The 12-step price calculation MUST be correct and in order. This is non-negotiable.

8. **Database queries are required**: If loyalty tier or group booking is hardcoded/skipped, this is a critical failure.

9. **Be constructive**: Point out what's wrong AND how to fix it. Provide examples.

10. **Context matters**: Students had starter code with hints. Evaluate based on what they had to implement, not the entire codebase.

11. **Modern Angular is required**: Using old patterns (NgModules, \*ngIf) when new ones were specified is a problem.

12. **Testing is important**: Complex logic like price calculation needs tests. Lack of tests for critical paths is a major issue.

13. **Be fair but rigorous**: This is a complex exercise. Students should demonstrate understanding of multi-step calculations, database queries, and validation logic. Anything less is insufficient.

---

## Begin Your Review

Now review the provided student solution using the criteria and structure above. Be thorough, constructive, and fair.
