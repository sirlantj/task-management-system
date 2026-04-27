import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { tasksApi, ApiError } from '../api/client';
import { Layout } from '../components/Layout';
import { TaskForm, type TaskFormValues } from '../components/TaskForm';

export default function CreateTaskPage() {
  const navigate = useNavigate();

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async ({ title, description, dueDate }: TaskFormValues) => {
    setError('');
    setIsLoading(true);
    try {
      await tasksApi.create({
        title,
        description: description.trim() || null,
        dueDate: dueDate ? `${dueDate}T00:00:00.000Z` : null,
      });
      navigate('/tasks');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to create task.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Layout>
      <div className="max-w-xl">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">New Task</h1>
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <TaskForm
            isLoading={isLoading}
            error={error}
            submitLabel="Create Task"
            onSubmit={handleSubmit}
            onCancel={() => navigate('/tasks')}
          />
        </div>
      </div>
    </Layout>
  );
}
