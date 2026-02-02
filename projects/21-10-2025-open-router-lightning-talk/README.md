# Live Chat Monorepo

A Turborepo monorepo featuring a real-time chat application with an Angular frontend and Express backend, synchronized via Server-Sent Events (SSE).

## Project Structure

```
.
├── apps/
│   ├── backend/          # Express server with SSE
│   └── frontend/         # Angular app with Tailwind CSS
├── package.json          # Root workspace configuration
├── turbo.json           # Turborepo pipeline configuration
└── README.md
```

## Tech Stack

### Backend
- **Node.js** with **Express**
- **TypeScript** for type safety
- **Server-Sent Events (SSE)** for real-time messaging
- **CORS** enabled for cross-origin requests

### Frontend
- **Angular 20** (latest version)
- **Tailwind CSS 4** for styling
- **TypeScript**
- Modern responsive chat UI

## Features

- ✨ Real-time message synchronization via SSE
- 💬 Beautiful chat interface with Tailwind CSS
- 🔄 Live connection status indicator
- 👥 Multiple client support
- 🎨 Modern gradient UI design
- ⚡ Fast development with Turborepo caching

## Getting Started

### Prerequisites

- Node.js >= 18.0.0
- npm

### Installation

1. Install dependencies from the root:

```bash
npm install
```

This will install dependencies for both the root workspace and all apps.

2. Install backend dependencies:

```bash
cd apps/backend
npm install
cd ../..
```

3. Install frontend dependencies:

```bash
cd apps/frontend
npm install
cd ../..
```

### Running the Application

#### Option 1: Run both apps simultaneously (recommended)

From the root directory:

```bash
npm run dev
```

This will start:
- Backend server on `http://localhost:3001`
- Frontend app on `http://localhost:4200`

#### Option 2: Run apps individually

**Backend:**
```bash
cd apps/backend
npm run dev
```

**Frontend:**
```bash
cd apps/frontend
npm run dev
```

### Usage

1. Open your browser to `http://localhost:4200`
2. Enter your name in the name field
3. Type a message and click "Send" or press Enter
4. Open multiple browser windows to see real-time synchronization
5. Watch the connection indicator (green = connected, red = disconnected)

## API Endpoints

### Backend (Port 3001)

- `GET /api/sse` - Server-Sent Events endpoint for receiving live updates
- `POST /api/messages` - Send a new message
  - Body: `{ "text": "message", "sender": "name" }`
- `GET /api/health` - Health check endpoint

## Development

### Build for Production

```bash
# Build all apps
npm run build

# Build specific app
cd apps/backend && npm run build
cd apps/frontend && npm run build
```

### Turbo Commands

```bash
# Run dev mode for all apps
npm run dev

# Build all apps
npm run build

# Lint all apps
npm run lint
```

## Architecture

### Server-Sent Events (SSE)

The backend maintains a set of active SSE connections. When a client sends a message via POST to `/api/messages`, the server broadcasts it to all connected clients through their SSE connections.

### Message Flow

1. Client connects to `/api/sse` and establishes an SSE connection
2. Client sends message via POST to `/api/messages`
3. Server receives message and broadcasts to all connected SSE clients
4. All clients receive the message in real-time
5. Frontend updates the UI automatically

## Troubleshooting

### Backend not connecting
- Ensure port 3001 is not in use
- Check that the backend server is running

### Frontend CORS errors
- Verify CORS is enabled in the backend
- Check that the API_URL in frontend matches the backend URL

### Messages not appearing
- Check browser console for errors
- Verify SSE connection is established (check connection indicator)
- Ensure both frontend and backend are running

## License

MIT

