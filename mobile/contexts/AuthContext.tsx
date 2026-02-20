import React, { createContext, useContext, useState, useEffect } from 'react';
import * as SecureStore from 'expo-secure-store';
import { api } from '../services/api';

interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, fullName: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkToken();
  }, []);

  async function checkToken() {
    const token = await SecureStore.getItemAsync('authToken');
    setIsAuthenticated(!!token);
    setIsLoading(false);
  }

  async function login(email: string, password: string) {
    const result = await api.auth.login(email, password);
    if (result.success && result.token) {
      await SecureStore.setItemAsync('authToken', result.token);
      setIsAuthenticated(true);
    } else {
      throw new Error('Login failed');
    }
  }

  async function register(email: string, password: string, fullName: string) {
    const result = await api.auth.register(email, password, fullName);
    if (result.success && result.token) {
      await SecureStore.setItemAsync('authToken', result.token);
      setIsAuthenticated(true);
    } else {
      throw new Error(result.errors?.join(', ') || 'Registration failed');
    }
  }

  async function logout() {
    await SecureStore.deleteItemAsync('authToken');
    setIsAuthenticated(false);
  }

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
