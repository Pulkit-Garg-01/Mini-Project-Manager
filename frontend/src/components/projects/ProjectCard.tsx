import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Project } from '../../types/project.types';
import './Project.css';
import { Trash2Icon } from 'lucide-react';

interface ProjectCardProps {
  project: Project;
  onDelete: (id: string) => void;
}

const ProjectCard: React.FC<ProjectCardProps> = ({ project, onDelete }) => {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate(`/projects/${project.id}`);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (window.confirm('Are you sure you want to delete this project?')) {
      onDelete(project.id);
    }
  };

  return (
    <div className="project-card" onClick={handleClick}>
      <div className="project-card-header">
        <h3>{project.title}</h3>
        <button 
          onClick={handleDelete}
          className="btn-delete"
          title="Delete project"
        >
          <Trash2Icon size={20} />
        </button>
      </div>
      
      {project.description && (
        <p className="project-description">{project.description}</p>
      )}
      
      <div className="project-card-footer">
        {project.taskCount !== undefined && (
          <span className="project-tasks">
            Tasks: {project.completedTaskCount}/{project.taskCount}
          </span>
        )}
        <span className="project-date">
          Created: {new Date(project.createdAt).toLocaleDateString()}
        </span>
      </div>
    </div>
  );
};

export default ProjectCard;
