# ConcertTicketing
Alaska assessment

# Concert Ticketing System - API Documentation

This document provides comprehensive documentation for all the API endpoints available in the Concert Ticketing System.

## Base URL
```
https://localhost:7071/api
```

## Authentication
Most endpoints require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

---

## 🔐 Authentication Endpoints

### 1. Register User
**POST** `/api/auth/register`

Register a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "message": "User registered successfully."
}
```

**Response (400 Bad Request):**
```json
{
  "errors": [
  {
    "code": "PasswordRequiresNonAlphanumeric",
    "description": "Passwords must have at least one non alphanumeric character."
  },
  {
    "code": "PasswordRequiresUpper",
    "description": "Passwords must have at least one uppercase ('A'-'Z')."
 }
]
}
```

---

### 2. Login User
**POST** `/api/auth/login`

Authenticate user and receive JWT token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-08-09T14:30:00Z"
}
```

**Response (401 Unauthorized):**
```json
"Invalid credentials"
```

---

## 🎪 Events Endpoints

### 3. Get All Events
**GET** `/api/events`

Retrieve all events with their ticket types.

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Summer Music Festival",
    "venue": "Central Park",
    "description": "Annual summer music festival featuring top artists",
    "eventDate": "2025-08-15T19:00:00Z",
    "capacity": 5000,
    "ticketTypes": [
      {
        "id": 1,
        "type": "General Admission",
        "price": 75.00,
        "totalQuantity": 3000,
        "availableQuantity": 2850,
        "eventId": 1
      },
      {
        "id": 2,
        "type": "VIP",
        "price": 150.00,
        "totalQuantity": 500,
        "availableQuantity": 450,
        "eventId": 1
      }
    ]
  }
]
```

---

### 4. Create Event
**POST** `/api/events` 🔒 *Requires Authentication*

Create a new event.

**Request Body:**
```json
{
  "name": "Jazz Night",
  "venue": "Blue Note Club",
  "description": "Intimate jazz performance with renowned artists",
  "eventDate": "2025-09-20T20:00:00Z",
  "capacity": 200,
  "ticketTypes": [
    {
      "type": "Standard",
      "price": 45.00,
      "totalQuantity": 150,
      "availableQuantity": 150
    },
    {
      "type": "Premium",
      "price": 75.00,
      "totalQuantity": 50,
      "availableQuantity": 50
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "id": 2,
  "name": "Jazz Night",
  "venue": "Blue Note Club",
  "description": "Intimate jazz performance with renowned artists",
  "eventDate": "2025-09-20T20:00:00Z",
  "capacity": 200,
  "ticketTypes": [
    {
      "id": 3,
      "type": "Standard",
      "price": 45.00,
      "totalQuantity": 150,
      "availableQuantity": 150,
      "eventId": 2
    },
    {
      "id": 4,
      "type": "Premium",
      "price": 75.00,
      "totalQuantity": 50,
      "availableQuantity": 50,
      "eventId": 2
    }
  ]
}
```

---

### 5. Update Event
**PUT** `/api/events/{id}` 🔒 *Requires Authentication*

Update an existing event by ID.

**URL Parameters:**
- `id` (integer): Event ID

**Request Body:**
```json
{
  "name": "Jazz Night - Updated",
  "venue": "Blue Note Club - Main Stage",
  "description": "Updated: Intimate jazz performance with world-renowned artists",
  "eventDate": "2025-09-20T20:30:00Z",
  "capacity": 250,
  "ticketTypes": [
    {
      "type": "Standard",
      "price": 50.00,
      "totalQuantity": 180,
      "availableQuantity": 180
    },
    {
      "type": "Premium",
      "price": 80.00,
      "totalQuantity": 70,
      "availableQuantity": 70
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "id": 2,
  "name": "Jazz Night - Updated",
  "venue": "Blue Note Club - Main Stage",
  "description": "Updated: Intimate jazz performance with world-renowned artists",
  "eventDate": "2025-09-20T20:30:00Z",
  "capacity": 250,
  "ticketTypes": [
    {
      "id": 5,
      "type": "Standard",
      "price": 50.00,
      "totalQuantity": 180,
      "availableQuantity": 180,
      "eventId": 2
    },
    {
      "id": 6,
      "type": "Premium",
      "price": 80.00,
      "totalQuantity": 70,
      "availableQuantity": 70,
      "eventId": 2
    }
  ]
}
```

**Response (404 Not Found):**
```json
"Event not found"
```

---

### 6. Cancel Event
**DELETE** `/api/events/{id}` 🔒 *Requires Authentication*

Cancel (delete) an event by ID.

**URL Parameters:**
- `id` (integer): Event ID

**Response (200 OK):**
```json
"Event cancelled has been cancelled"
```

**Response (404 Not Found):**
```json
"Event not found"
```

---

## 🎫 Tickets Endpoints

### 7. Get Ticket Availability
**GET** `/api/tickets/availability?eventId={eventId}`

Get available tickets for a specific event.

**Query Parameters:**
- `eventId` (integer): Event ID

**Response (200 OK):**
```json
[
  {
    "type": "General Admission",
    "price": 75.00,
    "availableQuantity": 2850
  },
  {
    "type": "VIP",
    "price": 150.00,
    "availableQuantity": 450
  }
]
```

---

### 8. Reserve Tickets
**POST** `/api/tickets/reservation` 🔒 *Requires Authentication*

Reserve tickets for an event. Reservations expire after 15 minutes.

**Request Body:**
```json
{
  "eventId": 1,
  "ticketTypeId": 1,
  "quantity": 2
}
```

**Response (200 OK):**
```json
{
  "id": 101,
  "eventId": 1,
  "userId": "user@example.com",
  "ticketTypeId": 1,
  "quantity": 2,
  "reservedAt": "2025-08-09T12:00:00Z",
  "expireAt": "2025-08-09T12:15:00Z",
  "isPurchased": false,
  "ticketType": null
}
```

**Response (400 Bad Request):**
```json
"Insufficient tickets available"
```

---

### 9. Get Reservation Details
**GET** `/api/tickets/reservation/{id}` 🔒 *Requires Authentication*

Get details of a specific reservation.

**URL Parameters:**
- `id` (integer): Reservation ID

**Response (200 OK):**
```json
{
  "id": 101,
  "eventId": 1,
  "userId": "user@example.com",
  "ticketTypeId": 1,
  "quantity": 2,
  "reservedAt": "2025-08-09T12:00:00Z",
  "expireAt": "2025-08-09T12:15:00Z",
  "isPurchased": false,
  "ticketType": {
    "id": 1,
    "type": "General Admission",
    "price": 75.00,
    "totalQuantity": 3000,
    "availableQuantity": 2848,
    "eventId": 1
  }
}
```

---

### 10. Cancel Reservation
**DELETE** `/api/tickets/reservations/{id}` 🔒 *Requires Authentication*

Cancel a ticket reservation (only if not purchased).

**URL Parameters:**
- `id` (integer): Reservation ID

**Response (200 OK):**
```json
"Reservation cancelled and tickets released"
```

**Response (404 Not Found):**
```json
"Reservation not found"
```

**Response (400 Bad Request):**
```json
"Cannot cancel a reservation that has been purchased"
```

---

### 11. Purchase Reserved Tickets
**POST** `/api/tickets/purchases` 🔒 *Requires Authentication*

Purchase previously reserved tickets.

**Request Body:**
```json
{
  "reservationId": 101
}
```

**Response (200 OK):**
```json
"Purchase confirmed"
```

**Response (400 Bad Request):**
```json
"Invalid or expired reservation"
```

---

## 💰 Sales Endpoints

### 12. Get Total Sales for Event
**GET** `/api/sales/totals/{eventId}` 🔒 *Requires Authentication*

Get total revenue and tickets sold for a specific event.

**URL Parameters:**
- `eventId` (integer): Event ID

**Response (200 OK):**
```json
{
  "totalRevenue": 15750.00,
  "totalTicketsSold": 157
}
```

---

## 📋 Data Models

### Event
```json
{
  "id": 1,
  "name": "string",
  "venue": "string",
  "description": "string",
  "eventDate": "2025-08-15T19:00:00Z",
  "capacity": 5000,
  "ticketTypes": [...]
}
```

### TicketType
```json
{
  "id": 1,
  "type": "string",
  "price": 75.00,
  "totalQuantity": 3000,
  "availableQuantity": 2850,
  "eventId": 1
}
```

### TicketReservation
```json
{
  "id": 101,
  "eventId": 1,
  "userId": "string",
  "ticketTypeId": 1,
  "quantity": 2,
  "reservedAt": "2025-08-09T12:00:00Z",
  "expireAt": "2025-08-09T12:15:00Z",
  "isPurchased": false,
  "ticketType": {...}
}
```

### Sale
```json
{
  "id": 201,
  "eventId": 1,
  "reservationId": 101,
  "totalPrice": 150.00,
  "purchasedAt": "2025-08-09T12:10:00Z",
  "reservation": {...}
}
```

---

## 🚨 Error Handling

### Common HTTP Status Codes

- **200 OK**: Successful request
- **400 Bad Request**: Invalid request data or business logic error
- **401 Unauthorized**: Authentication required or invalid credentials
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

### Common Error Responses

**Validation Error (400):**
```json
{
  "errors": [
    {
      "code": "ValidationError",
      "description": "Email and password are required"
    }
  ]
}
```

**Authentication Error (401):**
```json
"Invalid credentials"
```

**Not Found Error (404):**
```json
"Event not found"
```

**Business Logic Error (400):**
```json
"Insufficient tickets available"
```

---

## 🔄 Workflow Examples

### Complete Ticket Purchase Flow

1. **Get Events**: `GET /api/events`
2. **Check Availability**: `GET /api/tickets/availability?eventId=1`
3. **Register/Login**: `POST /api/auth/register` or `POST /api/auth/login`
4. **Reserve Tickets**: `POST /api/tickets/reservation`
5. **Purchase Tickets**: `POST /api/tickets/purchases`
6. **Check Sales**: `GET /api/sales/totals/1` (admin only)

### Event Management Flow

1. **Login**: `POST /api/auth/login`
2. **Create Event**: `POST /api/events`
3. **Update Event**: `PUT /api/events/{id}`
4. **Monitor Sales**: `GET /api/sales/totals/{eventId}`
5. **Cancel Event**: `DELETE /api/events/{id}` (if needed)

---

## 📝 Notes

- **Reservations expire after 15 minutes** if not purchased
- **Expired reservations are automatically cleaned up** when new operations are performed
- **All authenticated endpoints require a valid JWT token** in the Authorization header
- **Ticket quantities are automatically managed** (reduced on reservation, restored on cancellation)
- **Events can only be modified by authenticated users**
- **Sales data is only accessible to authenticated users**
API_DOCUMENTATION.md
Displaying API_DOCUMENTATION.md.
