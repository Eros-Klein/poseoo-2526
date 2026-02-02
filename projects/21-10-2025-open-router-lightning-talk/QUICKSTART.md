# Quick Start Guide

Get your chat application running in 3 simple steps!

## 🚀 Installation & Running

### Step 1: Install Root Dependencies
```bash
npm install
```

### Step 2: Install App Dependencies
```bash
# Backend
cd apps/backend
npm install
cd ../..

# Frontend  
cd apps/frontend
npm install
cd ../..
```

### Step 3: Run the Apps
```bash
# From root directory - runs both apps simultaneously
npm run dev
```

That's it! 🎉

## 📱 Access the Application

- **Frontend:** http://localhost:4200
- **Backend API:** http://localhost:3001
- **SSE Endpoint:** http://localhost:3001/api/sse

## 🧪 Test It Out

1. Open http://localhost:4200 in your browser
2. Enter your name
3. Type a message and hit Enter or click Send
4. Open another browser window/tab to see real-time sync
5. Watch messages appear instantly across all windows!

## 🛠️ Troubleshooting

**Port already in use?**
```bash
# Kill process on port 3001 (backend)
lsof -ti:3001 | xargs kill -9

# Kill process on port 4200 (frontend)
lsof -ti:4200 | xargs kill -9
```

**Connection issues?**
- Make sure both backend and frontend are running
- Check the connection indicator (should be green)
- Look for errors in browser console (F12)

## 📦 What's Included

- ✅ Turborepo monorepo setup
- ✅ Express backend with SSE
- ✅ Angular 20 frontend
- ✅ Tailwind CSS 4 styling
- ✅ TypeScript everywhere
- ✅ Real-time chat functionality
- ✅ Beautiful modern UI

## 📚 Next Steps

- Read the main [README.md](./README.md) for detailed documentation
- Check [apps/backend/README.md](./apps/backend/README.md) for API details
- Check [apps/frontend/README.md](./apps/frontend/README.md) for frontend architecture

Happy coding! 💬

