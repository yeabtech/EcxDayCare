# Phase 7: Parents & Classrooms Implementation

## Overview
This phase implements full-stack Parents and Classrooms features following the established architecture patterns from the Children module (Phase 6).

## Architecture

### Three-Layer Architecture Pattern

#### 1. Domain Layer (`Daycare.Domain/`)
Models core business logic with:
- **Entities**: `Parent`, `Classroom`
- **Value Objects**: `ContactInfo`, `AgeGroup`, `ClassroomCapacity`, `ParentStatus`, `ClassroomStatus`
- **Relationships**: `ParentChild` join entity for many-to-many, Classroom owns Children collection
- **Repository Contracts**: `IParentRepository`, `IClassroomRepository`

#### 2. Application Layer (`Daycare.Application/`)
Implements CQRS pattern with:
- **Commands**: `CreateParentCommand`, `UpdateParentCommand`, `DeleteParentCommand`, `LinkChildToParentCommand`, `UnlinkChildFromParentCommand`, `CreateClassroomCommand`, `UpdateClassroomCommand`, `DeleteClassroomCommand`, `EnrollChildCommand`, `RemoveChildCommand`
- **Queries**: `GetParentQuery`, `GetAllParentsQuery`, `SearchParentsQuery`, `GetParentsByStatusQuery`, `GetParentsByChildQuery`, `GetClassroomQuery`, `GetAllClassroomsQuery`, `SearchClassroomsQuery`
- **Handlers**: Command/Query handlers implementing business logic via repositories
- **DTOs**: `ParentDto`, `ClassroomDto` for API contracts

#### 3. Infrastructure Layer (`Daycare.Infrastructure/`)
Provides persistence with:
- **Repositories**: `ParentRepository`, `ClassroomRepository` with EF Core
- **EF Configurations**: `ParentConfiguration` (maps Parent, ParentChild, owned types), `ClassroomConfiguration` (pending)
- **DbContext**: Extensions to `DaycareDbContext` with DbSets and model configuration

#### 4. API Layer (`ECX_Daycare/`)
REST endpoints:
- **ParentsController**: CRUD endpoints for parents, child linking/unlinking
- **ClassroomsController**: CRUD endpoints for classrooms, child enrollment/removal

#### 5. React Frontend (`ECX_Daycare.Frontend/`)
Feature modules following Children pattern:
- **Parents Module** (`src/features/parents/`)
  - Types, API service, custom hook, form component, CRUD pages
  - Routes: `/parents`, `/parents/create`, `/parents/:id`, `/parents/:id/edit`
  - Sidebar integration

- **Classrooms Module** (`src/features/classrooms/`)
  - Types, API service, custom hook, form component, CRUD pages
  - Routes: `/classrooms`, `/classrooms/create`, `/classrooms/:id`, `/classrooms/:id/edit`
  - Sidebar integration with capacity visualization

## Domain Models

### Parent Aggregate
```
Parent
├── ParentId (TypedId)
├── PersonName (FirstName, LastName)
├── ContactInfo (Email, PhoneNumber)
├── Address (ValueObject)
├── ParentStatus (Active | Inactive)
├── Notes (optional)
├── ParentChildren (ICollection)
└── Timestamps
```

### ParentChild Join Entity
- ParentId, ChildId, CreatedAt
- Represents parent-child relationships

### Classroom Aggregate
```
Classroom
├── ClassroomId (TypedId)
├── Name
├── AgeGroup (MinAgeMonths, MaxAgeMonths)
├── ClassroomCapacity (MaxCapacity)
├── ClassroomStatus (Active | Inactive)
├── Description (optional)
├── LeadTeacherId (optional, nullable Guid)
├── Children (ICollection)
├── Timestamps
└── Methods: AddChild, RemoveChild, GetCurrentEnrollment, GetAvailableSpots, IsAtCapacity
```

## Files Created

### Backend
- `Daycare.Domain/Entities/Parent.cs`
- `Daycare.Domain/Entities/ParentStatus.cs`
- `Daycare.Domain/Entities/Classroom.cs`
- `Daycare.Domain/Entities/ClassroomStatus.cs`
- `Daycare.Domain/ValueObjects/ContactInfo.cs`
- `Daycare.Domain/ValueObjects/AgeGroup.cs` (includes ClassroomCapacity)
- `Daycare.Domain/Repositories/IParentRepository.cs`
- `Daycare.Domain/Repositories/IClassroomRepository.cs`
- `Daycare.Application/Features/Parents/Commands/ParentCommands.cs`
- `Daycare.Application/Features/Parents/Commands/Handlers/ParentCommandHandlers.cs`
- `Daycare.Application/Features/Parents/Queries/ParentQueries.cs`
- `Daycare.Application/Features/Parents/Queries/Handlers/ParentQueryHandlers.cs`
- `Daycare.Application/Features/Classrooms/Commands/ClassroomCommands.cs`
- `Daycare.Application/Features/Classrooms/Commands/Handlers/ClassroomCommandHandlers.cs`
- `Daycare.Application/Features/Classrooms/Queries/ClassroomQueries.cs`
- `Daycare.Application/Features/Classrooms/Queries/Handlers/ClassroomQueryHandlers.cs`
- `Daycare.Application/DTOs/ParentDto.cs`
- `Daycare.Application/DTOs/ClassroomDto.cs`
- `Daycare.Infrastructure/Data/EntityConfigurations/ParentConfiguration.cs`
- `Daycare.Infrastructure/Repositories/ParentRepository.cs`
- `Daycare.Infrastructure/Repositories/ClassroomRepository.cs`
- `ECX_Daycare/Controllers/ParentsController.cs`
- `ECX_Daycare/Controllers/ClassroomsController.cs`
- `Daycare.Domain.Tests/Features/Parents/ParentAggregateTests.cs`

### Frontend
- `src/features/parents/types.ts`
- `src/features/parents/services/parentsApi.ts`
- `src/features/parents/hooks/useParents.ts`
- `src/features/parents/components/ParentForm.tsx`
- `src/features/parents/pages/ParentsListPage.tsx`
- `src/features/parents/pages/ParentCreatePage.tsx`
- `src/features/parents/pages/ParentEditPage.tsx`
- `src/features/parents/pages/ParentDetailPage.tsx`
- `src/features/parents/index.ts` (barrel export)
- `src/features/classrooms/types.ts`
- `src/features/classrooms/services/classroomsApi.ts`
- `src/features/classrooms/hooks/useClassrooms.ts`
- `src/features/classrooms/components/ClassroomForm.tsx`
- `src/features/classrooms/pages/ClassroomsListPage.tsx`
- `src/features/classrooms/pages/ClassroomCreatePage.tsx`
- `src/features/classrooms/pages/ClassroomEditPage.tsx`
- `src/features/classrooms/pages/ClassroomDetailPage.tsx`
- `src/features/classrooms/index.ts` (barrel export)

### Configuration Updates
- `src/routes/index.tsx` - Added Parents/Classrooms routes
- `src/components/Sidebar.tsx` - Added Parents/Classrooms navigation

## API Endpoints

### Parents
- `GET /api/parents` - List with pagination
- `GET /api/parents/search?q={term}` - Search
- `GET /api/parents/{id}` - Get by ID
- `POST /api/parents` - Create
- `PUT /api/parents/{id}` - Update
- `DELETE /api/parents/{id}` - Delete
- `POST /api/parents/{parentId}/children/{childId}` - Link child
- `DELETE /api/parents/{parentId}/children/{childId}` - Unlink child
- `GET /api/parents/child/{childId}` - Get parents by child

### Classrooms
- `GET /api/classrooms` - List with pagination
- `GET /api/classrooms/search?q={term}` - Search
- `GET /api/classrooms/{id}` - Get by ID
- `POST /api/classrooms` - Create
- `PUT /api/classrooms/{id}` - Update
- `DELETE /api/classrooms/{id}` - Delete
- `POST /api/classrooms/{classroomId}/children/{childId}` - Enroll child
- `DELETE /api/classrooms/{classroomId}/children/{childId}` - Remove child

## React Features

### Parents Module
- **ParentsListPage**: Paginated list with search, status filtering
- **ParentCreatePage**: Form to create new parent
- **ParentEditPage**: Form to edit existing parent
- **ParentDetailPage**: Read-only parent details view
- **useParents Hook**: State management for parent data
- **parentsApi Service**: Centralized API calls

### Classrooms Module
- **ClassroomsListPage**: Paginated list with search, capacity visualization
- **ClassroomCreatePage**: Form to create new classroom
- **ClassroomEditPage**: Form to edit existing classroom
- **ClassroomDetailPage**: Read-only classroom details with enrollment bar chart
- **useClassrooms Hook**: State management for classroom data
- **classroomsApi Service**: Centralized API calls

## Navigation
Both Parents and Classrooms are integrated into the main Sidebar with icon navigation:
- Parents: Home icon
- Classrooms: Layers icon

## Outstanding Backend Work
Before full deployment, the following validations are required:
1. **BaseEntity/TypedId validation** - Ensure Parent/Classroom correctly inherit from BaseEntity and use TypedId
2. **Child entity updates** - Add `ClassroomId`, `Classroom` navigation property, `ParentChildren` collection, and methods: `AssignToClassroom()`, `RemoveFromClassroom()`
3. **DbContext updates** - Add `DbSet<Parent>`, `DbSet<Classroom>` and register model configurations
4. **EF Configuration fixes** - Validate and correct ParentConfiguration, create ClassroomConfiguration
5. **Service registration** - Register repositories, command/query handlers, and validators in DI container
6. **Database migration** - Create and apply migration for new tables
7. **CQRS handler compilation** - Verify all handlers compile against actual IMessageBus/IUnitOfWork signatures
8. **API response contract** - Confirm ApiResponse<T> and error handling patterns
9. **Unit/Integration tests** - Add tests for Parent/Classroom handlers and repository queries
10. **API route conflicts** - Review controller routing, especially literal routes like `/search` vs `/{id}`

## Outstanding Frontend Work
Before full deployment:
1. Run `npm run build` to verify TypeScript compilation
2. Test all CRUD operations against actual backend
3. Validate Axios API errors and response handling
4. Add input validation and form error messages
5. Test pagination, search, and filtering
6. Verify capacity visualization correctness in ClassroomDetailPage

## Next Steps
1. Execute `dotnet build` to identify compilation errors
2. Fix BaseEntity/DbContext/Child entity relationships
3. Create and apply EF migrations
4. Register all services in DI container
5. Run integration tests
6. Execute `npm run build && npm run lint` for frontend
7. Perform manual end-to-end testing across Parents/Classrooms/Children features
8. Deploy Phase 7 complete

---
**Phase 7 Status**: Scaffolding complete; awaiting backend validation and compilation fixes.
