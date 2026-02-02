# Frontend - Angular Chat Application

Modern chat interface built with Angular 20 and Tailwind CSS 4, featuring real-time message synchronization via Server-Sent Events.

## Features

- Real-time message updates using SSE
- Beautiful, responsive UI with Tailwind CSS
- Live connection status indicator
- Smooth animations and transitions
- Modern gradient design
- Customizable sender name
- Auto-scroll to latest messages

## Scripts

```bash
# Development server (port 4200)
npm run dev

# Build for production
npm run build

# Run tests
npm run test
```

## Tech Stack

- **Angular 20** - Latest version with standalone components
- **Tailwind CSS 4** - Utility-first CSS framework
- **TypeScript** - Type-safe development
- **RxJS** - Reactive programming
- **Server-Sent Events** - Real-time message streaming

## Architecture

### Component Structure

The main app component (`app.ts`) handles:
- SSE connection management
- Message state using Angular signals
- HTTP requests to backend API
- Real-time UI updates

### Styling

Tailwind CSS is configured to:
- Scan all `.html` and `.ts` files
- Provide utility classes for rapid UI development
- Include custom animations for message transitions

## API Integration

The frontend connects to the backend at `http://localhost:3001`:
- **SSE Connection:** `GET /api/sse`
- **Send Message:** `POST /api/messages`

## Development

The app runs on port 4200 by default. Live reload is enabled, so any changes to the source files will automatically refresh the browser.

## Building for Production

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.
