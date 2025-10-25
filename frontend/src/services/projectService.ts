import apiClient from './api';
import { Project, ProjectDetail, CreateProjectDto, UpdateProjectDto } from '../types/project.types';

export const projectService = {
  async getAll(): Promise<Project[]> {
    const response = await apiClient.get<any[]>('/projects');
    // Transform backend ProjectDto to frontend Project
    return response.data.map(p => ({
      id: String(p.id),
      title: p.title,
      description: p.description,
      createdAt: p.createdAt,
      taskCount: p.taskCount,
      completedTaskCount: p.completedTaskCount
    }));
  },

  async getById(id: string): Promise<ProjectDetail> {
    const response = await apiClient.get<any>(`/projects/${id}`);
    const data = response.data;
    
    // Transform backend ProjectDetailDto to frontend ProjectDetail
    return {
      id: String(data.id),
      title: data.title,
      description: data.description,
      createdAt: data.createdAt,
      tasks: data.tasks.map((t: any) => ({
        id: String(t.id),
        title: t.title,
        dueDate: t.dueDate,
        isCompleted: t.isCompleted,
        projectId: String(t.projectId),
        createdAt: t.createdAt
      }))
    };
  },

  async create(data: CreateProjectDto): Promise<Project> {
    const response = await apiClient.post<any>('/projects', data);
    const project = response.data;
    
    return {
      id: String(project.id),
      title: project.title,
      description: project.description,
      createdAt: project.createdAt,
      taskCount: 0,
      completedTaskCount: 0
    };
  },

  async update(id: string, data: UpdateProjectDto): Promise<Project> {
    const response = await apiClient.put<any>(`/projects/${id}`, data);
    const project = response.data;
    
    return {
      id: String(project.id),
      title: project.title,
      description: project.description,
      createdAt: project.createdAt
    };
  },

  async delete(id: string): Promise<void> {
    await apiClient.delete(`/projects/${id}`);
  }
};
