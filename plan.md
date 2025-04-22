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