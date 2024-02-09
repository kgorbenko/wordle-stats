import * as React from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import { assertNonNullable } from './utils';
import { ApplicationContextProvider } from './components/ApplicationContextProvider';

import './index.css';

const rootElement = document.getElementById('root');
assertNonNullable(rootElement);

const root = createRoot(rootElement);

root.render(
    <React.StrictMode>
        <ApplicationContextProvider>
            <App />
        </ApplicationContextProvider>
    </React.StrictMode>
);