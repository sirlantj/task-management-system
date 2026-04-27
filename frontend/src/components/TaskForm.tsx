import { useState, type FormEvent } from 'react';
import type { TaskStatus } from '../types/task';

export interface TaskFormValues {
  title: string;
  description: string;
  dueDate: string;
  status: TaskStatus;
}

interface TaskFormProps {
  initialValues?: Partial<TaskFormValues>;
  showStatus?: boolean;
  currentStatus?: TaskStatus;
  isLoading: boolean;
  error: string | null;
  submitLabel: string;
  onSubmit: (values: TaskFormValues) => void;
  onCancel: () => void;
}

const VALID_NEXT_STATUSES: Record<TaskStatus, TaskStatus[]> = {
  Pending: ['Pending', 'InProgress', 'Done'],
  InProgress: ['InProgress', 'Done'],
  Done: ['Done'],
};

const STATUS_LABELS: Record<TaskStatus, string> = {
  Pending: 'Pending',
  InProgress: 'In Progress',
  Done: 'Done',
};

function isoToDateInput(iso: string | null | undefined): string {
  if (!iso) return '';
  return iso.split('T')[0];
}

export function TaskForm({
  initialValues,
  showStatus = false,
  currentStatus,
  isLoading,
  error,
  submitLabel,
  onSubmit,
  onCancel,
}: TaskFormProps) {
  const [title, setTitle] = useState(initialValues?.title ?? '');
  const [description, setDescription] = useState(initialValues?.description ?? '');
  const [dueDate, setDueDate] = useState(isoToDateInput(initialValues?.dueDate));
  const [status, setStatus] = useState<TaskStatus>(initialValues?.status ?? 'Pending');
  const [titleError, setTitleError] = useState('');

  const validStatuses = currentStatus ? VALID_NEXT_STATUSES[currentStatus] : (['Pending'] as TaskStatus[]);

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!title.trim()) {
      setTitleError('Title is required.');
      return;
    }
    setTitleError('');
    onSubmit({ title: title.trim(), description, dueDate, status });
  };

  const inputClass =
    'mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm ' +
    'focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

  return (
    <form onSubmit={handleSubmit} noValidate className="space-y-5">
      {error && (
        <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <div>
        <label htmlFor="title" className="block text-sm font-medium text-gray-700">
          Title <span className="text-red-500">*</span>
        </label>
        <input
          id="title"
          type="text"
          value={title}
          onChange={e => setTitle(e.target.value)}
          className={inputClass}
          placeholder="What needs to be done?"
        />
        {titleError && <p className="mt-1 text-xs text-red-600">{titleError}</p>}
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700">
          Description
        </label>
        <textarea
          id="description"
          value={description}
          onChange={e => setDescription(e.target.value)}
          rows={3}
          className={inputClass}
          placeholder="Optional details"
        />
      </div>

      <div>
        <label htmlFor="dueDate" className="block text-sm font-medium text-gray-700">
          Due Date
        </label>
        <input
          id="dueDate"
          type="date"
          value={dueDate}
          onChange={e => setDueDate(e.target.value)}
          className={inputClass}
        />
      </div>

      {showStatus && (
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700">
            Status
          </label>
          <select
            id="status"
            value={status}
            onChange={e => setStatus(e.target.value as TaskStatus)}
            className={inputClass}
          >
            {(Object.keys(STATUS_LABELS) as TaskStatus[]).map(s => (
              <option key={s} value={s} disabled={!validStatuses.includes(s)}>
                {STATUS_LABELS[s]}
              </option>
            ))}
          </select>
        </div>
      )}

      <div className="flex gap-3 pt-1">
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 rounded-md bg-blue-600 text-white text-sm font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {isLoading ? 'Saving…' : submitLabel}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isLoading}
          className="px-4 py-2 rounded-md bg-gray-100 text-gray-700 text-sm font-medium hover:bg-gray-200 disabled:opacity-50 transition-colors"
        >
          Cancel
        </button>
      </div>
    </form>
  );
}
