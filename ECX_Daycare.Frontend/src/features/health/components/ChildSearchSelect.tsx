import { useEffect, useState } from 'react'
import { Check, ChevronDown, Search } from 'lucide-react'
import type { ChildOption } from '../types'

interface ChildSearchSelectProps {
  children: ChildOption[]
  value: string
  onChange: (childId: string) => void
  disabled?: boolean
  placeholder?: string
}

export const ChildSearchSelect = ({
  children,
  value,
  onChange,
  disabled = false,
  placeholder = 'Search children by name',
}: ChildSearchSelectProps) => {
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const selected = children.find((child) => child.id === value)
  const matches = children.filter((child) =>
    child.name.toLocaleLowerCase().includes(query.trim().toLocaleLowerCase()),
  )

  useEffect(() => {
    if (selected && !open) setQuery(selected.name)
  }, [selected, open])

  return (
    <div
      className="relative"
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget as Node | null)) setOpen(false)
      }}
    >
      <div className="relative">
        <Search size={17} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          role="combobox"
          aria-label="Search children"
          aria-expanded={open}
          aria-controls="health-child-options"
          aria-autocomplete="list"
          autoComplete="off"
          value={selected && !open ? selected.name : query}
          onFocus={() => setOpen(true)}
          onChange={(event) => {
            setQuery(event.target.value)
            onChange('')
            setOpen(true)
          }}
          disabled={disabled}
          placeholder={placeholder}
          className="w-full rounded-xl border border-slate-200 bg-white py-3 pl-10 pr-10 text-sm text-slate-800 outline-none transition focus:border-teal-500 focus:ring-4 focus:ring-teal-500/10 disabled:bg-slate-100"
        />
        <ChevronDown size={16} className="pointer-events-none absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
      </div>
      {open && !disabled && (
        <div
          id="health-child-options"
          role="listbox"
          className="absolute z-30 mt-2 max-h-64 w-full overflow-y-auto rounded-xl border border-slate-200 bg-white p-1.5 shadow-xl"
        >
          {matches.length ? matches.map((child) => (
            <button
              key={child.id}
              type="button"
              role="option"
              aria-selected={child.id === value}
              onClick={() => {
                onChange(child.id)
                setQuery(child.name)
                setOpen(false)
              }}
              className="flex w-full items-center justify-between rounded-lg px-3 py-2.5 text-left text-sm text-slate-700 transition hover:bg-teal-50 hover:text-teal-800"
            >
              {child.name}
              {child.id === value && <Check size={16} className="text-teal-600" />}
            </button>
          )) : <p className="px-3 py-3 text-sm text-slate-500">No children found.</p>}
        </div>
      )}
    </div>
  )
}
