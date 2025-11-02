# FStreak Architecture Summary

## Project Structure
- **FStreak.API**: Web API layer with controllers, DTOs, and SignalR hubs
- **FStreak.Application**: Application services, DTOs, and business logic
- **FStreak.Domain**: Domain entities, interfaces, and business rules
- **FStreak.Infrastructure**: Data access layer with EF Core, repositories, and DbContext

## Key Components

### Authentication & Authorization
- Uses ASP.NET Core Identity with custom `ApplicationUser` and `ApplicationRole`
- JWT Bearer authentication configured
- Current roles: "Admin", "Moderator", "User" (seeded in Program.cs)
- Need to add "Teacher" role for lesson management

### Database Context
- `FStreakDbContext` inherits from `IdentityDbContext<ApplicationUser, ApplicationRole, string>`
- Uses MySQL with Entity Framework Core
- Repository pattern with Unit of Work implementation

### Current Entities
- ApplicationUser, ApplicationRole
- Study-related: StudyGroup, StudySession, StudyRoom, StudyWallPost
- User management: UserFriend, UserBadge, UserChallenge
- Communication: RoomMessage, SessionMessage, Reaction
- System: StreakLog, Reminder, PushSubscription, RefreshToken

### Service Layer
- Repository pattern with interfaces in Domain layer
- Service implementations in Application layer
- AutoMapper for DTO mapping
- SignalR for real-time communication

### API Layer
- Controllers for different domains (Auth, StudyRoom, Streak, etc.)
- DTOs for request/response models
- Swagger documentation configured

## Implementation Plan for Lesson CRUD
1. Add "Teacher" role to role seeding
2. Create Lesson entity in Domain layer
3. Add Lesson to DbContext
4. Create Lesson repository interface and implementation
5. Create Lesson service interface and implementation
6. Create Lesson DTOs
7. Create LessonsController with CRUD endpoints
8. Add authorization policies for Teacher role
9. Create and apply EF Core migration
10. Test the implementation
