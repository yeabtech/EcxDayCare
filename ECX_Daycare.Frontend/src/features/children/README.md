# Children Feature Documentation

## Overview

The Children feature module provides complete CRUD functionality for managing child records in the ECX Daycare system. It integrates with the ASP.NET Core Children API and provides a professional, user-friendly interface.

## Features

✅ **List View**
- Display all children with pagination
- Search by name or parent name
- Status badges (Active, Inactive, Enrolled, Graduated)
- Allergies alert display
- Quick delete from hover menu

✅ **Create**
- Comprehensive form with validation
- Personal information section
- Parent/guardian details
- Medical information (allergies, medical history)
- Enrollment date and status
- Optional notes field

✅ **Details View**
- Full child information display
- Organized sections for easy reading
- Edit and delete actions
- Medical alerts for allergies
- System metadata (created/updated dates)

✅ **Edit**
- In-place editing with form validation
- Toggle between view and edit modes
- Success/error message handling
- Cancel to discard changes

✅ **Delete**
- Confirmation dialog to prevent accidental deletion
- Clear warning about irreversible action
- Smooth redirect after deletion

## Architecture

### Directory Structure

```
src/features/children/
├── types.ts                      # TypeScript interfaces
├── services/
│   └── childrenApi.ts          # API service layer
├── hooks/
│   └── useChildren.ts          # State management hook
├── components/
│   ├── ChildCard.tsx           # List item card
│   ├── ChildForm.tsx           # Create/Edit form
│   ├── DeleteConfirmation.tsx   # Delete dialog
│   ├── EmptyState.tsx          # Empty list state
│   └── LoadingState.tsx        # Loading skeleton
├── pages/
│   ├── ChildrenListPage.tsx    # List view
│   ├── ChildDetailsPage.tsx    # Details/Edit view
│   └── ChildCreatePage.tsx     # Create view
└── index.ts                     # Feature exports
```

### Communication Pattern

All API calls flow through a centralized service:

```
Components → useChildren Hook → childrenApi Service → Axios Client → Backend API
```

This ensures:
- No direct API calls in components
- Consistent error handling
- Reusable business logic
- Easy to test

## Usage

### Importing Components

```typescript
// Import from feature barrel export
import { ChildrenListPage, ChildDetailsPage } from '@features/children'

// Or specific components
import { ChildCard, ChildForm } from '@features/children'
```

### Using the Hook

```typescript
import { useChildren } from '@features/children'

function MyComponent() {
  const { 
	children,      // ChildrenListResponse
	isLoading,     // boolean
	error,         // string | null
	fetchChildren, // async function
	createChild,   // async function
  } = useChildren()

  // Fetch children
  const handleLoad = async () => {
	await fetchChildren({
	  page: 1,
	  pageSize: 10,
	  search: 'John'
	})
  }
}
```

### API Service Usage

```typescript
import { childrenApi } from '@features/children'

// Get paginated list
const list = await childrenApi.getChildren({
  page: 1,
  pageSize: 10,
  search: 'search term'
})

// Get single child
const child = await childrenApi.getChild('child-id')

// Create
const newChild = await childrenApi.createChild(formData)

// Update
const updated = await childrenApi.updateChild('id', formData)

// Delete
await childrenApi.deleteChild('id')
```

## Backend API Requirements

The feature expects the following API endpoints:

### GET /api/children
- **Params**: `page`, `pageSize`, `search`, `status`
- **Response**:
```json
{
  "data": {
	"items": [
	  {
		"id": "uuid",
		"firstName": "John",
		"lastName": "Doe",
		"dateOfBirth": "2020-01-01T00:00:00Z",
		"gender": "Male",
		"parentName": "Jane Doe",
		"parentEmail": "jane@example.com",
		"parentPhone": "123-456-7890",
		"address": "123 Main St",
		"allergies": "Peanuts",
		"medicalHistory": "None",
		"notes": "Some notes",
		"enrollmentDate": "2023-01-01T00:00:00Z",
		"status": "Active",
		"createdAt": "2023-01-01T00:00:00Z",
		"updatedAt": "2023-01-01T00:00:00Z"
	  }
	],
	"total": 100,
	"page": 1,
	"pageSize": 10
  },
  "success": true,
  "message": "Children retrieved successfully"
}
```

### GET /api/children/:id
- **Response**: Single child object

### POST /api/children
- **Body**: ChildFormInput (all required fields)
- **Response**: Created child object

### PUT /api/children/:id
- **Body**: ChildFormInput
- **Response**: Updated child object

### DELETE /api/children/:id
- **Response**: 200 status

## State Management

The `useChildren` hook manages:

```typescript
interface UseChildrenState {
  children: ChildrenListResponse | null  // Paginated list
  child: Child | null                    // Single child
  isLoading: boolean                     // Fetching list
  isCreating: boolean                    // Creating
  isUpdating: boolean                    // Updating
  isDeleting: boolean                    // Deleting
  error: string | null                   // Last error
  success: string | null                 // Last success
}
```

## Error Handling

Errors are handled at multiple levels:

1. **API Service**: Attempts operation, throws on failure
2. **Hook**: Catches error, updates state with message
3. **Component**: Displays error message to user
4. **User**: Can dismiss error and retry

Error messages:
- Network failures
- Validation errors (from backend)
- Not found errors
- Server errors

## Form Validation

The ChildForm component validates:

- Required fields (first name, last name, DOB, parent info, etc.)
- Email format
- Date formats

Validation occurs:
- **Client-side**: Immediate feedback before submission
- **Server-side**: Backend validates again (trust, but verify)

## Component Details

### ChildCard
- Displays single child summary
- Shows age, gender, parent, enrollment date
- Highlights allergies with warning
- Status badge with color coding
- Click to navigate to details

### ChildForm
- Comprehensive form for create/edit
- Organized into sections
- Real-time validation
- Clear error messages
- Submit button changes text between create/update

### DeleteConfirmation
- Modal dialog with overlay
- Shows item being deleted
- Requires confirmation
- Shows loading state during deletion
- Warning text about irreversible action

### EmptyState
- Shows when no children
- Optional action button
- Customizable message
- Icon from lucide-react

### LoadingState
- Skeleton loading
- Animates pulse effect
- Configurable count
- Professional appearance

## Extensibility

To add new features:

1. **New Status Type**: Update `ChildFormInput` and `Child` types
2. **New Fields**: Add to form, types, and API service
3. **New Filters**: Add to `ChildrenListParams` and API call
4. **Custom Hooks**: Create in `hooks/` directory
5. **New Pages**: Create in `pages/` directory and add routes

## Testing Strategy

Test coverage should include:

- **Components**: Visual tests, user interactions
- **Hook**: State changes, API calls, error handling
- **Service**: API call correctness
- **Integration**: Full flow from UI to API

## Performance Optimizations

- Pagination (default 10 per page)
- Debounced search
- Lazy loaded routes
- Memoized components (consider for future)
- API response caching (consider for future)

## Browser Support

Works in all modern browsers:
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Future Enhancements

- Batch operations (select multiple, delete)
- Export to CSV/PDF
- Bulk import
- Advanced filters (age range, status, etc.)
- Child photos/avatars
- Attendance integration
- Parent portal
- Email notifications

## Troubleshooting

### Children not loading
1. Check API is running
2. Verify `VITE_API_BASE_URL` in `.env`
3. Check browser console for errors
4. Verify API response format

### Form submission fails
1. Check validation error messages
2. Verify all required fields are filled
3. Check backend API error response
4. Verify API endpoint is correct

### State not updating
1. Check browser console for errors
2. Verify network request succeeded
3. Clear browser cache and reload
4. Check browser dev tools Network tab

## Contributing

When modifying this feature:
1. Keep components small and focused
2. Add JSDoc comments
3. Update this documentation
4. Test all CRUD operations
5. Test error scenarios
6. Test with slow network (DevTools)
