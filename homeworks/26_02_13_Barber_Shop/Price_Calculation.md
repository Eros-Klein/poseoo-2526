# Price Calculation Specification

## Overview

The barber shop uses a highly complex pricing model that calculates the **total price per appointment** based on multiple factors including service combinations, time-based pricing, customer loyalty history, and group bookings. The price is calculated server-side and requires querying the database for historical data.

## Base Pricing by Service Type

Each `StyleReference` enum value has a different base price:

| Style Reference | Base Price | Min. Duration (min) |
| --------------- | ---------- | ------------------- |
| Short           | €25.00     | 20                  |
| Medium          | €30.00     | 25                  |
| Long            | €35.00     | 30                  |
| Faded           | €40.00     | 30                  |
| Tapered         | €38.00     | 30                  |
| Undercut        | €42.00     | 35                  |
| Layered         | €45.00     | 40                  |
| Textured        | €48.00     | 40                  |
| SlickedBack     | €35.00     | 25                  |
| SideParted      | €32.00     | 25                  |
| ForwardCrop     | €38.00     | 30                  |
| Voluminous      | €50.00     | 45                  |
| Natural         | €28.00     | 20                  |
| MulletStyle     | €60.00     | 50                  |
| MohawkStyle     | €65.00     | 50                  |
| BeardShaped     | €15.00     | 10                  |
| CleanShaven     | €12.00     | 10                  |
| HotTowelShave   | €18.00     | 15                  |

## Validation Rules (Must Check BEFORE Price Calculation)

### 1. Weekday Restriction

- Appointments **MUST NOT** be scheduled Monday through Thursday
- Valid days: **Friday, Saturday, Sunday only**
- API should return `400 Bad Request` with message: "Gerrit's Cuts is closed Monday-Thursday. We value our leisure time."

### 2. Service Conflicts

Certain service combinations are **invalid** and must be rejected:

- **Conflicting beard services**: `CleanShaven` + `BeardShaped` (cannot be both)
- **Multiple length categories**: Cannot have more than one of: `Short`, `Medium`, `Long`
- If conflicts detected, return `400 Bad Request` with message: "Service combination conflict detected: [service1] and [service2] cannot be booked together."

### 3. Duration Validation

- Total appointment `Duration` must be **greater than or equal to** the sum of all service minimum durations
- Formula: `Duration >= Σ(Service.MinDuration)`
- If invalid, return `400 Bad Request` with message: "Appointment duration (X min) is insufficient for selected services (requires minimum Y min)."

### 4. Barber Availability

- **Gerrit** only works during peak hours: Friday 16:00-20:00, Saturday-Sunday 10:00-18:00
- If appointment with Gerrit falls outside these hours, return `400 Bad Request` with message: "Gerrit only works during peak hours (Fri 16:00-20:00, Sat-Sun 10:00-18:00)."
- **Todd** can work any valid business hours

### 5. Barber Time Conflict Detection

- Query database for existing appointments for the same barber
- Check if new appointment time overlaps with existing appointments (same date, overlapping start/end times)
- If conflict exists, return `409 Conflict` with message: "Time slot unavailable. [BarberName] already has an appointment at this time."

## Pricing Rules (Applied in Order)

### Step 1: Calculate Base Price

Start with the sum of all service base prices:

```
basePrice = Σ(Service.StyleReference.BasePrice)
```

### Step 2: Service Count Premium

Charge extra if customer books multiple services:

- **1 service**: No premium
- **2 services**: +5% (customer is needy)
- **3+ services**: +10% (very needy customer)

```
if (serviceCount == 2) basePrice *= 1.05
else if (serviceCount >= 3) basePrice *= 1.10
```

### Step 3: Service Combination Discounts

Apply automatic discounts for specific combinations:

**Combo A - Hair + Beard**: If appointment includes at least one haircut service AND at least one beard service:

- Determine which are haircut services: Any of `Short`, `Medium`, `Long`, `Faded`, `Tapered`, `Undercut`, `Layered`, `Textured`, `SlickedBack`, `SideParted`, `ForwardCrop`, `Voluminous`, `Natural`, `MulletStyle`, `MohawkStyle`
- Determine which are beard services: Any of `BeardShaped`, `CleanShaven`, `HotTowelShave`
- If BOTH categories present: Apply 10% discount to the cheapest beard service

**Combo B - Package Deal**: If appointment has 3 or more services total:

- Apply 15% discount to the entire current total (stacks with Combo A if applicable)

**Important**: Combo discounts apply to the price AFTER service count premium

### Step 4: Payday Surcharge

- If appointment date is the **15th of any month**: +25% surcharge
- Rationale: Customers just got paid

### Step 5: Sunday Premium

- If appointment is on **Sunday**: +€20.00 flat fee
- Rationale: Weekend work premium

### Step 6: Time-Based Pricing Modifiers

Apply ONE of the following based on day and time:

**Peak Hours** (+30% surcharge):

- Friday: 16:00-20:00
- Saturday: 10:00-18:00
- Sunday: 10:00-18:00

**Happy Hours** (-15% discount):

- Friday: 14:00-16:00 (just before peak)

**Off-Peak/Dead Hours** (-20% discount):

- Friday: 08:00-10:00 (early morning)

**Important**: These modifiers are mutually exclusive. Check in order: Peak → Happy → Off-Peak

### Step 7: Barber-Specific Markup

- If `BarberName == "Gerrit"`: +20% ("Master Artisan Fee")
- If `BarberName == "Todd"`: -€5.00 (apprentice discount)
- Else: No adjustment

### Step 8: Duration Fee

For every 15 minutes beyond the **required minimum duration** (not the standard 30 min baseline):

- Calculate required minimum: `requiredMin = Σ(Service.MinDuration)`
- If `Duration > requiredMin`: charge €2.50 per 15-minute increment

```
extraMinutes = Duration - requiredMin
if (extraMinutes > 0) {
    increments = ceil(extraMinutes / 15)
    durationFee = increments * 2.50
}
```

### Step 9: Beverage Surcharge

- If `BeverageChoice` is not null/empty: +€8.00
- Rationale: Premium beverage service

### Step 10: Customer Loyalty Tier Discount

**Requires Database Query**: Count the number of **previous** appointments for this `CustomerName` (exclude current appointment):

| Tier     | Previous Appointments | Discount |
| -------- | --------------------- | -------- |
| Bronze   | 0-2                   | 0%       |
| Silver   | 3-5                   | 5%       |
| Gold     | 6-10                  | 10%      |
| Platinum | 11+                   | 15%      |

Apply the discount to the running total.

### Step 11: Group Booking Discount

**Requires Database Query**: Check if there are other appointments with the same `CustomerName` on the same `Date` where the time ranges overlap (within ±30 minutes):

- Count appointments where:
  - Same `CustomerName`
  - Same `Date`
  - Time ranges overlap: `|StartTime1 - StartTime2| <= 30 minutes`

| Group Size | Discount |
| ---------- | -------- |
| 1 person   | 0%       |
| 2-3 people | 10%      |
| 4+ people  | 20%      |

**Implementation Note**: Students must detect overlapping time ranges and count group members.

### Step 12: VIP Multiplier (Final Step)

- If `IsVip == true`: Multiply entire total by **1.5x**
- Rationale: VIP customers receive premium service and pay premium prices
- This is the **final** calculation step

## Calculation Order Summary

1. ✅ Base price (sum of services)
2. ✅ Service count premium (+5% or +10%)
3. ✅ Combo discounts (-10% on beard service, -15% package deal)
4. ✅ Payday surcharge (+25% if 15th)
5. ✅ Sunday premium (+€20)
6. ✅ Time-based modifier (+30%, -15%, or -20%)
7. ✅ Barber markup (Gerrit +20%, Todd -€5)
8. ✅ Duration fee (€2.50 per 15 min over required minimum)
9. ✅ Beverage surcharge (+€8)
10. ✅ Loyalty tier discount (0-15% based on history)
11. ✅ Group booking discount (0-20% based on overlaps)
12. ✅ VIP multiplier (×1.5)

## Implementation Requirements

### Backend Tasks

1. **Create a `PriceCalculationService`** with method `CalculatePrice(Appointment appt)`
2. **Implement all validation rules** with appropriate HTTP status codes
3. **Query database** for:
   - Existing appointments (barber conflict detection)
   - Customer appointment history (loyalty tier)
   - Group bookings (time overlap detection)
4. **Add `decimal CalculatedPrice { get; }` property** to `Appointment` entity (computed, not stored)
5. **Include calculated price in API responses**

### Frontend Tasks

- Display calculated price in dashboard
- Show real-time price estimate in editor as user selects options
- Display helpful error messages for validation failures

## Example Calculations

### Example 1: Simple Weekday Booking (Invalid)

**Input:**

- Date: Monday, March 10
- Services: Faded Cut

**Result:** `400 Bad Request` - "Gerrit's Cuts is closed Monday-Thursday."

---

### Example 2: Service Conflict (Invalid)

**Input:**

- Date: Friday, March 14
- Services: CleanShaven + BeardShaped

**Result:** `400 Bad Request` - "Service combination conflict: CleanShaven and BeardShaped cannot be booked together."

---

### Example 3: Standard Friday Appointment

**Input:**

- Services: Faded (€40)
- Date: Friday, March 7, 10:00
- Duration: 45 min
- Barber: Todd
- Previous appointments: 4 (Silver tier)
- Beverage: None
- VIP: No

**Calculation:**

1. Base: €40.00
2. Service count: 1 service → no premium
3. Combo: No combo
4. Payday: Not 15th → no surcharge
5. Sunday: Not Sunday → no premium
6. Time modifier: 10:00 Friday → no modifier
7. Barber (Todd): €40 - €5 = €35.00
8. Duration: Required min = 30, actual = 45, extra = 15 → €2.50
   - Total: €35 + €2.50 = €37.50
9. Beverage: None
10. Loyalty (Silver, 4 prev): €37.50 × 0.95 = €35.63
11. Group: No overlaps → no discount
12. VIP: No

**Final Price: €35.63**

---

### Example 4: Complex VIP Weekend Package

**Input:**

- Services: Undercut (€42), BeardShaped (€15), HotTowelShave (€18)
- Date: Sunday, June 15, 15:00
- Duration: 90 min
- Barber: Gerrit
- Previous appointments: 12 (Platinum tier)
- Beverage: "Champagne"
- VIP: Yes
- Group: 3 people (overlapping times detected)

**Calculation:**

1. Base: €42 + €15 + €18 = €75.00
2. Service count: 3 services → €75 × 1.10 = €82.50
3. Combo discounts:
   - Hair + Beard combo: 10% off cheapest beard (€15) → -€1.50 = €81.00
   - Package deal (3 services): €81 × 0.85 = €68.85
4. Payday (15th): €68.85 × 1.25 = €86.06
5. Sunday: €86.06 + €20 = €106.06
6. Time (Sunday 15:00 = peak): €106.06 × 1.30 = €137.88
7. Barber (Gerrit): €137.88 × 1.20 = €165.46
8. Duration: Required = 35+10+15 = 60 min, actual = 90, extra = 30
   - Increments: ceil(30/15) = 2 → €5.00
   - Total: €165.46 + €5 = €170.46
9. Beverage: €170.46 + €8 = €178.46
10. Loyalty (Platinum, 12 prev): €178.46 × 0.85 = €151.69
11. Group (3 people): €151.69 × 0.90 = €136.52
12. VIP: €136.52 × 1.5 = €204.78

**Final Price: €204.78**

---

## Deliverables

You must implement:

1. Service conflict detection algorithm
2. Time overlap detection for barber conflicts and group bookings
3. Database queries for loyalty tier calculation
4. Multi-step price calculation engine following exact order
5. Comprehensive validation with meaningful error responses
6. Unit tests covering edge cases and calculation scenarios
