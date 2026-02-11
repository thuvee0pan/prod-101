import type { Metadata } from 'next';
import './globals.css';
import { Sidebar } from '@/components/Sidebar';

export const metadata: Metadata = {
  title: 'Execution OS',
  description: 'Personal execution operating system. One goal. Two projects. No excuses.',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="font-mono">
        <div className="flex min-h-screen">
          <Sidebar />
          <main className="flex-1 p-6 ml-56">{children}</main>
        </div>
      </body>
    </html>
  );
}
