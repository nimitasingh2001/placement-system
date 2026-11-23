import { Navigate, Route, Routes } from 'react-router';
import { useAuth } from './auth/context';
import AppLayout from './components/AppLayout';
import Placeholder from './components/Placeholder';
import RequireRole from './components/RequireRole';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';

// "/" sends each person to their own home screen
function RoleHome() {
  const { session } = useAuth();
  if (!session) return <Navigate to="/login" replace />;
  return <Navigate to={session.role === 'Admin' ? '/admin' : '/jobs'} replace />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/" element={<RoleHome />} />

      <Route element={<RequireRole role="Student" />}>
        <Route element={<AppLayout />}>
          <Route path="jobs" element={<Placeholder title="Jobs for you" />} />
          <Route path="applications" element={<Placeholder title="Where things stand" />} />
          <Route path="profile" element={<Placeholder title="Your details" />} />
        </Route>
      </Route>

      <Route element={<RequireRole role="Admin" />}>
        <Route element={<AppLayout />}>
          <Route path="admin" element={<Placeholder title="Overview" />} />
          <Route path="admin/manage" element={<Placeholder title="Manage" />} />
        </Route>
      </Route>

      <Route path="*" element={<Placeholder title="We couldn't find that page" />} />
    </Routes>
  );
}