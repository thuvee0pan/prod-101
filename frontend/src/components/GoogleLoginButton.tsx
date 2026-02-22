import React from 'react';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { useAuth } from '@/lib/auth';
import { logger } from '@/lib/logger';

interface GoogleLoginButtonProps {
    onSuccess?: () => void;
    onError?: () => void;
}

export const GoogleLoginButton: React.FC<GoogleLoginButtonProps> = ({ onSuccess, onError }) => {
    const { login } = useAuth();

    const handleSuccess = async (credentialResponse: CredentialResponse) => {
        try {
            if (credentialResponse.credential) {
                await login(credentialResponse.credential);
                if (onSuccess) onSuccess();
            }
        } catch (error) {
            logger.error('GoogleLogin', 'Login callback failed', {
                message: error instanceof Error ? error.message : 'Unknown error',
            });
            if (onError) onError();
        }
    };

    return (
        <div className="flex justify-center">
            <GoogleLogin
                onSuccess={handleSuccess}
                onError={() => {
                    logger.error('GoogleLogin', 'Google OAuth popup failed');
                    if (onError) onError();
                }}
                useOneTap
                theme="filled_black"
                shape="pill"
            />
        </div>
    );
};
