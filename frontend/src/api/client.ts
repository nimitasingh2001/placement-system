const API_URL = import.meta.env.VITE_API_URL as string;

// the shape of the error JSON your API returns (ProblemDetails)
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

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body !== undefined && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (!response.ok) {
    let problem: ApiProblem = { status: response.status, title: response.statusText || 'Request failed' };
    try {
      problem = { ...problem, ...(await response.json()) };
    } catch {
      // the response had no JSON body (for example a plain 401), keep the basic problem
    }
    throw new ApiError(problem);
  }

  if (response.status === 204) return undefined as T;   // "no content"
  return (await response.json()) as T;
}

// a friendly sentence for the screen, whatever went wrong
export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) return error.message;
  return "We can't reach the server right now. Please try again in a moment.";
}