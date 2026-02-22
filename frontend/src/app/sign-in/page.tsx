'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import { GoogleLoginButton } from '@/components/GoogleLoginButton';

export default function SignInPage() {
  const { user, loading } = useAuth();
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (user && !loading) {
      router.push('/');
    }
  }, [user, loading, router]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-muted">Loading...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-bg">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <h1 className="text-accent font-bold text-2xl tracking-tight">EXECUTION OS</h1>
          <p className="text-muted text-sm mt-2">Ship or shut up.</p>
        </div>

        <div className="card text-center">
          <h2 className="text-lg font-bold text-white mb-2">Sign In</h2>
          <p className="text-muted text-sm mb-6">
            One goal. Two projects. No excuses.
          </p>

          {error && (
            <p className="text-danger text-sm mb-4">{error}</p>
          )}

          <GoogleLoginButton
            onSuccess={() => router.push('/')}
            onError={() => setError('Login failed. Please try again.')}
          />

        </div>

        <p className="text-center text-muted text-xs mt-6">
          Your data stays yours. We only use Google for authentication.
        </p>
      </div>
    </div>
  );
}
