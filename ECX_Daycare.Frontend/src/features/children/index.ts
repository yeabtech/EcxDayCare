/**
 * Children feature exports
 * Centralized exports for all children feature components and services
 */

// Pages
export { ChildrenListPage } from './pages/ChildrenListPage'
export { ChildDetailsPage } from './pages/ChildDetailsPage'
export { ChildCreatePage } from './pages/ChildCreatePage'
export { ChildEditPage } from './pages/ChildEditPage'

// Components
export { ChildCard } from './components/ChildCard'
export { ChildForm } from './components/ChildForm'
export { ParentMultiSelect } from './components/ParentMultiSelect'
export { PhotoUpload } from './components/PhotoUpload'
export { DeleteConfirmation } from './components/DeleteConfirmation'
export { EmptyState } from './components/EmptyState'
export { LoadingState } from './components/LoadingState'

// Types
export type {
  Child,
  ChildFormInput,
  ChildrenListResponse,
  ChildrenListParams,
  ChildResponse,
  ChildrenResponse,
  ParentAssignment,
  ParentLink,
  ParentRole,
} from './types'

// Services
export { childrenApi } from './services/childrenApi'

// Hooks
export { useChildren } from './hooks/useChildren'
