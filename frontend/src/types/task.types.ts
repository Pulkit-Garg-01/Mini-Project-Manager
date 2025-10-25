export interface Task {
  id: string;
  title: string;
  dueDate?: string;
  isCompleted: boolean;
  projectId: string;
  createdAt: string;
}

export interface CreateTaskDto {
  title: string;
  dueDate?: string;
  projectId: number;  // Backend expects number
}

export interface UpdateTaskDto {
  title: string;
  dueDate?: string;
  isCompleted: boolean;
}