import type { Config } from 'tailwindcss'

export default {
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Open Sans', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        'page-title': ['2rem', { fontWeight: '700', lineHeight: '2.5rem' }],
        'section-title': ['1.5rem', { fontWeight: '600', lineHeight: '2rem' }],
        'button': ['0.875rem', { fontWeight: '600', lineHeight: '1.25rem' }],
        'table-header': ['0.875rem', { fontWeight: '600', lineHeight: '1.25rem' }],
      },
      colors: {
        // Primary Green - Daycare Theme
        primary: {
          50: '#f2f7f5',
          100: '#e6efeb',
          200: '#c2dfd5',
          300: '#9dcfbf',
          400: '#5daea8',
          500: '#1B5E3A', // Main brand green
          600: '#184d33',
          700: '#153e2a',
          800: '#122f22',
          900: '#0e231a',
        },
        // Accent Gold/Orange
        accent: {
          50: '#fef9f3',
          100: '#fdf3e6',
          200: '#fae7cd',
          300: '#f7dbb3',
          400: '#f1c985',
          500: '#F5A623', // Main accent gold
          600: '#daa02e',
          700: '#b98325',
          800: '#99651d',
          900: '#7a4d15',
        },
        // Semantic colors
        success: {
          50: '#f0fcf4',
          100: '#dffce5',
          500: '#10b981',
          600: '#059669',
          700: '#047857',
        },
        warning: {
          50: '#fffbeb',
          100: '#fef3c7',
          500: '#f59e0b',
          600: '#d97706',
          700: '#b45309',
        },
        danger: {
          50: '#fef2f2',
          100: '#fee2e2',
          500: '#ef4444',
          600: '#dc2626',
          700: '#b91c1c',
        },
        info: {
          50: '#f0f9ff',
          100: '#e0f2fe',
          500: '#0ea5e9',
          600: '#0284c7',
          700: '#0369a1',
        },
      },
    },
  },
  plugins: [],
} satisfies Config
