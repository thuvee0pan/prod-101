/**
 * Structured Logger Utility
 *
 * Centralizes frontend logging with level-based output.
 * In production, debug logs are suppressed and sensitive data is redacted.
 *
 * NEVER log: tokens, emails, passwords, full names, profile URLs, request/response bodies.
 * Safe to log: HTTP method, path, status codes, durations, entity IDs (truncated).
 */

type LogLevel = 'debug' | 'info' | 'warn' | 'error';

const isProd = process.env.NODE_ENV === 'production';

function formatMessage(level: LogLevel, context: string, message: string): string {
    const timestamp = new Date().toISOString();
    return `[${timestamp}] [${level.toUpperCase()}] [${context}] ${message}`;
}

function log(level: LogLevel, context: string, message: string, data?: Record<string, unknown>) {
    // Suppress debug in production
    if (level === 'debug' && isProd) return;

    const formatted = formatMessage(level, context, message);

    const logFn =
        level === 'error' ? console.error :
            level === 'warn' ? console.warn :
                level === 'debug' ? console.debug :
                    console.info;

    if (data && Object.keys(data).length > 0) {
        logFn(formatted, data);
    } else {
        logFn(formatted);
    }
}

/** Truncate an ID for safe logging (first 8 chars) */
export function safeId(id: string): string {
    return id.length > 8 ? id.substring(0, 8) + '…' : id;
}

export const logger = {
    debug: (context: string, message: string, data?: Record<string, unknown>) =>
        log('debug', context, message, data),

    info: (context: string, message: string, data?: Record<string, unknown>) =>
        log('info', context, message, data),

    warn: (context: string, message: string, data?: Record<string, unknown>) =>
        log('warn', context, message, data),

    error: (context: string, message: string, data?: Record<string, unknown>) =>
        log('error', context, message, data),
};
