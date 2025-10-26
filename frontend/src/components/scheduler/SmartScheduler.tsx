import React, { useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { schedulerService } from '../../services/schedulerService';
import { ScheduleResponse } from '../../types/scheduler.types';
import { Task } from '../../types/task.types';
import './SmartScheduler.css';

interface SmartSchedulerProps {
  projectId: string;
  projectTitle: string;
  tasks: Task[];
  onClose: () => void;
}

const ScheduleSchema = Yup.object().shape({
  startDate: Yup.date()
    .required('Start date is required')
    .min(new Date(), 'Start date must be in the future'),
  endDate: Yup.date()
    .required('End date is required')
    .min(Yup.ref('startDate'), 'End date must be after start date'),
  dailyWorkHours: Yup.number()
    .required('Daily work hours is required')
    .min(1, 'Minimum 1 hour')
    .max(24, 'Maximum 24 hours'),
});

const SmartScheduler: React.FC<SmartSchedulerProps> = ({ 
  projectId, 
  projectTitle,
  tasks,
  onClose 
}) => {
  const [schedule, setSchedule] = useState<ScheduleResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [selectedPriorityTasks, setSelectedPriorityTasks] = useState<string[]>([]);

  const incompleteTasks = tasks.filter(t => !t.isCompleted);

  const formik = useFormik({
    initialValues: {
      startDate: new Date().toISOString().split('T')[0],
      endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      dailyWorkHours: 8,
    },
    validationSchema: ScheduleSchema,
    onSubmit: async (values) => {
      try {
        setLoading(true);
        setError('');
        
        const response = await schedulerService.generateSchedule(projectId, {
          startDate: values.startDate,
          endDate: values.endDate,
          dailyWorkHours: values.dailyWorkHours,
          priorityTaskIds: selectedPriorityTasks.map(id => Number(id))
        });
        
        setSchedule(response);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to generate schedule');
      } finally {
        setLoading(false);
      }
    },
  });

  const togglePriorityTask = (taskId: string) => {
    setSelectedPriorityTasks(prev =>
      prev.includes(taskId)
        ? prev.filter(id => id !== taskId)
        : [...prev, taskId]
    );
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'High': return '#ef4444';
      case 'Medium': return '#f59e0b';
      case 'Low': return '#10b981';
      default: return '#6b7280';
    }
  };

  return (
    <div className="scheduler-modal">
      <div className="scheduler-content">
        <div className="scheduler-header">
          <h2>Smart Scheduler</h2>
          <button onClick={onClose} className="close-btn">×</button>
        </div>

        {!schedule ? (
          <>
            <p className="scheduler-subtitle">
              Let AI help you plan your work for <strong>{projectTitle}</strong>
            </p>

            {error && <div className="error-message">{error}</div>}

            <form onSubmit={formik.handleSubmit} className="scheduler-form">
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="startDate">Start Date</label>
                  <input
                    id="startDate"
                    type="date"
                    {...formik.getFieldProps('startDate')}
                    className={formik.touched.startDate && formik.errors.startDate ? 'input-error' : ''}
                  />
                  {formik.touched.startDate && formik.errors.startDate && (
                    <div className="field-error">{formik.errors.startDate}</div>
                  )}
                </div>

                <div className="form-group">
                  <label htmlFor="endDate">End Date</label>
                  <input
                    id="endDate"
                    type="date"
                    {...formik.getFieldProps('endDate')}
                    className={formik.touched.endDate && formik.errors.endDate ? 'input-error' : ''}
                  />
                  {formik.touched.endDate && formik.errors.endDate && (
                    <div className="field-error">{formik.errors.endDate}</div>
                  )}
                </div>

                <div className="form-group">
                  <label htmlFor="dailyWorkHours">Daily Work Hours</label>
                  <input
                    id="dailyWorkHours"
                    type="number"
                    min="1"
                    max="24"
                    {...formik.getFieldProps('dailyWorkHours')}
                    className={formik.touched.dailyWorkHours && formik.errors.dailyWorkHours ? 'input-error' : ''}
                  />
                  {formik.touched.dailyWorkHours && formik.errors.dailyWorkHours && (
                    <div className="field-error">{formik.errors.dailyWorkHours}</div>
                  )}
                </div>
              </div>

              {incompleteTasks.length > 0 && (
                <div className="priority-section">
                  <label>Priority Tasks (optional)</label>
                  <p className="help-text">Select tasks that should be scheduled first</p>
                  <div className="task-checkboxes">
                    {incompleteTasks.map(task => (
                      <label key={task.id} className="task-checkbox">
                        <input
                          type="checkbox"
                          checked={selectedPriorityTasks.includes(task.id)}
                          onChange={() => togglePriorityTask(task.id)}
                        />
                        <span>{task.title}</span>
                        {task.dueDate && (
                          <span className="task-due-date">
                            Due: {new Date(task.dueDate).toLocaleDateString()}
                          </span>
                        )}
                      </label>
                    ))}
                  </div>
                </div>
              )}

              <div className="form-actions">
                <button type="button" onClick={onClose} className="btn-secondary">
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={loading || incompleteTasks.length === 0}
                  className="btn-primary"
                >
                  {loading ? '🔄 Generating Schedule...' : 'Generate Schedule'}
                </button>
              </div>
            </form>
          </>
        ) : (
          <div className="schedule-result">
            <div className="result-header">
              <h3>📅 Your Smart Schedule</h3>
              <p className="result-summary">
                {schedule.scheduledTasks} of {schedule.totalTasks} tasks scheduled
              </p>
            </div>

            {schedule.warnings.length > 0 && (
              <div className="warnings">
                {schedule.warnings.map((warning, index) => (
                  <div key={index} className="warning-item">
                    ⚠️ {warning}
                  </div>
                ))}
              </div>
            )}

            <div className="schedule-timeline">
              {schedule.schedule.map((item, index) => (
                <div key={index} className="schedule-item">
                  <div className="schedule-date">
                    <div className="date-circle">{new Date(item.scheduledDate).getDate()}</div>
                    <div className="date-month">
                      {new Date(item.scheduledDate).toLocaleDateString('en-US', { month: 'short' })}
                    </div>
                  </div>
                  <div className="schedule-details">
                    <div className="schedule-task-header">
                      <h4>{item.title}</h4>
                      <span 
                        className="schedule-priority"
                        style={{ backgroundColor: getPriorityColor(item.priority) }}
                      >
                        {item.priority}
                      </span>
                    </div>
                    <p className="schedule-hours">⏱️ {item.estimatedHours} hours</p>
                    <p className="schedule-reason">{item.reason}</p>
                  </div>
                </div>
              ))}
            </div>

            <div className="form-actions">
              <button onClick={() => setSchedule(null)} className="btn-secondary">
                ← Back
              </button>
              <button onClick={onClose} className="btn-primary">
                Done
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default SmartScheduler;
