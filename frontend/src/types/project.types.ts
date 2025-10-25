import { Task } from './task.types';
export interface Project {
id: string;
title: string;
description?: string;
createdAt: string;
taskCount?: number;
completedTaskCount?: number;
}
export interface ProjectDetail {
  id: string;
  title: string;
  description?: string;
  createdAt: string;
  tasks: Task[];
}
export interface CreateProjectDto {
title: string;
description?: string;
}
export interface UpdateProjectDto {
title?: string;
description?: string;
}