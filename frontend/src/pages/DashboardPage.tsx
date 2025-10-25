import React from 'react';
import ProjectList from '../components/projects/ProjectList';
import './Pages.css';

const DashboardPage: React.FC = () => {
  return (
    <div className="page-container">
      <ProjectList />
    </div>
  );
};

export default DashboardPage;
