import type { AuthResponse } from "../api/types";

export type Session = AuthResponse;

const KEY = 'placement.session';

// "2026-11-23T04:12:42.3432088Z" has more digits than every browser accepts, so trim to milliseconds
function expiryTime(session: Session): number {
  return Date.parse(session.expiresAtUtc.replace(/(\.\d{3})\d+/, '$1'));
}

export function expiresInMs(session: Session): number {
  return expiryTime(session) - Date.now();
}

export function loadSession(): Session | null {
  try {
    const raw = localStorage.getItem(KEY);
    if (!raw) return null;
    const session = JSON.parse(raw) as Session;
    if (expiresInMs(session) <= 0) {
      localStorage.removeItem(KEY);   // expired while the browser was closed
      return null;
    }
    return session;
  } catch {
    return null;
  }
}

export function saveSession(session: Session): void {
  localStorage.setItem(KEY, JSON.stringify(session));
}

export function clearSession(): void {
  localStorage.removeItem(KEY);
}

export function getToken(): string | null {
  return loadSession()?.token ?? null;
}