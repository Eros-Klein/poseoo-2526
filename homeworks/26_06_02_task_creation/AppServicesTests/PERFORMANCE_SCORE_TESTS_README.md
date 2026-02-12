# Performance Score Calculation Tests

## Overview

This document describes the test suite for the Grammy performance score calculation logic defined in `PerformanceScoreCalculationTests.cs`. These tests are designed to validate the calculation requirements **without implementing the actual calculation logic**.

## Test Coverage

### Budget Factor Tests (6 tests)

1. **CalculatePerformanceScore_ArtistExceedsBudgetBy25Percent_AddsOnePoint**
   - Verifies that exceeding budget by 25% adds +1 point
   - Uses budget: $100,000, used: $125,000

2. **CalculatePerformanceScore_ArtistExceedsBudgetBy100Percent_AddsFourPoints**
   - Verifies that exceeding budget by 100% adds +4 points (4 × 25% increments)
   - Uses budget: $50,000, used: $100,000

3. **CalculatePerformanceScore_ArtistExceedsBudgetBy300Percent_CapsAtTenPoints**
   - Verifies that budget exceedance is capped at +10 points
   - Uses budget: $100,000, used: $400,000

4. **CalculatePerformanceScore_ArtistUndershootsBudgetBy25Percent_SubtractsOnePoint**
   - Verifies that undershooting budget by 25% subtracts -1 point
   - Uses budget: $100,000, used: $75,000

5. **CalculatePerformanceScore_ArtistUndershootsBudgetBy300Percent_CapsAtNegativeTenPoints**
   - Verifies that budget undershoot is capped at -10 points
   - Uses budget: $100,000, used: $1,000

### Winning Categories Tests (2 tests)

6. **CalculatePerformanceScore_ArtistWinsAcrossGenresCategory_AddsTwoPoints**
   - Verifies that winning an AcrossGenres category adds +2 points
   - Category: "Record of the Year" (AcrossGenres)

7. **CalculatePerformanceScore_ArtistWinsGenreSpecificCategory_AddsOnePoint**
   - Verifies that winning a GenreSpecific category adds +1 point
   - Category: "Best Rock Album" (GenreSpecific)

8. **CalculatePerformanceScore_ArtistWinsMultipleCategories_AddsAllWinningPoints**
   - Verifies that multiple wins accumulate correctly
   - Tests: 1 AcrossGenres (2 pts) + 2 GenreSpecific (1 pt each) = 4 points

### Nominated Categories Tests (2 tests)

9. **CalculatePerformanceScore_ArtistNominatedInAcrossGenresCategory_AddsQuarterPoint**
   - Verifies that an AcrossGenres nomination adds +0.25 points
   - Category: "Song of the Year" (AcrossGenres)

10. **CalculatePerformanceScore_ArtistNominatedInGenreSpecificCategory_AddsTenthPoint**
    - Verifies that a GenreSpecific nomination adds +0.1 points
    - Category: "Best Jazz Vocal Album" (GenreSpecific)

11. **CalculatePerformanceScore_ArtistNominatedInMultipleCategories_AddsAllNominationPoints**
    - Verifies that multiple nominations accumulate correctly
    - Tests: 2 AcrossGenres (0.25 each) + 1 GenreSpecific (0.1) = 0.6 points

### Complex Scenario Tests (4 tests)

12. **CalculatePerformanceScore_ComplexScenarioWithAllFactors_CalculatesCorrectScore**
    - Comprehensive test combining all three factors:
      - Budget: +1 point (25% exceedance)
      - Winning: +2 points (1 AcrossGenres win)
      - Nominations: +0.6 points (2 AcrossGenres + 1 GenreSpecific)
    - Expected total: 3.6 points (for single artist)

13. **CalculatePerformanceScore_NonPerformingArtist_IsNotIncludedInCalculation**
    - Verifies that artists without performances are excluded from statistics
    - Tests that only performing artists appear in results

14. **CalculatePerformanceScore_MultiplePerformingArtists_NormalizesScoresByTotalFactors**
    - Verifies normalization across multiple performing artists
    - Tests the division by total factors across all performing artists
    - Artist 1: 1.1 factors, Artist 2: 1.25 factors
    - Total: 2.35 factors for normalization

15. **CalculatePerformanceScore_NoWinnersAnnounced_ReturnsEmptyStatistics**
    - Verifies that statistics endpoint returns empty results when no winners are announced
    - Critical for the `/statistics` endpoint requirement

## Calculation Formula (from README)

The performance score = (sum of factors) / (total factors across all performing artists)

### Factors:
1. **Budget Factor** (per artist):
   - Exceedance: +1 per 25% over budget (capped at +10)
   - Undershoot: -1 per 25% under budget (capped at -10)

2. **Winning Categories Factor** (per artist):
   - AcrossGenres win: +2 points
   - GenreSpecific win: +1 point

3. **Nominated Categories Factor** (per artist):
   - AcrossGenres nomination: +0.25 points
   - GenreSpecific nomination: +0.1 points

## Implementation Notes

All tests contain TODO comments indicating where the actual calculation logic should be called. The tests follow the Arrange-Act-Assert pattern and use xUnit as the testing framework.

To implement the calculation logic:

1. Create an `IPerformanceScoreCalculator` interface in the AppServices project
2. Implement a `PerformanceScoreCalculator` class
3. Uncomment and complete the Act and Assert sections in each test
4. Run `dotnet test` to verify all tests pass

## Running the Tests

```bash
cd AppServicesTests
dotnet test
```

Note: Tests will initially be skipped or incomplete until the actual calculation logic is implemented.
