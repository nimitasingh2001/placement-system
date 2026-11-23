import { Navigate, Outlet } from 'react-router';
import type { Role } from '../api/types';
import { useAuth } from '../auth/context';

export default function RequireRole({ role }: { role: Role }) {
  const { session } = useAuth();

  if (!session) return <Navigate to="/login" replace />;
  if (session.role !== role) return <Navigate to="/" replace />;   // wrong area: send them home
  return <Outlet />;   // allowed: show the page inside
}