/**
 * Caching Utilities
 * Client-side caching layer for API responses and data
 */

export type CacheStrategy = 'memory' | 'localStorage' | 'both'

export interface CacheConfig {
  ttl?: number // Time to live in milliseconds
  strategy?: CacheStrategy
  key: string
}

export interface CacheEntry<T> {
  data: T
  timestamp: number
  ttl: number
}

/**
 * In-memory cache store
 */
const memoryCache = new Map<string, CacheEntry<any>>()

/**
 * Local storage cache store (with prefix)
 */
const CACHE_PREFIX = 'ecx_cache_'

/**
 * Check if cache entry is still valid
 */
const isCacheValid = (entry: CacheEntry<any>): boolean => {
  if (entry.ttl === 0) return true // No expiration
  return Date.now() - entry.timestamp < entry.ttl
}

/**
 * Get data from memory cache
 */
const getFromMemory = <T = any>(key: string): T | null => {
  const entry = memoryCache.get(key)
  if (!entry) return null
  if (!isCacheValid(entry)) {
    memoryCache.delete(key)
    return null
  }
  return entry.data as T
}

/**
 * Set data in memory cache
 */
const setInMemory = <T = any>(key: string, data: T, ttl: number = 0): void => {
  memoryCache.set(key, {
    data,
    timestamp: Date.now(),
    ttl,
  })
}

/**
 * Get data from local storage cache
 */
const getFromLocalStorage = <T = any>(key: string): T | null => {
  try {
    const stored = localStorage.getItem(CACHE_PREFIX + key)
    if (!stored) return null

    const entry: CacheEntry<T> = JSON.parse(stored)
    if (!isCacheValid(entry)) {
      localStorage.removeItem(CACHE_PREFIX + key)
      return null
    }

    return entry.data
  } catch (error) {
    console.error(`Error reading from cache: ${key}`, error)
    return null
  }
}

/**
 * Set data in local storage cache
 */
const setInLocalStorage = <T = any>(key: string, data: T, ttl: number = 0): void => {
  try {
    const entry: CacheEntry<T> = {
      data,
      timestamp: Date.now(),
      ttl,
    }
    localStorage.setItem(CACHE_PREFIX + key, JSON.stringify(entry))
  } catch (error) {
    console.error(`Error writing to cache: ${key}`, error)
  }
}

/**
 * Get cached data with fallback strategy
 */
export const getCached = <T = any>(
  key: string,
  strategy: CacheStrategy = 'memory'
): T | null => {
  if (strategy === 'memory' || strategy === 'both') {
    const data = getFromMemory<T>(key)
    if (data !== null) return data
  }

  if (strategy === 'localStorage' || strategy === 'both') {
    const data = getFromLocalStorage<T>(key)
    if (data !== null) {
      // Cache in memory for faster access
      if (strategy === 'both') {
        setInMemory(key, data, 5 * 60 * 1000) // 5 minutes in memory
      }
      return data
    }
  }

  return null
}

/**
 * Set cached data
 */
export const setCached = <T = any>(
  key: string,
  data: T,
  ttl: number = 5 * 60 * 1000, // 5 minutes default
  strategy: CacheStrategy = 'memory'
): void => {
  if (strategy === 'memory' || strategy === 'both') {
    setInMemory(key, data, ttl)
  }

  if (strategy === 'localStorage' || strategy === 'both') {
    setInLocalStorage(key, data, ttl)
  }
}

/**
 * Clear specific cache entry
 */
export const clearCache = (key: string, strategy: CacheStrategy = 'both'): void => {
  if (strategy === 'memory' || strategy === 'both') {
    memoryCache.delete(key)
  }

  if (strategy === 'localStorage' || strategy === 'both') {
    try {
      localStorage.removeItem(CACHE_PREFIX + key)
    } catch (error) {
      console.error(`Error clearing cache: ${key}`, error)
    }
  }
}

/**
 * Clear all cache entries
 */
export const clearAllCache = (strategy: CacheStrategy = 'both'): void => {
  if (strategy === 'memory' || strategy === 'both') {
    memoryCache.clear()
  }

  if (strategy === 'localStorage' || strategy === 'both') {
    try {
      const keys = Object.keys(localStorage)
      keys.forEach((key) => {
        if (key.startsWith(CACHE_PREFIX)) {
          localStorage.removeItem(key)
        }
      })
    } catch (error) {
      console.error('Error clearing all cache', error)
    }
  }
}

/**
 * Get cache statistics
 */
export const getCacheStats = () => {
  return {
    memorySize: memoryCache.size,
    localStorageSize:
      typeof localStorage !== 'undefined'
        ? Object.keys(localStorage).filter((k) => k.startsWith(CACHE_PREFIX)).length
        : 0,
  }
}

/**
 * Decorator function to cache async function results
 */
export const withCache = <T extends any[], R>(
  fn: (...args: T) => Promise<R>,
  options: { ttl?: number; strategy?: CacheStrategy; keyFn?: (...args: T) => string } = {}
) => {
  const { ttl = 5 * 60 * 1000, strategy = 'memory', keyFn } = options

  return async (...args: T): Promise<R> => {
    const cacheKey = keyFn ? keyFn(...args) : JSON.stringify(args)

    // Try to get from cache first
    const cached = getCached<R>(cacheKey, strategy)
    if (cached !== null) {
      return cached
    }

    // Fetch fresh data
    const result = await fn(...args)

    // Cache the result
    setCached(cacheKey, result, ttl, strategy)

    return result
  }
}
