# 🚀 Quick Start Guide

## Getting Started

### 1. Start the Development Server

```bash
cd Frontend
npm start
```

The application will open at `http://localhost:4200/`

### 2. Navigate the Application

#### Stages View (Default)
- View the **A-Stage** and **B-Stage** in the left sidebar
- Click on any stage to see its details
- Explore categories, nominations, and scheduled performances
- Look for:
  - 🏆 Winner indicators
  - 🎵 Performance badges
  - Budget and timing information

#### Statistics View
- Click **Statistics** in the navigation bar
- See performance rankings for all performing artists
- Check out the detailed score breakdown
- Review the scoring methodology explanation

### 3. Sample Data Overview

The application includes realistic Grammy data:
- **2 stages** with 4 total categories
- **8 artists** (Tyler the Creator, Billie Eilish, etc.)
- **5 scheduled performances**
- **2 winners announced** (Tyler and Billie)

### 4. Key Features to Explore

✅ **Interactive Stage Selection**
   - Click different stages to see their schedules

✅ **Category Priority Badges**
   - Yellow badges = Across Genres (higher priority)
   - Gray badges = Genre Specific

✅ **Performance Indicators**
   - Artists with performances have a 🎵 badge
   - Non-performing nominees are listed without badges

✅ **Performance Scores**
   - Color-coded scores (green = high, yellow = medium, red = low)
   - Medal rankings for top 3 performers

✅ **Responsive Design**
   - Try resizing your browser window
   - Works great on mobile too!

## What's Included

- ✅ Full data models matching backend structure
- ✅ Mock data service (no backend needed)
- ✅ Stages component with interactive exploration
- ✅ Statistics component with performance scoring
- ✅ Modern, responsive UI design
- ✅ Navigation and routing
- ✅ Builds successfully with no errors

## Understanding the Scoring

Performance scores are calculated using:

1. **Budget Points** (-10 to +10)
   - Going over budget = positive points
   - Going under budget = negative points

2. **Winning Categories Points**
   - Across Genres win = 2 points
   - Genre Specific win = 1 point

3. **Nominated Categories Points**
   - Across Genres nomination = 0.25 points
   - Genre Specific nomination = 0.1 points

The final score is the sum divided by the number of performing artists.

## Next Steps

When you're ready to connect to the real backend:
1. Generate API client with `npm run generate-web-api`
2. Replace `MockGrammyDataService` with real API calls
3. Models are already backend-compatible!

## Need Help?

- Check `GRAMMY_FRONTEND.md` for detailed documentation
- Review `FRONTEND_IMPLEMENTATION_SUMMARY.md` for implementation details
- All code is well-commented and follows Angular best practices

Enjoy exploring the Grammy Performance Planning app! 🎵
