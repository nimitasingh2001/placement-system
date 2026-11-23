import { createContext, useContext } from 'react';
import type { Session } from './session';

export type AuthContextValue = {
  session: Session | null;
  login: (email: string, password: string) => Promise<void>;
  register: (rollNo: string, email: string, password: string) => Promise<void>;
  logout: () => void;
};

export const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth must be used inside <AuthProvider>');
  return value;
}