import React, { useState, useEffect } from 'react';
import { taskService } from '../../services/taskService';
import { Task } from '../../types/task.types';
import TaskItem from './TaskItem';
import TaskForm from './TaskForm';
import './Task.css';

interface TaskListProps {
  projectId: string;
}

const TaskList: React.FC<TaskListProps> = ({ projectId }) => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    fetchTasks();
  }, [projectId]);

  const fetchTasks = async () => {
    try {
      setLoading(true);
      const data = await taskService.getByProjectId(projectId);
      setTasks(data);
    } catch (err: any) {
      setError('Failed to load tasks');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTask = async (title: string, dueDate?: string) => {
    try {
      const newTask = await taskService.create({ 
        title, 
        dueDate, 
        projectId: Number(projectId)  // Ensure projectId is a number
      });
      setTasks([...tasks, newTask]);
      setShowForm(false);
    } catch (err: any) {
      setError('Failed to create task');
    }
  };

  const handleToggleTask = async (id: string) => {
    try {
      const updatedTask = await taskService.toggleComplete(id);
      setTasks(tasks.map(t => t.id === id ? updatedTask : t));
    } catch (err: any) {
      setError('Failed to update task');
    }
  };

  const handleDeleteTask = async (id: string) => {
    try {
      await taskService.delete(id);
      setTasks(tasks.filter(t => t.id !== id));
    } catch (err: any) {
      setError('Failed to delete task');
    }
  };

  if (loading) {
    return <div className="loading">Loading tasks...</div>;
  }

  return (
    <div className="task-list-container">
      <div className="task-list-header">
        <h3>Tasks</h3>
        <button 
          onClick={() => setShowForm(!showForm)}
          className="btn-primary"
        >
          {showForm ? 'Cancel' : 'Add Task'}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showForm && (
        <TaskForm 
          onSubmit={handleCreateTask}
          onCancel={() => setShowForm(false)}
        />
      )}

      <div className="task-list">
        {tasks.length === 0 ? (
          <p className="no-tasks">No tasks yet. Add your first task!</p>
        ) : (
          tasks.map(task => (
            <TaskItem
              key={task.id}
              task={task}
              onToggle={handleToggleTask}
              onDelete={handleDeleteTask}
            />
          ))
        )}
      </div>
    </div>
  );
};

export default TaskList;
