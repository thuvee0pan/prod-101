'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/lib/auth';

const nav = [
  { href: '/', label: 'Dashboard' },
  { href: '/goals', label: 'Goal' },
  { href: '/projects', label: 'Projects' },
  { href: '/todos', label: 'Todos' },
  { href: '/daily-log', label: 'Daily Log' },
  { href: '/weekly-review', label: 'Weekly Review' },
];

export function Sidebar() {
  const pathname = usePathname();
  const { user, logout } = useAuth();

  return (
    <aside className="fixed left-0 top-0 h-screen w-56 bg-surface border-r border-border flex flex-col">
      <div className="p-5 border-b border-border">
        <h1 className="text-accent font-bold text-lg tracking-tight">EXECUTION OS</h1>
        <p className="text-muted text-xs mt-1">Ship or shut up.</p>
      </div>
      <nav className="flex-1 p-3">
        {nav.map((item) => {
          const active = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`block px-3 py-2 rounded-md text-sm mb-1 transition-colors ${
                active
                  ? 'bg-accent/10 text-accent border-l-2 border-accent'
                  : 'text-muted hover:text-white hover:bg-border/50'
              }`}
            >
              {item.label}
            </Link>
          );
        })}
      </nav>
      <div className="p-4 border-t border-border">
        {user && (
          <div className="flex items-center gap-2 mb-3">
            {user.profilePicture ? (
              <img
                src={user.profilePicture}
                alt={user.name}
                className="w-7 h-7 rounded-full"
                referrerPolicy="no-referrer"
              />
            ) : (
              <div className="w-7 h-7 rounded-full bg-accent/20 flex items-center justify-center text-xs text-accent font-bold">
                {user.name.charAt(0).toUpperCase()}
              </div>
            )}
            <div className="flex-1 min-w-0">
              <p className="text-xs text-white truncate">{user.name}</p>
              <p className="text-[10px] text-muted truncate">{user.email}</p>
            </div>
          </div>
        )}
        <button
          onClick={logout}
          className="text-xs text-muted hover:text-danger transition-colors w-full text-left"
        >
          Sign out
        </button>
        <p className="text-xs text-muted mt-2">v0.1.0 MVP</p>
      </div>
    </aside>
  );
}
