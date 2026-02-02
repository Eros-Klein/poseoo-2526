# Backend - Express Server with SSE

Express backend server providing real-time chat functionality using Server-Sent Events.

## Features

- Server-Sent Events (SSE) for real-time message broadcasting
- RESTful API for sending messages
- CORS enabled for cross-origin requests
- TypeScript for type safety
- Active connection tracking

## Scripts

```bash
# Development mode with hot reload
npm run dev

# Build for production
npm run build

# Run production build
npm start
```

## API Endpoints

### GET /api/sse
Establishes a Server-Sent Events connection for receiving real-time messages.

**Response:** Event stream with chat messages

### POST /api/messages
Send a new chat message to all connected clients.

**Request Body:**
```json
{
  "text": "Hello, world!",
  "sender": "John Doe"
}
```

**Response:**
```json
{
  "success": true,
  "message": {
    "id": "1234567890",
    "text": "Hello, world!",
    "timestamp": "2025-10-21T08:30:00.000Z",
    "sender": "John Doe"
  }
}
```

### GET /api/health
Health check endpoint.

**Response:**
```json
{
  "status": "ok",
  "clients": 2
}
```

## Environment

- **Port:** 3001
- **CORS:** Enabled for all origins

## Development

The server uses `tsx watch` for hot reloading during development. Any changes to TypeScript files will automatically restart the server.

