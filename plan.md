# Entity Framework Setup Plan

## 1. ✅ Install Required Packages
- ✅ EF Core packages already installed in server.csproj:
  - Microsoft.EntityFrameworkCore.SqlServer
  - Microsoft.EntityFrameworkCore.Design
  - Microsoft.EntityFrameworkCore.Tools

## 2. ✅ Create Entity Models
- ✅ Created/updated entity models based on SQL schema:
  - ✅ User model
  - ✅ Product metadata models (Condition, Category, Tag)
  - ✅ AuctionProduct and related models
  - ✅ BuyProduct and related models
  - ✅ BorrowProduct and related models
  - ✅ Review and related models
- ✅ Fixed models:
  - ✅ Fixed Product base class by removing Tags and Images properties
  - ✅ Created AuctionProductImage and AuctionProductProductTag models
  - ✅ Updated AuctionProduct model to use the proper navigation properties
  - ✅ Added backward compatibility with legacy repository implementation
    - ✅ Added Title property that maps to DisplayTitle in ProductCondition, ProductCategory, and ProductTag
    - ✅ Added Id property to Bid model
    - ✅ Added legacy Tags and Images properties to AuctionProduct
    - ✅ Added legacy constructor to AuctionProduct

## 3. ✅ Create DbContext
- ✅ Created ApplicationDbContext
- ✅ Configured entity relationships and constraints
- ✅ Fixed DbContext by adding missing entity references

## 4. ✅ Configure Connection String
- ✅ Connection string already exists in appsettings.json
- ✅ Configured DbContext in Program.cs

## 5. Enable Migrations
- Initialize migrations
- Create initial migration

## 6. ✅ Gradual Migration to EF Core
- ✅ Modified AuctionProductsRepository to use EF Core for the GetProducts method
- ✅ Updated dependency injection in Program.cs to inject DbContext
- Continue migrating remaining repository methods to EF Core as needed:
  - GetProductById
  - AddProduct
  - UpdateProduct
  - DeleteProduct

## 7. Additional Steps
- Migrate other repositories to EF Core
- Implement unit of work pattern (if needed)
- Configure eager loading and navigation properties 

# Borrow Products Refactoring Plan

## Overview
We need to refactor the borrow products flow to move the repository from the frontend to the server. The frontend service will use API calls to the backend instead of direct database access. We'll also implement Entity Framework on the backend for data access.

## Reference
The auction flow can be used as a reference as it has already been refactored:
- AuctionProductsRepository.cs
- AuctionProductsService.cs
- AuctionProductsController.cs

## Steps

### 1. Server-side Implementation

#### 1.1 Create/Update Entity Models
- [x] Ensure server-side BorrowProduct model is compatible with Entity Framework
- [x] Update/Create any related models (e.g., BorrowProductImage)

#### 1.2 Implement Server-side Repository
- [x] Create server/MarketMinds/Repositories/BorrowProductsRepository/IBorrowProductsRepository.cs
- [x] Create server/MarketMinds/Repositories/BorrowProductsRepository/BorrowProductsRepository.cs
- [x] Implement Entity Framework for data access

#### 1.3 Create Controller
- [x] Create server/Controllers/BorrowProductsController.cs
- [x] Implement API endpoints for all required operations:
  - [x] GET /api/borrowproducts - Get all borrow products
  - [x] GET /api/borrowproducts/{id} - Get a specific borrow product
  - [x] POST /api/borrowproducts - Create a new borrow product
  - [x] PUT /api/borrowproducts/{id} - Update an existing borrow product
  - [x] DELETE /api/borrowproducts/{id} - Delete a borrow product

#### 1.4 Update Program.cs
- [x] Register IBorrowProductsRepository and its implementation
- [x] Ensure proper dependency injection

### 2. Client-side Implementation

#### 2.1 Update BorrowProductsService
- [x] Refactor service to use HTTP client for API calls
- [x] Implement all required API methods
- [x] Ensure proper error handling

#### 2.2 Update IBorrowProductsService
- [x] Update interface to reflect any changes in API methods

### 3. Fixed Database Schema Mapping Issues
- [x] Updated ApplicationDbContext to properly map C# properties to database columns:
  - [x] Added column name mappings for BorrowProduct properties (daily_rate, time_limit, start_date, end_date, is_borrowed)
  - [x] Added column name mappings for BorrowProductImage (product_id)
  - [x] Added column name mappings for BorrowProductProductTag (product_id, tag_id)
  - [x] Configured proper entity relationships

- [x] Made repository methods more robust:
  - [x] Updated GetProducts to handle potential database schema issues
  - [x] Updated GetProductByID to handle potential database schema issues
  - [x] Made controller more resilient to empty result sets

### 4. Fixed Service Initialization Issues
- [x] Enhanced BorrowProductsService to add null checking and better error handling:
  - [x] Added validation for configuration in the constructor
  - [x] Added null checks for httpClient before use
  - [x] Added more detailed exception handling
  - [x] Ensured the repository constructor also initializes httpClient with default values

- [x] Fixed App.xaml.cs to properly initialize BorrowProductsService:
  - [x] Updated the service initialization to use Configuration instead of BorrowProductsRepository
  - [x] This ensures the HTTP client is properly initialized for API calls

### 5. Testing
- [ ] Test GET endpoints
- [ ] Test POST endpoint
- [ ] Test PUT endpoint
- [ ] Test DELETE endpoint

### 6. Cleanup
- [ ] Remove old repository implementation (after confirming new implementation works)
- [ ] Update any references to old implementation

## Next Steps

1. Verify that the service initialization fixes resolved the API call issues
2. Test all API endpoints to confirm functionality
3. Address any additional issues that may arise during testing
4. Once stable, remove the old repository implementation 
5. Consider creating database migrations if needed to ensure schema compatibility 