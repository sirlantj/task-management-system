import type { Task } from '../types/task';

interface TaskCardProps {
  task: Task;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}

const STATUS_STYLES: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Done: 'bg-green-100 text-green-800',
};

const STATUS_LABELS: Record<string, string> = {
  Pending: 'Pending',
  InProgress: 'In Progress',
  Done: 'Done',
};

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function TaskCard({ task, onEdit, onDelete }: TaskCardProps) {
  const isDone = task.status === 'Done';

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-5 flex flex-col gap-3">
      <div className="flex items-start justify-between gap-3">
        <h3 className="font-medium text-gray-900 break-words leading-snug">{task.title}</h3>
        <span
          className={`shrink-0 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLES[task.status] ?? 'bg-gray-100 text-gray-700'}`}
        >
          {STATUS_LABELS[task.status] ?? task.status}
        </span>
      </div>

      {task.description && (
        <p className="text-sm text-gray-600 break-words">{task.description}</p>
      )}

      {task.dueDate && (
        <p className="text-xs text-gray-400">Due: {formatDate(task.dueDate)}</p>
      )}

      <div className="flex gap-2 pt-1">
        <button
          onClick={() => onEdit(task.id)}
          className="text-sm px-3 py-1.5 rounded bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors"
        >
          Edit
        </button>
        <button
          onClick={() => onDelete(task.id)}
          disabled={isDone}
          title={isDone ? 'Completed tasks cannot be deleted' : undefined}
          className={`text-sm px-3 py-1.5 rounded transition-colors ${
            isDone
              ? 'bg-gray-100 text-gray-300 cursor-not-allowed'
              : 'bg-red-50 hover:bg-red-100 text-red-700'
          }`}
        >
          Delete
        </button>
      </div>
    </div>
  );
}
