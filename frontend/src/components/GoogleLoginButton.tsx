import React from 'react';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { useAuth } from '@/lib/auth';

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
            console.error('Login Failed', error);
            if (onError) onError();
        }
    };

    return (
        <div className="flex justify-center">
            <GoogleLogin
                onSuccess={handleSuccess}
                onError={() => {
                    console.log('Login Failed');
                    if (onError) onError();
                }}
                useOneTap
                theme="filled_black"
                shape="pill"
            />
        </div>
    );
};
