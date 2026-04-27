import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { tasksApi } from '../api/client';
import { getErrorMessage } from '../utils/errors';
import type { Task } from '../types/task';
import { Layout } from '../components/Layout';
import { TaskForm, type TaskFormValues } from '../components/TaskForm';

export default function EditTaskPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [task, setTask] = useState<Task | null>(null);
  const [isFetching, setIsFetching] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!id) return;
    tasksApi
      .getById(id)
      .then(setTask)
      .catch(err => setError(getErrorMessage(err, 'Task not found.')))
      .finally(() => setIsFetching(false));
  }, [id]);

  const handleSubmit = async ({ title, description, dueDate, status }: TaskFormValues) => {
    if (!id || !task) return;
    setError('');
    setIsLoading(true);
    try {
      await tasksApi.update(id, {
        title,
        description: description.trim() || null,
        dueDate: dueDate ? `${dueDate}T00:00:00.000Z` : null,
        status: status !== task.status ? status : undefined,
      });
      navigate('/tasks');
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to update task.'));
    } finally {
      setIsLoading(false);
    }
  };

  if (isFetching) {
    return (
      <Layout>
        <p className="text-gray-400 text-sm">Loading…</p>
      </Layout>
    );
  }

  if (!task) {
    return (
      <Layout>
        <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">
          {error || 'Task not found.'}
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-xl">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Edit Task</h1>
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <TaskForm
            initialValues={{
              title: task.title,
              description: task.description ?? '',
              dueDate: task.dueDate ?? '',
              status: task.status,
            }}
            showStatus
            currentStatus={task.status}
            isLoading={isLoading}
            error={error}
            submitLabel="Save Changes"
            onSubmit={handleSubmit}
            onCancel={() => navigate('/tasks')}
          />
        </div>
      </div>
    </Layout>
  );
}
