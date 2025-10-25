import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { projectService } from '../services/projectService';
import { ProjectDetail } from '../types/project.types';
import TaskList from '../components/tasks/TaskList';
import './Pages.css';

const ProjectDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [project, setProject] = useState<ProjectDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    if (id) {
      fetchProject(id);
    }
  }, [id]);

  const fetchProject = async (projectId: string) => {
    try {
      setLoading(true);
      const data = await projectService.getById(projectId);
      setProject(data);
    } catch (err: any) {
      setError('Failed to load project');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="loading">Loading project...</div>;
  }

  if (error || !project) {
    return (
      <div className="error-container">
        <p className="error-message">{error || 'Project not found'}</p>
        <button onClick={() => navigate('/dashboard')} className="btn-primary">
          Back to Dashboard
        </button>
      </div>
    );
  }

  return (
    <div className="page-container">
      <div className="project-details-header">
        <button onClick={() => navigate('/dashboard')} className="btn-back">
          ← Back
        </button>
        <h1>{project.title}</h1>
        {project.description && <p className="project-description">{project.description}</p>}
        <p className="project-date">
          Created: {new Date(project.createdAt).toLocaleDateString()}
        </p>
      </div>

      <TaskList projectId={project.id} />
    </div>
  );
};

export default ProjectDetailsPage;
