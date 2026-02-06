# Grammy Performance Planning - Frontend

This Angular 21 frontend application provides a beautiful, modern UI for managing and viewing Grammy Award performance schedules and statistics.

## Features

### 🎭 Stages View
- **Interactive stage selection**: Click on any stage to view detailed information
- **Category display**: View all categories for each stage with their priority levels
- **Artist nominations**: See all nominated artists per category
- **Performance details**: View scheduled performances with timestamps and budgets
- **Winner indicators**: Visual indicators for category winners (🏆)
- **Performance badges**: Clear visual distinction for performing artists (🎵)

### 📊 Statistics View
- **Performance scores**: Calculated scores for all performing artists
- **Ranking system**: Visual ranking with medals for top 3 performers (🥇🥈🥉)
- **Detailed breakdown**: See budget points, winning category points, and nomination points
- **Summary cards**: Quick overview of key statistics
- **Score visualization**: Color-coded scores (green for high, yellow for medium, red for low)
- **Calculation explanation**: Built-in documentation of how scores are calculated

## Technology Stack

- **Angular 21**: Latest Angular with standalone components
- **Signals**: Reactive state management using Angular signals
- **TypeScript**: Full type safety
- **CSS3**: Modern, responsive styling with grid and flexbox
- **Mock Data**: Sample data service for development without backend

## Project Structure

```
Frontend/src/app/
├── models/
│   └── grammy.models.ts          # TypeScript interfaces for all data types
├── services/
│   └── mock-grammy-data.service.ts # Mock data service with sample Grammy data
├── stages-list/
│   ├── stages-list.ts            # Stages component logic
│   ├── stages-list.html          # Stages template
│   └── stages-list.css           # Stages styles
├── statistics/
│   ├── statistics.ts             # Statistics component logic
│   ├── statistics.html           # Statistics template
│   └── statistics.css            # Statistics styles
├── app.ts                        # Root component
├── app.html                      # Root template with navigation
├── app.css                       # Root component styles
└── app.routes.ts                 # Application routing
```

## Data Models

### Stage
```typescript
interface Stage {
  id: string;
  name: string;
  categories: Category[];
}
```

### Category
```typescript
interface Category {
  id: string;
  name: string;
  priority: PriorityLevel;
  budget: number;
  stageId: string;
  artists: Artist[];
  winner?: Artist;
}
```

### Artist
```typescript
interface Artist {
  id: string;
  name: string;
  performance?: Performance;
  categories: Category[];
  winningCategories: Category[];
}
```

### Performance
```typescript
interface Performance {
  id: string;
  artistId: string;
  categoryId: string;
  dateTime: string;
  budget: number;
}
```

### Statistics
```typescript
interface ArtistStatistics {
  artistId: string;
  artistName: string;
  performanceScore: number;
  budgetPoints: number;
  winningCategoriesPoints: number;
  nominatedCategoriesPoints: number;
}
```

## Mock Data

The application includes comprehensive mock data featuring:

### Stages
- **A-Stage**: 3 categories (Record of the year, Vocal album of the year, Rap album of the year)
- **B-Stage**: 1 category (Best New Artist)

### Artists
- Tyler the Creator
- Bad Bunny
- Clipse
- Billie Eilish
- Justin Bieber
- Doja Cat
- Kendrick Lamar
- JID

### Performances
5 scheduled performances with realistic budgets and timestamps

## Performance Score Calculation

The statistics view calculates performance scores using the following formula:

### Budget Points
- **Exceedances**: +1 point per 25% over budget (max +10)
- **Undershoots**: -1 point per 25% under budget (max -10)

### Winning Categories Points
- **Across Genres**: 2 points per win
- **Genre Specific**: 1 point per win

### Nominated Categories Points
- **Across Genres**: 0.25 points per nomination
- **Genre Specific**: 0.1 points per nomination

### Final Score
```
performanceScore = (budgetPoints + winningPoints + nominationPoints) / totalPerformingArtists
```

## Running the Application

```bash
# Install dependencies
npm install

# Run development server
npm start

# Build for production
npm run build
```

Navigate to `http://localhost:4200/` to view the application.

## Navigation

- **Stages** (`/stages`): View and explore all stages and their performances
- **Statistics** (`/statistics`): View performance scores and rankings

## Key Features Implemented

✅ Standalone Angular 21 components  
✅ Signal-based reactive state management  
✅ Responsive, modern UI design  
✅ Mock data service (no backend required)  
✅ Performance score calculations  
✅ Interactive stage/category exploration  
✅ Visual indicators for winners and performances  
✅ Comprehensive statistics display  
✅ Mobile-friendly responsive design  
✅ Clean, maintainable code structure  

## Future Integration

When the backend API is ready, simply:

1. Replace `MockGrammyDataService` with a real API service
2. Update the component `ngOnInit` methods to call the real API
3. The models are already structured to match the backend data format

Example conversion:
```typescript
// Current (mock)
this.stages.set(this.mockDataService.getStages());

// Future (real API)
this.stages.set(await this.api.invoke(getStages, {}));
```

## Design Philosophy

- **User-Centric**: Intuitive navigation and clear information hierarchy
- **Visual Feedback**: Icons, badges, and color coding for quick understanding
- **Performance**: Efficient rendering with Angular signals
- **Accessibility**: Semantic HTML and proper ARIA labels
- **Responsive**: Works seamlessly on desktop, tablet, and mobile devices

## Notes

- Statistics are only displayed when category winners are announced
- Artists can only perform once per event
- Performances must be at least 30 minutes apart on the same stage
- Priority constraints ensure artists perform at their highest-priority category
