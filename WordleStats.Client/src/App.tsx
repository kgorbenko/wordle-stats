import * as React from 'react';
import { RouterProvider } from 'react-router-dom';
import { useApplicationContext } from './ApplicationContext.ts';
import routerDefinition from './routing/routerDefinition.tsx';

import './App.css';

export const App: React.FC = () => {
    const context = useApplicationContext();

    const router = React.useMemo(
        () => routerDefinition(context),
        [context]
    );

    return (
        <RouterProvider
            router={router}
            fallbackElement={<p>Initial Load...</p>}
        />
    );
};