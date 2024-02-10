import * as React from 'react';
import { useLocalStorage } from '@uidotdev/usehooks';
import { ApplicationContext, IApplicationContext, IApplicationState, IUser } from '../ApplicationContext';

const initialContextValue: IApplicationState = {
    user: undefined
};

export const ApplicationContextProvider = ({ children }: React.PropsWithChildren) => {
    const [contextData, setContextData] = useLocalStorage<IApplicationState>("WordleStatsApplicationContext", initialContextValue);

    const setUser = React.useCallback(
        (user: IUser | undefined) => {
            setContextData(prevState => ({
                ...prevState,
                user
            }));
        },
        [setContextData]
    );

    const contextValue: IApplicationContext = React.useMemo(
        () => ({
            state: contextData,
            setUser: setUser
        }),
        [contextData, setUser]
    );

    return (
        <ApplicationContext.Provider value={contextValue}>
            {children}
        </ApplicationContext.Provider>
    );
}