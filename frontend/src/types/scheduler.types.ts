export interface ScheduleRequest {
  startDate: string;
  endDate: string;
  dailyWorkHours: number;
  priorityTaskIds?: number[];
}

export interface ScheduledTask {
  taskId: string;
  title: string;
  scheduledDate: string;
  estimatedHours: number;
  priority: string;
  reason: string;
}

export interface ScheduleResponse {
  projectId: string;
  projectTitle: string;
  startDate: string;
  endDate: string;
  totalTasks: number;
  scheduledTasks: number;
  schedule: ScheduledTask[];
  warnings: string[];
}
