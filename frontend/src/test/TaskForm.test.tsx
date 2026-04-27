import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { TaskForm } from '../components/TaskForm';

function renderForm(onSubmit = vi.fn(), onCancel = vi.fn()) {
  render(
    <TaskForm
      isLoading={false}
      error={null}
      submitLabel="Save"
      onSubmit={onSubmit}
      onCancel={onCancel}
    />
  );
}

describe('TaskForm', () => {
  it('shows a title error when submitted with an empty title', async () => {
    renderForm();
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    expect(screen.getByText('Title is required.')).toBeInTheDocument();
  });

  it('calls onSubmit with trimmed title when form is valid', async () => {
    const onSubmit = vi.fn();
    renderForm(onSubmit);
    await userEvent.type(screen.getByLabelText(/title/i), '  Buy milk  ');
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'Buy milk' })
    );
  });

  it('calls onCancel when Cancel is clicked', async () => {
    const onCancel = vi.fn();
    renderForm(vi.fn(), onCancel);
    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(onCancel).toHaveBeenCalledOnce();
  });
});
