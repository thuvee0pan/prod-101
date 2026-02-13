import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        bg: '#0a0a0a',
        surface: '#141414',
        border: '#262626',
        accent: '#22c55e',
        'accent-dim': '#16a34a',
        warning: '#f59e0b',
        danger: '#ef4444',
        muted: '#737373',
      },
    },
  },
  plugins: [],
};
export default config;
