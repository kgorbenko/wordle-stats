import * as React from 'react';
import { useApplicationContext } from '../ApplicationContext';
import { ILoginRequest, IRegisterRequest } from '../serverClient';
import {
    loginAsync as makeLoginRequestAsync,
    registerAsync as makeRegisterRequestAsync
} from '../serverClient';

export interface IAuthentication {
    readonly isAuthenticated: boolean,
    readonly registerAsync: (token: string, userName: string, password: string) => Promise<boolean>;
    readonly loginAsync: (userName: string, password: string) => Promise<boolean>;
}

export const useAuthentication = (): IAuthentication => {
    const context = useApplicationContext();

    const registerAsync = React.useCallback(
        async (token: string, userName: string, password: string) => {
            const request: IRegisterRequest = {
                token,
                userName,
                password
            };

            const result = await makeRegisterRequestAsync(request);

            if (result === undefined) {
                return false;
            }

            context.setUser({
                token: result.token,
                userName: result.userName
            });

            return true;
        },
        [context]
    );

    const loginAsync = React.useCallback(
        async (userName: string, password: string) => {
            const request: ILoginRequest = {
                userName,
                password
            };

            const result = await makeLoginRequestAsync(request);

            if (result === undefined) {
                return false;
            }

            context.setUser({
                token: result.token,
                userName: result.userName
            });

            return true;
        },
        [context]
    );

    return {
        isAuthenticated: context.state.user !== undefined,
        registerAsync,
        loginAsync
    };
};