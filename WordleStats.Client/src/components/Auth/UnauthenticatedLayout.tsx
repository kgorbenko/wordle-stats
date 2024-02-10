import * as React from 'react';
import { Typography } from '@mui/material';
import { Outlet } from 'react-router-dom';

import './UnauthenticatedLayout.scss';

export const UnauthenticatedLayout: React.FC = () =>
    <div className="unauthenticated-layout">
        <div className="header-section">
            <Typography variant="h4">
                Welcome to WordleStats
            </Typography>
        </div>

        <div className="content-area">
            <Outlet />
        </div>
    </div>;