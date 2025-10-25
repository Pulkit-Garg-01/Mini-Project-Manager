import apiClient from './api';
import { ScheduleRequest, ScheduleResponse } from '../types/scheduler.types';

export const schedulerService = {
  async generateSchedule(
    projectId: string, 
    request: ScheduleRequest
  ): Promise<ScheduleResponse> {
    const response = await apiClient.post<any>(
      `/projects/${projectId}/scheduler/schedule`,
      {
        ...request,
        priorityTaskIds: request.priorityTaskIds?.map(id => Number(id))
      }
    );
    
    const data = response.data;
    
    return {
      projectId: String(data.projectId),
      projectTitle: data.projectTitle,
      startDate: data.startDate,
      endDate: data.endDate,
      totalTasks: data.totalTasks,
      scheduledTasks: data.scheduledTasks,
      schedule: data.schedule.map((s: any) => ({
        taskId: String(s.taskId),
        title: s.title,
        scheduledDate: s.scheduledDate,
        estimatedHours: s.estimatedHours,
        priority: s.priority,
        reason: s.reason
      })),
      warnings: data.warnings || []
    };
  }
};
