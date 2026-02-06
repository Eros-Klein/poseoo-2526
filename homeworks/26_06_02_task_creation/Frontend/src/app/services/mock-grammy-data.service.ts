import { Injectable } from '@angular/core';
import { Stage, Category, Artist, Performance, PriorityLevel, ArtistStatistics } from '../models/grammy.models';

@Injectable({
  providedIn: 'root'
})
export class MockGrammyDataService {
  private stages: Stage[] = [];
  private categories: Category[] = [];
  private artists: Artist[] = [];
  private performances: Performance[] = [];

  constructor() {
    this.initializeMockData();
  }

  private initializeMockData(): void {
    // Create Artists
    const tylerTheCreator: Artist = {
      id: '1',
      name: 'Tyler the Creator',
      categories: [],
      winningCategories: []
    };

    const badBunny: Artist = {
      id: '2',
      name: 'Bad Bunny',
      categories: [],
      winningCategories: []
    };

    const clipse: Artist = {
      id: '3',
      name: 'Clipse',
      categories: [],
      winningCategories: []
    };

    const billieEilish: Artist = {
      id: '4',
      name: 'Billie Eilish',
      categories: [],
      winningCategories: []
    };

    const justinBieber: Artist = {
      id: '5',
      name: 'Justin Bieber',
      categories: [],
      winningCategories: []
    };

    const dojaCat: Artist = {
      id: '6',
      name: 'Doja Cat',
      categories: [],
      winningCategories: []
    };

    const kendrickLamar: Artist = {
      id: '7',
      name: 'Kendrick Lamar',
      categories: [],
      winningCategories: []
    };

    const jid: Artist = {
      id: '8',
      name: 'JID',
      categories: [],
      winningCategories: []
    };

    // Create Performances
    const perf1: Performance = {
      id: 'p1',
      artistId: '1',
      categoryId: 'c1',
      dateTime: '2026-02-01T17:30:00Z',
      budget: 150000
    };

    const perf2: Performance = {
      id: 'p2',
      artistId: '3',
      categoryId: 'c1',
      dateTime: '2026-02-01T18:00:00Z',
      budget: 70000
    };

    const perf3: Performance = {
      id: 'p3',
      artistId: '4',
      categoryId: 'c2',
      dateTime: '2026-02-01T18:30:00Z',
      budget: 90000
    };

    const perf4: Performance = {
      id: 'p4',
      artistId: '5',
      categoryId: 'c2',
      dateTime: '2026-02-01T20:30:00Z',
      budget: 10000
    };

    const perf5: Performance = {
      id: 'p5',
      artistId: '8',
      categoryId: 'c3',
      dateTime: '2026-02-01T19:00:00Z',
      budget: 60000
    };

    this.performances = [perf1, perf2, perf3, perf4, perf5];

    // Link performances to artists
    tylerTheCreator.performance = perf1;
    clipse.performance = perf2;
    billieEilish.performance = perf3;
    justinBieber.performance = perf4;
    jid.performance = perf5;

    // Create Categories
    const recordOfTheYear: Category = {
      id: 'c1',
      name: 'Record of the year',
      priority: PriorityLevel.AcrossGenres,
      budget: 100000,
      stageId: 's1',
      artists: [tylerTheCreator, badBunny, clipse, billieEilish],
      winner: tylerTheCreator
    };

    const vocalAlbumOfTheYear: Category = {
      id: 'c2',
      name: 'Vocal album of the year',
      priority: PriorityLevel.AcrossGenres,
      budget: 80000,
      stageId: 's1',
      artists: [billieEilish, justinBieber, dojaCat],
      winner: billieEilish
    };

    const rapAlbumOfTheYear: Category = {
      id: 'c3',
      name: 'Rap album of the year',
      priority: PriorityLevel.GenreSpecific,
      budget: 50000,
      stageId: 's1',
      artists: [kendrickLamar, jid, tylerTheCreator]
    };

    const bestNewArtist: Category = {
      id: 'c4',
      name: 'Best New Artist',
      priority: PriorityLevel.AcrossGenres,
      budget: 60000,
      stageId: 's2',
      artists: [dojaCat, jid]
    };

    this.categories = [recordOfTheYear, vocalAlbumOfTheYear, rapAlbumOfTheYear, bestNewArtist];

    // Link categories to artists
    tylerTheCreator.categories = [recordOfTheYear, rapAlbumOfTheYear];
    tylerTheCreator.winningCategories = [recordOfTheYear];
    
    badBunny.categories = [recordOfTheYear];
    
    clipse.categories = [recordOfTheYear];
    
    billieEilish.categories = [recordOfTheYear, vocalAlbumOfTheYear];
    billieEilish.winningCategories = [vocalAlbumOfTheYear];
    
    justinBieber.categories = [vocalAlbumOfTheYear];
    
    dojaCat.categories = [vocalAlbumOfTheYear, bestNewArtist];
    
    kendrickLamar.categories = [rapAlbumOfTheYear];
    
    jid.categories = [rapAlbumOfTheYear, bestNewArtist];

    this.artists = [tylerTheCreator, badBunny, clipse, billieEilish, justinBieber, dojaCat, kendrickLamar, jid];

    // Create Stages
    const aStage: Stage = {
      id: 's1',
      name: 'A-Stage',
      categories: [recordOfTheYear, vocalAlbumOfTheYear, rapAlbumOfTheYear]
    };

    const bStage: Stage = {
      id: 's2',
      name: 'B-Stage',
      categories: [bestNewArtist]
    };

    this.stages = [aStage, bStage];
  }

  getStages(): Stage[] {
    return this.stages;
  }

  getStageById(id: string): Stage | undefined {
    return this.stages.find(s => s.id === id);
  }

  getCategories(stageId?: string): Category[] {
    if (stageId) {
      return this.categories.filter(c => c.stageId === stageId);
    }
    return this.categories;
  }

  getCategoryById(id: string): Category | undefined {
    return this.categories.find(c => c.id === id);
  }

  getArtists(categoryId?: string): Artist[] {
    if (categoryId) {
      const category = this.categories.find(c => c.id === categoryId);
      return category?.artists || [];
    }
    return this.artists;
  }

  getPerformances(artistId?: string): Performance[] {
    if (artistId) {
      return this.performances.filter(p => p.artistId === artistId);
    }
    return this.performances;
  }

  getPerformanceById(id: string): Performance | undefined {
    return this.performances.find(p => p.id === id);
  }

  getStatistics(): ArtistStatistics[] {
    // Only calculate statistics if there are winners announced
    const hasWinners = this.categories.some(c => c.winner !== undefined);
    if (!hasWinners) {
      return [];
    }

    const performingArtists = this.artists.filter(a => a.performance !== undefined);
    const statistics: ArtistStatistics[] = [];

    for (const artist of performingArtists) {
      if (!artist.performance) continue;

      const performance = this.performances.find(p => p.id === artist.performance!.id);
      if (!performance) continue;

      const category = this.categories.find(c => c.id === performance.categoryId);
      if (!category) continue;

      // Calculate budget points
      const budgetDifference = performance.budget - category.budget;
      const budgetDifferencePercentage = budgetDifference / category.budget;
      
      let budgetPoints = 0;
      if (budgetDifferencePercentage > 0) {
        // Exceedance - positive points
        const steps = Math.floor(budgetDifferencePercentage / 0.25);
        budgetPoints = Math.min(steps, 10);
      } else if (budgetDifferencePercentage < 0) {
        // Undershoot - negative points
        const steps = Math.ceil(budgetDifferencePercentage / 0.25);
        budgetPoints = Math.max(steps, -10);
      }

      // Calculate winning categories points
      let winningCategoriesPoints = 0;
      for (const winningCategory of artist.winningCategories) {
        if (winningCategory.priority === PriorityLevel.AcrossGenres) {
          winningCategoriesPoints += 2;
        } else {
          winningCategoriesPoints += 1;
        }
      }

      // Calculate nominated categories points
      let nominatedCategoriesPoints = 0;
      for (const nominatedCategory of artist.categories) {
        if (nominatedCategory.priority === PriorityLevel.AcrossGenres) {
          nominatedCategoriesPoints += 0.25;
        } else {
          nominatedCategoriesPoints += 0.1;
        }
      }

      const totalFactors = budgetPoints + winningCategoriesPoints + nominatedCategoriesPoints;
      const performanceScore = totalFactors / performingArtists.length;

      statistics.push({
        artistId: artist.id,
        artistName: artist.name,
        performanceScore,
        budgetPoints,
        winningCategoriesPoints,
        nominatedCategoriesPoints
      });
    }

    // Sort by performance score descending
    return statistics.sort((a, b) => b.performanceScore - a.performanceScore);
  }

  setWinner(categoryId: string, artistId: string): void {
    const category = this.categories.find(c => c.id === categoryId);
    const artist = this.artists.find(a => a.id === artistId);
    
    if (category && artist) {
      category.winner = artist;
      if (!artist.winningCategories.includes(category)) {
        artist.winningCategories.push(category);
      }
    }
  }
}
