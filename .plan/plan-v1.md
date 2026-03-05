# Product Requirements Document (PRD): Cinema Express (Serverless-First MVP)

## 1. Project Overview

A frictionless, high-performance movie booking application. It prioritizes speed by eliminating the Login/Sign-up phase and uses **IndexedDB** for client-side ticket persistence. The backend follows a **Vertical Slice / MVC API** pattern to manage global seat inventory.

## 2. System Flow (The "Linear Path")

1. **Movie Hub:** User lands on the home page and sees all movies immediately.
2. **Date Selection (Priority):** User picks a **Date** first to filter which Cinemas and Theaters are showing movies on that day.
3. **Location Selection:** User selects **Cinema** -> **Theater (Hall)**.
4. **Time Slot Selection:** User picks a specific **Show Time**.
5. **Interactive Seat Map:** User selects seats. (System checks against Database for availability).
6. **Stripe-Style Checkout:** Credit card payment (Visa/Mastercard).
7. **Local Fulfillment:** Upon success, the ticket is saved to **IndexedDB** for offline access and future reference.

---

## 3. Functional Requirements

### 3.1 Seamless Discovery (No-Auth)

* **Requirement:** The app must allow full navigation to the seat selection and payment pages without an `Authorize` attribute.
* **Benefit:** Reduces bounce rate and speeds up the "Time-to-Ticket."

### 3.2 Show & Schedule Logic

* **Date-First Filtering:** The UI must prioritize the **Show Date**.
* *Validation:* Show Times must be dynamically filtered based on the selected Date + Theater + Movie.


* **Cinema/Theater Hierarchy:** * A Movie can play in multiple **Cinemas**.
* A Cinema can have multiple **Theaters (Halls)**.



### 3.3 Global Seat State Management

* **Real-time Availability:** When a user selects a Show Time, the API must return the current state of the `SeatStatus` table.
* **Conflict Prevention:** * Seats marked as `Sold` or `Reserved` in the SQL Database must be disabled in the Blazor Seat Map.
* **Related Seats:** If a user selects a "Couple Seat," the system must logically group and lock both seats together.



### 3.4 Payment & Local Storage (IndexedDB)

* **Payment Simulation:** A secure-looking UI for Visa/Mastercard entry.
* **Post-Payment Action:** 1.  **Backend:** Mark seats as `Sold`.
2.  **Frontend:** Write the Booking Object to **IndexedDB**.
* **IndexedDB Schema:**
* `StoreName`: `Tickets`
* `KeyPath`: `BookingId` (GUID)
* `Indexes`: `MovieTitle`, `ShowDate`.



---

## 4. Technical Architecture Alignment

### 4.1 Frontend (Blazor WASM)

* **State Container:** Use a Scoped Service to hold the "Current Booking" state (MovieId, CinemaId, SelectedSeats) as the user navigates through the steps.
* **JS Interop:** Since Blazor doesn't have a native IndexedDB wrapper in the standard library, ensure your **JS Interop** or a library (like `Blazor.IndexedDB`) is correctly configured.

### 4.2 Backend (MVC API / Domain)

* **Flow:** `Controller` -> `MediatR Handler (Vertical Slice)` -> `Domain Logic` -> `EF Core / Database`.
* **Atomic Transactions:** Ensure that when a payment is "confirmed," the database update for seat status is atomic to prevent overbooking.

---

## 5. Implementation Checklist (Review these against your code)

| Feature | Status | Verification Detail |
| --- | --- | --- |
| **No-Auth Access** | [ ] | Check if `[AllowAnonymous]` is on all booking APIs. |
| **Date-First Filter** | [ ] | Does selecting a date refresh the Theater/Time list? |
| **Seat Locking** | [ ] | Do already-purchased seats show as "Greyed Out"? |
| **Payment UI** | [ ] | Is there a Visa/Mastercard input form before the final save? |
| **IndexedDB Save** | [ ] | After payment, does a record appear in F12 -> Application -> IndexedDB? |
| **Related Seats** | [ ] | If I click seat A1 (Couple), does A2 also highlight? |

---

### Would you like me to focus on any specific part for the next step?

1. **Database Schema (SQL):** To handle the Movie/Cinema/Theater relationship.
2. **Blazor Service (C#):** A sample `IndexedDBService.cs` to handle ticket saving.
3. **UI Layout:** A mockup for the "Date-First" selection screen.