import * as React from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App.tsx';
import { assertNonNullable } from './utils.ts';

import './index.css';

const rootElement = document.getElementById('root');
assertNonNullable(rootElement);

const root = createRoot(rootElement);

root.render(
    <React.StrictMode>
        <App />
    </React.StrictMode>
);