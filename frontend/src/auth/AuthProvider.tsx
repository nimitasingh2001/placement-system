import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api, setUnauthorizedHandler } from '../api/client';
import type { AuthResponse } from '../api/types';
import { AuthContext } from './context';
import { clearSession, expiresInMs, loadSession, saveSession, type Session } from './session';

export default function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => loadSession());

  const logout = useCallback(() => {
    clearSession();
    setSession(null);   // the route guards notice this and send the user to the login page
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const result = await api<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    saveSession(result);
    setSession(result);
  }, []);

  const register = useCallback(
    async (rollNo: string, email: string, password: string) => {
      await api('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({ rollNo, email, password }),
      });
      await login(email, password);   // the new account is signed in straight away
    },
    [login],
  );

  // let the API client sign us out when the server rejects our token
  useEffect(() => {
    setUnauthorizedHandler(logout);
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  // and sign out by ourselves at the moment the token expires
  useEffect(() => {
    if (!session) return;
    const timer = window.setTimeout(logout, Math.max(expiresInMs(session), 0));
    return () => window.clearTimeout(timer);
  }, [session, logout]);

  const value = useMemo(() => ({ session, login, register, logout }), [session, login, register, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}