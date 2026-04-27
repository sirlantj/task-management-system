import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { tasksApi, ApiError } from '../api/client';
import type { Task } from '../types/task';
import { Layout } from '../components/Layout';
import { TaskCard } from '../components/TaskCard';

export default function TaskListPage() {
  const navigate = useNavigate();

  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    tasksApi
      .getAll()
      .then(setTasks)
      .catch(err =>
        setError(err instanceof ApiError ? err.message : 'Failed to load tasks.')
      )
      .finally(() => setIsLoading(false));
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this task? This action cannot be undone.')) return;
    try {
      await tasksApi.delete(id);
      setTasks(prev => prev.filter(t => t.id !== id));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to delete task.');
    }
  };

  return (
    <Layout>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">My Tasks</h1>
        <button
          onClick={() => navigate('/tasks/new')}
          className="px-4 py-2 rounded-md bg-blue-600 text-white text-sm font-medium hover:bg-blue-700 transition-colors"
        >
          + New Task
        </button>
      </div>

      {error && (
        <div className="mb-5 rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {isLoading ? (
        <p className="text-gray-400 text-sm">Loading tasks…</p>
      ) : tasks.length === 0 ? (
        <div className="text-center py-20 text-gray-400">
          <p className="text-lg font-medium">No tasks yet</p>
          <p className="text-sm mt-1">Click &ldquo;New Task&rdquo; to get started.</p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {tasks.map(task => (
            <TaskCard
              key={task.id}
              task={task}
              onEdit={id => navigate(`/tasks/${id}/edit`)}
              onDelete={handleDelete}
            />
          ))}
        </div>
      )}
    </Layout>
  );
}
