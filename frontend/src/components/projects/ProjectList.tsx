import React, { useState, useEffect } from 'react';
import { projectService } from '../../services/projectService';
import { Project } from '../../types/project.types';
import ProjectCard from './ProjectCard';
import ProjectForm from './ProjectForm';
import './Project.css';

const ProjectList: React.FC = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    fetchProjects();
  }, []);

  const fetchProjects = async () => {
    try {
      setLoading(true);
      const data = await projectService.getAll();
      setProjects(data);
    } catch (err: any) {
      setError('Failed to load projects');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateProject = async (title: string, description?: string) => {
    try {
      const newProject = await projectService.create({ title, description });
      setProjects([...projects, newProject]);
      setShowForm(false);
    } catch (err: any) {
      setError('Failed to create project');
    }
  };

  const handleDeleteProject = async (id: string) => {
    try {
      await projectService.delete(id);
      setProjects(projects.filter(p => p.id !== id));
    } catch (err: any) {
      setError('Failed to delete project');
    }
  };

  if (loading) {
    return <div className="loading">Loading projects...</div>;
  }

  return (
    <div className="project-list-container">
      <div className="project-list-header">
        <h2>My Projects</h2>
        <button 
          onClick={() => setShowForm(!showForm)}
          className="btn-primary"
        >
          {showForm ? 'Cancel' : 'New Project'}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showForm && (
        <ProjectForm 
          onSubmit={handleCreateProject}
          onCancel={() => setShowForm(false)}
        />
      )}

      <div className="project-grid">
        {projects.length === 0 ? (
          <p className="no-projects">
            No projects yet. Create your first project!
          </p>
        ) : (
          projects.map(project => (
            <ProjectCard
              key={project.id}
              project={project}
              onDelete={handleDeleteProject}
            />
          ))
        )}
      </div>
    </div>
  );
};

export default ProjectList;
