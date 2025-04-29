# Refactor Plan: Shared Library for DTOs, Domain, and IRepository

## Step 1: Create Shared Class Library
- [x] Create a new .NET class library project: MarketMinds.Shared
- [ ] Add it to the solution and reference it from both backend and frontend projects

## Step 2: Move Shared Code
- [ ] Move all DTOs, domain models, and IRepository interfaces from backend (and frontend, if any) to MarketMinds.Shared
- [ ] Update namespaces and project references accordingly

## Step 3: Update Backend and Frontend
- [ ] Update backend and frontend to use types from MarketMinds.Shared
- [ ] Remove duplicate code from backend/frontend

## Step 4: Test Build
- [ ] Ensure both backend and frontend build and run correctly

## Step 5: Further Refactoring (Controllers, Repos, Services)
- [ ] Restrict backend controllers to use only IRepository
- [ ] Introduce repository layer in frontend, move HTTP logic from services to repositories
- [ ] Update services to use repositories

## Step 6: Final Testing
- [ ] Ensure all functionality is preserved and code is DRY 