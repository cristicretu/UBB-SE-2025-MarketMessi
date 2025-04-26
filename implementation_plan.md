# Implementation Plan for User Authentication System

## Overview
This plan outlines the implementation of a robust user authentication system (login/registration) using MVVM architecture, Entity Framework, and RESTful API communication.

## Current State Analysis
After reviewing the codebase, we've identified:

1. **User Entity**: Exists in `DomainLayer.Domain`
2. **UserRepository**: Exists but uses an in-memory collection instead of Entity Framework
3. **LoginService/RegisterService**: Exist but don't communicate with a backend API
4. **ViewModels**: Exist but need updates for proper API integration
5. **Backend API**: Needs a UserController for authentication operations

## Architecture Updates

### 1. Data Layer
- [x] Update User entity with necessary properties for authentication
- [x] Create IUserRepository interface for database operations
- [x] Implement UserRepository using HttpClient for API communication
- [x] Create UserDto for API data transfer

### 2. Service Layer
- [x] Create IAuthService interface defining authentication operations
- [x] Implement AuthService to interact with the backend API
  - [x] Configure HttpClient with base URL from app configuration
  - [x] Implement endpoints for registration, login, etc.
- [x] Update existing LoginService and RegisterService to use the new repository
- [x] Update ResetPasswordService to use the new repository

### 3. ViewModel Layer
- [x] Update LoginViewModel to use the new AuthService
- [x] Update RegisterViewModel to use the new AuthService

### 4. Backend API Layer
- [x] Create UsersController in the server project
- [x] Implement registration and login endpoints
- [x] User entity mapping is already in ApplicationDbContext

## Implementation Steps

### Phase 1: Backend API
1. [x] Create UsersController with:
   - [x] Registration endpoint
   - [x] Login endpoint
   - [x] Username availability check endpoint
2. [x] Updated UsersController to properly handle PasswordHash

### Phase 2: Client Data and Service Layer
1. [x] Create IUserRepository interface
2. [x] Implement UserRepository with API communication
3. [x] Create and implement IAuthService
4. [x] Create UserDto for API data transfer
5. [x] Update legacy services (LoginService, RegisterService, ResetPasswordService) to use new repositories

### Phase 3: ViewModel Updates
1. [x] Update LoginViewModel to use AuthService
2. [x] Update RegisterViewModel to use AuthService
3. [x] Update LoginPage to use async pattern
4. [x] Update RegisterPage to use async pattern

### Phase 4: Integration and Testing
1. [ ] Test registration flow
2. [ ] Test login flow
3. [ ] Test password reset flow
4. [ ] Fix any issues

## Completed Implementation

We have successfully:
1. Created a UsersController for the backend API with endpoints for registration, login, and username availability checking
2. Implemented proper password hashing for security
3. Created an IUserRepository interface and implementation using HttpClient to communicate with the API
4. Created an IAuthService interface and implementation to provide business logic for authentication
5. Updated the LoginViewModel and RegisterViewModel to use the new AuthService
6. Updated the LoginPage and RegisterPage to properly handle async operations with error handling
7. Created a UserDto to handle data transfer between client and server
8. Updated the server-side controller to use proper password hashing with the existing User model
9. Updated legacy services (LoginService, RegisterService, ResetPasswordService) to work with the new async repository pattern
10. Added backward compatibility methods where needed for a smooth transition

## Remaining Tasks
1. Test the complete authentication flow to ensure it works end-to-end
2. Monitor password reset functionality

## Security Considerations
1. Passwords are hashed using SHA-256 in the backend before storage
2. Input validation is performed both on client and server sides
3. API responses don't include sensitive information like passwords

## API Endpoints Design
- [x] POST `/api/users/register` - Register a new user
- [x] POST `/api/users/login` - Authenticate user
- [x] GET `/api/users/check-username/{username}` - Check if username is available

## Next Steps
1. Update the LoginPage.xaml.cs to properly handle the async pattern
2. Ensure the ApplicationDbContext includes User entity mapping
3. Test the complete authentication flow 