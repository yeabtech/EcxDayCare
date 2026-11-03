/**
 * ParentMultiSelect
 * Searchable multi-select for assigning one or more parents to a child.
 *
 * Each selected parent carries a role (Mother / Father / Guardian) which is
 * persisted on the ParentChildren join table via the child create/update payload.
 */
import { useEffect, useMemo, useRef, useState } from "react";
import { Check, ChevronDown, Loader2, Search, UserPlus, X } from "lucide-react";
import { parentsApi } from "@features/parents/services/parentsApi";
import type { Parent } from "@features/parents/types";
import type { ParentAssignment, ParentRole } from "../types";

const ROLE_OPTIONS: ParentRole[] = ["Mother", "Father", "Guardian"];

interface ParentMultiSelectProps {
  /** Currently selected parent assignments (id + role). */
  value: ParentAssignment[];
  onChange: (value: ParentAssignment[]) => void;
  /** Parent IDs that already have a link, used to preselect on edit. */
  error?: string;
  disabled?: boolean;
}

export const ParentMultiSelect = ({
  value,
  onChange,
  error,
  disabled = false,
}: ParentMultiSelectProps) => {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<Parent[]>([]);
  const [selectedParents, setSelectedParents] = useState<
    Record<string, Parent>
  >({});
  const [isSearching, setIsSearching] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  // Load a page of parents on mount so the dropdown is not empty before typing.
  useEffect(() => {
    let active = true;
    const load = async () => {
      setIsSearching(true);
      try {
        const data = await parentsApi.getParents({ page: 1, pageSize: 25 });
        if (active) setResults(data.items ?? []);
      } catch {
        if (active) setResults([]);
      } finally {
        if (active) setIsSearching(false);
      }
    };
    load();
    return () => {
      active = false;
    };
  }, []);

  // Debounced search against the backend search endpoint.
  useEffect(() => {
    const term = query.trim();
    if (term.length === 0) return;

    let active = true;
    const handle = setTimeout(async () => {
      setIsSearching(true);
      try {
        const data = await parentsApi.getParents({
          search: term,
          page: 1,
          pageSize: 25,
        });
        if (active) setResults(data.items ?? []);
      } catch {
        if (active) setResults([]);
      } finally {
        if (active) setIsSearching(false);
      }
    }, 300);

    return () => {
      active = false;
      clearTimeout(handle);
    };
  }, [query]);

  // Close the dropdown when clicking outside.
  useEffect(() => {
    const onDocumentClick = (e: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(e.target as Node)
      ) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", onDocumentClick);
    return () => document.removeEventListener("mousedown", onDocumentClick);
  }, []);

  const selectedIds = useMemo(
    () => new Set(value.map((v) => v.ParentId)),
    [value],
  );
  const selectedParentIds = useMemo(
    () => value.map((assignment) => assignment.ParentId).sort().join('|'),
    [value],
  );

  // On child edit, the saved ParentChildren links contain IDs and roles only.
  // Resolve each linked parent so the selected chips display real names rather
  // than exposing the raw database identifier.
  useEffect(() => {
    const ids = selectedParentIds ? selectedParentIds.split('|') : [];
    if (!ids.length) return;
    let active = true;
    void Promise.allSettled(ids.map((id) => parentsApi.getParent(id))).then((results) => {
      if (!active) return;
      setSelectedParents((current) => {
        const next = { ...current };
        results.forEach((result, index) => {
          if (result.status === 'fulfilled') next[ids[index]] = result.value;
        });
        return next;
      });
    });
    return () => { active = false; };
  }, [selectedParentIds]);

  /**
   * Backend ParentDto is PascalCase; the parents feature normalises to camelCase,
   * so read both shapes defensively.
   */
  const parentId = (p: Parent) => String((p as any).id ?? (p as any).Id ?? "");
  const parentFirstName = (p: Parent) =>
    String((p as any).firstName ?? (p as any).FirstName ?? "");
  const parentLastName = (p: Parent) =>
    String((p as any).lastName ?? (p as any).LastName ?? "");
  const parentEmail = (p: Parent) =>
    String((p as any).email ?? (p as any).Email ?? "");

  const handleAdd = (parent: Parent) => {
    const id = parentId(parent);
    if (!id || selectedIds.has(id)) return;

    setSelectedParents((prev) => ({ ...prev, [id]: parent }));
    onChange([...value, { ParentId: id, Role: "Guardian" }]);
    setQuery("");
  };

  const handleRemove = (id: string) => {
    onChange(value.filter((v) => v.ParentId !== id));
  };

  const handleRoleChange = (id: string, role: ParentRole) => {
    onChange(value.map((v) => (v.ParentId === id ? { ...v, Role: role } : v)));
  };

  const visibleResults = results.filter((p) => !selectedIds.has(parentId(p)));

  return (
    <div ref={containerRef} className="relative">
      {/* Search input */}
      <div className="relative">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-gray-400">
          <Search className="w-4 h-4" />
        </div>
        <input
          type="text"
          value={query}
          disabled={disabled}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => setIsOpen(true)}
          placeholder="Search parents by name or email…"
          className={`w-full pl-9 pr-9 py-2 border rounded-lg focus:outline-none focus:ring-2 disabled:bg-gray-100 ${
            error
              ? "border-red-500 focus:ring-red-500"
              : "border-gray-300 focus:ring-primary-500"
          }`}
        />
        <div className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400">
          {isSearching ? (
            <Loader2 className="w-4 h-4 animate-spin" />
          ) : (
            <ChevronDown className="w-4 h-4" />
          )}
        </div>
      </div>

      {error && <p className="text-red-500 text-xs mt-1">{error}</p>}

      {/* Dropdown */}
      {isOpen && !disabled && (
        <div className="absolute z-20 mt-1 w-full bg-white border border-gray-200 rounded-lg shadow-lg max-h-64 overflow-y-auto">
          {isSearching && visibleResults.length === 0 ? (
            <div className="px-4 py-3 text-sm text-gray-500 flex items-center gap-2">
              <Loader2 className="w-4 h-4 animate-spin" /> Searching…
            </div>
          ) : visibleResults.length === 0 ? (
            <div className="px-4 py-3 text-sm text-gray-500">
              {query.trim()
                ? "No matching parents found."
                : "No parents available. Create a parent first."}
            </div>
          ) : (
            visibleResults.map((p) => {
              const id = parentId(p);
              return (
                <button
                  key={id}
                  type="button"
                  onClick={() => handleAdd(p)}
                  className="w-full flex items-center gap-3 px-4 py-2 text-left hover:bg-primary-50 transition-colors"
                >
                  <UserPlus className="w-4 h-4 text-primary-600 shrink-0" />
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      {parentFirstName(p)} {parentLastName(p)}
                    </p>
                    <p className="text-xs text-gray-500 truncate">
                      {parentEmail(p)}
                    </p>
                  </div>
                </button>
              );
            })
          )}
        </div>
      )}

      {/* Selected chips with role selector */}
      {value.length > 0 && (
        <ul className="mt-3 space-y-2">
          {value.map((assignment) => {
            const parent = selectedParents[assignment.ParentId];
            const label = parent
              ? `${parentFirstName(parent)} ${parentLastName(parent)}`
              : assignment.ParentId;
            return (
              <li
                key={assignment.ParentId}
                className="flex items-center gap-3 p-2 bg-gray-50 border border-gray-200 rounded-lg"
              >
                <Check className="w-4 h-4 text-green-600 shrink-0" />
                <span className="flex-1 text-sm text-gray-800 truncate">
                  {label}
                </span>

                <select
                  value={assignment.Role}
                  disabled={disabled}
                  onChange={(e) =>
                    handleRoleChange(
                      assignment.ParentId,
                      e.target.value as ParentRole,
                    )
                  }
                  className="text-xs border border-gray-300 rounded-md px-2 py-1 bg-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  {ROLE_OPTIONS.map((role) => (
                    <option key={role} value={role}>
                      {role}
                    </option>
                  ))}
                </select>

                <button
                  type="button"
                  disabled={disabled}
                  onClick={() => handleRemove(assignment.ParentId)}
                  className="p-1 text-gray-400 hover:text-red-600 transition-colors"
                  aria-label={`Remove ${label}`}
                >
                  <X className="w-4 h-4" />
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
};
