/**
 * Performance Monitoring Utilities
 * Track and analyze performance metrics
 */

export interface PerformanceMetric {
  name: string
  duration: number
  timestamp: number
  details?: Record<string, any>
}

class PerformanceMonitor {
  private metrics: PerformanceMetric[] = []
  private timers: Map<string, number> = new Map()
  private maxMetricsSize = 1000

  /**
   * Start timing a metric
   */
  start(name: string): void {
    this.timers.set(name, performance.now())
  }

  /**
   * End timing and record metric
   */
  end(name: string, details?: Record<string, any>): PerformanceMetric | null {
    const startTime = this.timers.get(name)
    if (!startTime) {
      console.warn(`Timer "${name}" not started`)
      return null
    }

    const duration = performance.now() - startTime
    const metric: PerformanceMetric = {
      name,
      duration,
      timestamp: Date.now(),
      details,
    }

    this.metrics.push(metric)

    // Keep metrics within size limit
    if (this.metrics.length > this.maxMetricsSize) {
      this.metrics.shift()
    }

    this.timers.delete(name)
    return metric
  }

  /**
   * Measure a synchronous function
   */
  measure<T>(name: string, fn: () => T, details?: Record<string, any>): T {
    this.start(name)
    try {
      const result = fn()
      this.end(name, details)
      return result
    } catch (error) {
      this.end(name, { ...details, error: String(error) })
      throw error
    }
  }

  /**
   * Measure an async function
   */
  async measureAsync<T>(
    name: string,
    fn: () => Promise<T>,
    details?: Record<string, any>
  ): Promise<T> {
    this.start(name)
    try {
      const result = await fn()
      this.end(name, details)
      return result
    } catch (error) {
      this.end(name, { ...details, error: String(error) })
      throw error
    }
  }

  /**
   * Get average duration for a metric name
   */
  getAverageDuration(name: string): number {
    const relevantMetrics = this.metrics.filter((m) => m.name === name)
    if (relevantMetrics.length === 0) return 0

    const total = relevantMetrics.reduce((sum, m) => sum + m.duration, 0)
    return total / relevantMetrics.length
  }

  /**
   * Get metrics by name
   */
  getMetrics(name?: string): PerformanceMetric[] {
    if (!name) return this.metrics

    return this.metrics.filter((m) => m.name === name)
  }

  /**
   * Get performance summary
   */
  getSummary(): Record<string, { count: number; avg: number; min: number; max: number }> {
    const summary: Record<string, { count: number; avg: number; min: number; max: number }> = {}

    this.metrics.forEach((metric) => {
      if (!summary[metric.name]) {
        summary[metric.name] = { count: 0, avg: 0, min: Infinity, max: -Infinity }
      }

      const stats = summary[metric.name]
      stats.count++
      stats.avg = (stats.avg * (stats.count - 1) + metric.duration) / stats.count
      stats.min = Math.min(stats.min, metric.duration)
      stats.max = Math.max(stats.max, metric.duration)
    })

    return summary
  }

  /**
   * Clear all metrics
   */
  clear(): void {
    this.metrics = []
    this.timers.clear()
  }

  /**
   * Clear specific metric
   */
  clearMetric(name: string): void {
    this.metrics = this.metrics.filter((m) => m.name !== name)
    this.timers.delete(name)
  }

  /**
   * Log performance report to console
   */
  report(): void {
    const summary = this.getSummary()
    console.table(summary)
  }

  /**
   * Export metrics as JSON
   */
  export(): string {
    return JSON.stringify(
      {
        metrics: this.metrics,
        summary: this.getSummary(),
        exportTime: new Date().toISOString(),
      },
      null,
      2
    )
  }
}

// Global instance
export const performanceMonitor = new PerformanceMonitor()

/**
 * Decorator to automatically measure function execution
 */
export function Measure(target: any, propertyKey: string, descriptor: PropertyDescriptor) {
  const originalMethod = descriptor.value

  descriptor.value = function (...args: any[]) {
    const metricName = `${target.constructor.name}.${propertyKey}`
    return performanceMonitor.measure(metricName, () => originalMethod.apply(this, args))
  }

  return descriptor
}

/**
 * High Resolution Timer utility
 */
export class Timer {
  private startTime: number

  constructor() {
    this.startTime = performance.now()
  }

  /**
   * Get elapsed time in milliseconds
   */
  elapsed(): number {
    return performance.now() - this.startTime
  }

  /**
   * Get elapsed time in seconds
   */
  elapsedSeconds(): number {
    return this.elapsed() / 1000
  }

  /**
   * Reset timer
   */
  reset(): void {
    this.startTime = performance.now()
  }

  /**
   * Get formatted elapsed time
   */
  toString(): string {
    const elapsed = this.elapsed()
    if (elapsed < 1000) return `${elapsed.toFixed(2)}ms`
    return `${(elapsed / 1000).toFixed(2)}s`
  }
}
