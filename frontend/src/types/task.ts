export type TaskStatus = 'Pending' | 'InProgress' | 'Done';

export interface Task {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  dueDate: string | null;
  userId: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTaskPayload {
  title: string;
  description: string | null;
  dueDate: string | null;
}

export interface UpdateTaskPayload {
  title: string;
  description: string | null;
  dueDate: string | null;
  status?: TaskStatus;
}
