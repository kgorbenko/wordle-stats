import * as React from 'react';
import { useFetcher } from 'react-router-dom';
import { useApplicationContext } from '../../../ApplicationContext.ts';
import { logoutRoute } from '../../../routing/routes.ts';

export const AuthStatus: React.FC = () => {
    const context = useApplicationContext();
    const  fetcher = useFetcher();

    if (!context.state.user) {
        return <p>You are not logged in.</p>;
    }

    const isLoggingOut = fetcher.formData != null;

    return (
        <div>
            <p>Welcome {context.state.user.userName}!</p>
            <fetcher.Form method="post" action={logoutRoute}>
                <button type="submit" disabled={isLoggingOut}>
                    {isLoggingOut ? "Signing out..." : "Sign out"}
                </button>
            </fetcher.Form>
        </div>
    );
}