import { getToken } from '../auth/session';

const API_URL = import.meta.env.VITE_API_URL as string | undefined;

if (!API_URL) {
  throw new Error('VITE_API_URL is not set. Create frontend/.env.development and restart "npm run dev".');
}

export type ApiProblem = {
  status: number;
  title: string;
  detail?: string;
  errorCode?: number;
  errors?: Record<string, string[]>;   // present for validation errors
};

export class ApiError extends Error {
  problem: ApiProblem;

  constructor(problem: ApiProblem) {
    super(problem.detail ?? problem.title);
    this.problem = problem;
  }
}

// the auth provider registers a function here, to sign out when a token stops working
let unauthorizedHandler: (() => void) | null = null;
export function setUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler;
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body !== undefined && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const token = getToken();
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const response = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (!response.ok) {
    // a 401 while we were sending a token means it expired or was rejected: sign out.
    // (a wrong password on the login form also gives 401, but no token was sent then)
    if (response.status === 401 && token) unauthorizedHandler?.();

    let problem: ApiProblem = { status: response.status, title: response.statusText || 'Request failed' };
    try {
      problem = { ...problem, ...(await response.json()) };
    } catch {
      // no JSON body (for example a plain 401 or 429), keep the basic problem
    }
    throw new ApiError(problem);
  }

  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

// one friendly sentence for the screen, whatever went wrong
export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    const { status, errors } = error.problem;
    if (status === 429) return 'Too many attempts. Please wait a minute and try again.';
    if (errors) {
      const first = Object.values(errors)[0]?.[0];   // the first validation message
      if (first) return first;
    }
    return error.message;
  }
  console.error(error);   // the real reason, for you; users only see the sentence below
  return "We can't reach the server right now. Please try again in a moment.";
}