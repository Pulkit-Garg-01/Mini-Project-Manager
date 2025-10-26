import React from 'react';
import { Task } from '../../types/task.types';
import './Task.css';
import { Trash2Icon } from 'lucide-react';

interface TaskItemProps {
  task: Task;
  onToggle: (id: string) => void;
  onDelete: (id: string) => void;
}

const TaskItem: React.FC<TaskItemProps> = ({ task, onToggle, onDelete }) => {
  const handleDelete = () => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      onDelete(task.id);
    }
  };

  return (
    <div className={`task-item ${task.isCompleted ? 'completed' : ''}`}>
      <input
        type="checkbox"
        checked={task.isCompleted}
        onChange={() => onToggle(task.id)}
        className="task-checkbox"
      />
      
      <div className="task-content">
        <span className="task-title">{task.title}</span>
        {task.dueDate && (
          <span className="task-due-date">
            Due: {new Date(task.dueDate).toLocaleDateString()}
          </span>
        )}
      </div>
      
      <button 
        onClick={handleDelete}
        className="task-delete-btn"
        title="Delete task"
      >
        <Trash2Icon size={20} />
      </button>
    </div>
  );
};

export default TaskItem;
