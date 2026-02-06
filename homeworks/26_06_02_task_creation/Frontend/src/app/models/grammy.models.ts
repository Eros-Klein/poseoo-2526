export enum PriorityLevel {
  GenreSpecific = 1,
  AcrossGenres = 2
}

export interface Stage {
  id: string;
  name: string;
  categories: Category[];
}

export interface Category {
  id: string;
  name: string;
  priority: PriorityLevel;
  budget: number;
  stageId: string;
  artists: Artist[];
  winner?: Artist;
}

export interface Artist {
  id: string;
  name: string;
  performance?: Performance;
  categories: Category[];
  winningCategories: Category[];
}

export interface Performance {
  id: string;
  artistId: string;
  categoryId: string;
  dateTime: string;
  budget: number;
}

export interface ArtistStatistics {
  artistId: string;
  artistName: string;
  performanceScore: number;
  budgetPoints: number;
  winningCategoriesPoints: number;
  nominatedCategoriesPoints: number;
}
