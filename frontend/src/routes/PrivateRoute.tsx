import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

interface PrivateRouteProps {
  redirectTo?: string;
}

export const PrivateRoute: React.FC<PrivateRouteProps> = ({ 
  redirectTo = '/login' 
}) => {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? <Outlet /> : <Navigate to={redirectTo} replace />;
};

