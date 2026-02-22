'use client';

import { createContext, useContext, useEffect, useState, useCallback } from 'react';
import type { AuthUser } from '@/types/api';
import { logger, safeId } from '@/lib/logger';

interface AuthContextType {
  user: AuthUser | null;
  loading: boolean;
  login: (idToken: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  loading: true,
  login: async () => { },
  logout: () => { },
});

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem('auth');
    if (stored) {
      try {
        const parsed = JSON.parse(stored) as AuthUser;
        setUser(parsed);
        logger.debug('Auth', 'Session restored from storage', { userId: safeId(parsed.userId) });
      } catch {
        localStorage.removeItem('auth');
        logger.warn('Auth', 'Corrupt auth data in storage — cleared');
      }
    }
    setLoading(false);
  }, []);

  const login = useCallback(async (idToken: string) => {
    const res = await fetch('/api/auth/google', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ idToken }),
    });

    if (!res.ok) {
      const body = await res.json().catch(() => ({}));
      logger.error('Auth', 'Login failed', { status: res.status });
      throw new Error(body.error || 'Login failed');
    }

    const data: AuthUser = await res.json();
    setUser(data);
    localStorage.setItem('auth', JSON.stringify(data));
    logger.info('Auth', 'User logged in', { userId: safeId(data.userId) });
  }, []);

  const logout = useCallback(() => {
    logger.info('Auth', 'User logged out');
    setUser(null);
    localStorage.removeItem('auth');
  }, []);

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
