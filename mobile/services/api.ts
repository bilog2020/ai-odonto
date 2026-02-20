import * as SecureStore from 'expo-secure-store';

const API_URL = process.env.EXPO_PUBLIC_API_URL || 'http://localhost:8080';

async function getToken(): Promise<string | null> {
  return await SecureStore.getItemAsync('authToken');
}

async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = await getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(`API Error ${response.status}: ${error}`);
  }

  return response.json();
}

export const api = {
  auth: {
    register: (email: string, password: string, fullName: string) =>
      request<{ success: boolean; token?: string; errors?: string[] }>(
        '/api/auth/register',
        { method: 'POST', body: JSON.stringify({ email, password, fullName }) }
      ),
    login: (email: string, password: string) =>
      request<{ success: boolean; token?: string }>(
        '/api/auth/login',
        { method: 'POST', body: JSON.stringify({ email, password }) }
      ),
  },
  chat: {
    getSessions: () =>
      request<Array<{ id: number; title: string; createdAt: string }>>('/api/chat/sessions'),
    createSession: (title: string) =>
      request<{ id: number; title: string }>('/api/chat/sessions', {
        method: 'POST',
        body: JSON.stringify({ title }),
      }),
    getSession: (id: number) =>
      request<{ id: number; messages: Array<{ role: string; content: string; sourceChunkIds: number[] }> }>(
        `/api/chat/sessions/${id}`
      ),
    sendMessage: (sessionId: number, message: string) =>
      request<{ content: string; sourceChunkIds: number[] }>(
        `/api/chat/sessions/${sessionId}/messages`,
        { method: 'POST', body: JSON.stringify({ message }) }
      ),
    deleteSession: (id: number) =>
      request<void>(`/api/chat/sessions/${id}`, { method: 'DELETE' }),
  },
  documents: {
    getAll: () =>
      request<Array<{ id: number; title: string; status: string }>>('/api/documents'),
    getById: (id: number) =>
      request<{ id: number; title: string; chunks: Array<{ content: string }> }>(
        `/api/documents/${id}`
      ),
  },
};
