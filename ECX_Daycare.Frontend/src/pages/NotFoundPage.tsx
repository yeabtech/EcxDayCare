/**
 * Not Found Page
 */
import { Link } from 'react-router-dom'
import { ROUTES } from '@routes/constants'

export const NotFoundPage = () => {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-gray-900">404</h1>
        <p className="text-xl text-gray-600 mt-4">Page not found</p>
        <p className="text-gray-500 mt-2">The page you're looking for doesn't exist or has been moved.</p>
        <Link
          to={ROUTES.DASHBOARD}
          className="mt-8 inline-block btn-primary"
        >
          Go to Dashboard
        </Link>
      </div>
    </div>
  )
}
