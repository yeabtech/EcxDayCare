/**
 * Export Data Utilities
 * Functions for exporting data in various formats (CSV, JSON, Excel)
 */

export type ExportFormat = 'csv' | 'json' | 'tsv'

export interface ExportOptions {
  filename?: string
  format?: ExportFormat
  fields?: string[]
  dateFormat?: string
}

/**
 * Convert array of objects to CSV string
 */
export const convertToCSV = <T extends Record<string, any>>(
  data: T[],
  fields?: string[]
): string => {
  if (data.length === 0) return ''

  // Get all keys or use specified fields
  const keys = fields || Object.keys(data[0])

  // Create header
  const header = keys.map((key) => `"${String(key).replace(/"/g, '""')}"`).join(',')

  // Create rows
  const rows = data.map((item) =>
    keys
      .map((key) => {
        const value = item[key]
        if (value === null || value === undefined) return ''
        if (typeof value === 'object') return `"${JSON.stringify(value).replace(/"/g, '""')}"`
        return `"${String(value).replace(/"/g, '""')}"`
      })
      .join(',')
  )

  return [header, ...rows].join('\n')
}

/**
 * Convert array of objects to TSV string
 */
export const convertToTSV = <T extends Record<string, any>>(
  data: T[],
  fields?: string[]
): string => {
  if (data.length === 0) return ''

  const keys = fields || Object.keys(data[0])

  const header = keys.join('\t')
  const rows = data.map((item) =>
    keys
      .map((key) => {
        const value = item[key]
        if (value === null || value === undefined) return ''
        if (typeof value === 'object') return JSON.stringify(value)
        return String(value)
      })
      .join('\t')
  )

  return [header, ...rows].join('\n')
}

/**
 * Convert array of objects to JSON string
 */
export const convertToJSON = <T extends Record<string, any>>(
  data: T[],
  fields?: string[]
): string => {
  if (fields) {
    const filtered = data.map((item) => {
      const obj: Record<string, any> = {}
      fields.forEach((field) => {
        obj[field] = item[field]
      })
      return obj
    })
    return JSON.stringify(filtered, null, 2)
  }
  return JSON.stringify(data, null, 2)
}

/**
 * Download data as file
 */
export const downloadFile = (content: string, filename: string, mimeType: string = 'text/plain') => {
  const blob = new Blob([content], { type: mimeType })
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

/**
 * Export data in specified format
 */
export const exportData = <T extends Record<string, any>>(
  data: T[],
  options: ExportOptions = {}
) => {
  const { filename = 'export', format = 'csv', fields } = options

  let content: string
  let mimeType: string

  switch (format) {
    case 'json':
      content = convertToJSON(data, fields)
      mimeType = 'application/json'
      break
    case 'tsv':
      content = convertToTSV(data, fields)
      mimeType = 'text/tab-separated-values'
      break
    case 'csv':
    default:
      content = convertToCSV(data, fields)
      mimeType = 'text/csv'
      break
  }

  const timestamp = new Date().toISOString().split('T')[0]
  const fullFilename = `${filename}_${timestamp}.${format}`

  downloadFile(content, fullFilename, mimeType)
}

/**
 * Export with format selection dialog
 */
export const exportDataWithFormat = <T extends Record<string, any>>(
  data: T[],
  baseName: string,
  fields?: string[]
) => {
  const format = prompt('Export format (csv, json, tsv):', 'csv')

  if (!format) return

  exportData(data, {
    filename: baseName,
    format: (format.toLowerCase() as ExportFormat) || 'csv',
    fields,
  })
}
