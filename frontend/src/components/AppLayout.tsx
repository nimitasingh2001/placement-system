import { useEffect, useState } from 'react';
import { NavLink, Outlet } from 'react-router';
import { api } from '../api/client';
import type { StudentProfile } from '../api/types';
import { useAuth } from '../auth/context';

const studentLinks = [
  { to: '/jobs', label: 'Jobs for you' },
  { to: '/applications', label: 'My applications' },
  { to: '/profile', label: 'My profile' },
];

const adminLinks = [
  { to: '/admin', label: 'Overview', end: true },
  { to: '/admin/manage', label: 'Manage', end: false },
];

export default function AppLayout() {
  const { session, logout } = useAuth();
  const [studentName, setStudentName] = useState('');

  useEffect(() => {
    if (session?.role !== 'Student') return;
    api<StudentProfile>('/api/me/profile')
      .then((p) => setStudentName(`${p.firstName} ${p.lastName ?? ''}`.trim()))
      .catch(() => setStudentName(''));
  }, [session]);

  const isAdmin = session?.role === 'Admin';
  const links = isAdmin ? adminLinks : studentLinks;

  return (
    <>
      <nav className="top" aria-label="Main">
        <span className="brand">Placement Hub</span>
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end={'end' in link ? !!link.end : false}
            className={({ isActive }) => (isActive ? 'on' : '')}
          >
            {link.label}
          </NavLink>
        ))}
        <span className="sp" />
        <span className="mu">{isAdmin ? 'Placement Office' : studentName}</span>
        <button className="btn" onClick={logout}>Log out</button>
      </nav>
      <main>
        <Outlet />
      </main>
    </>
  );
}