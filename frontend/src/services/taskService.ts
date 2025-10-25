import apiClient from './api';
import { Task, CreateTaskDto, UpdateTaskDto } from '../types/task.types';

export const taskService = {
  async getByProjectId(projectId: string): Promise<Task[]> {
    // Backend endpoint is /api/tasks/project/{projectId}
    const response = await apiClient.get<any[]>(`/tasks/project/${projectId}`);
    
    return response.data.map(t => ({
      id: String(t.id),
      title: t.title,
      dueDate: t.dueDate,
      isCompleted: t.isCompleted,
      projectId: String(t.projectId),
      createdAt: t.createdAt
    }));
  },

  async create(data: CreateTaskDto): Promise<Task> {
    // Convert projectId to number for backend
    const createData = {
      ...data,
      projectId: Number(data.projectId)
    };
    
    const response = await apiClient.post<any>('/tasks', createData);
    const task = response.data;
    
    return {
      id: String(task.id),
      title: task.title,
      dueDate: task.dueDate,
      isCompleted: task.isCompleted,
      projectId: String(task.projectId),
      createdAt: task.createdAt
    };
  },

  async update(id: string, data: UpdateTaskDto): Promise<Task> {
    const response = await apiClient.put<any>(`/tasks/${id}`, data);
    const task = response.data;
    
    return {
      id: String(task.id),
      title: task.title,
      dueDate: task.dueDate,
      isCompleted: task.isCompleted,
      projectId: String(task.projectId),
      createdAt: task.createdAt
    };
  },

  async toggleComplete(id: string): Promise<Task> {
    const response = await apiClient.patch<any>(`/tasks/${id}/toggle`);
    const task = response.data;
    
    return {
      id: String(task.id),
      title: task.title,
      dueDate: task.dueDate,
      isCompleted: task.isCompleted,
      projectId: String(task.projectId),
      createdAt: task.createdAt
    };
  },

  async delete(id: string): Promise<void> {
    await apiClient.delete(`/tasks/${id}`);
  }
};
