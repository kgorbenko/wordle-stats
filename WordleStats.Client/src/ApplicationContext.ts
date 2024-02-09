import * as React from 'react';
import { assertNonNullable } from './utils';

export interface IUser {
    readonly token: string;
    readonly userName: string;
}

export interface IApplicationState {
    readonly user: IUser | undefined;
}

export interface IApplicationContext {
    readonly state: IApplicationState;
    readonly setUser: (user: IUser | undefined) => void;
}

export const ApplicationContext = React.createContext<IApplicationContext | undefined>(undefined);

export const useApplicationContext = () => {
    const context = React.useContext(ApplicationContext);
    assertNonNullable(context);
    return context;
}