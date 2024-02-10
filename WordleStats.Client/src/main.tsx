import * as React from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import { assertNonNullable } from './utils';
import { ApplicationContextProvider } from './components/ApplicationContextProvider';
import { ThemeProvider } from '@mui/material';

import theme from './theme.ts';

import './index.css';

const rootElement = document.getElementById('root');
assertNonNullable(rootElement);

const root = createRoot(rootElement);

root.render(
    <React.StrictMode>
        <ApplicationContextProvider>
            <ThemeProvider theme={theme}>
                <App />
            </ThemeProvider>
        </ApplicationContextProvider>
    </React.StrictMode>
);