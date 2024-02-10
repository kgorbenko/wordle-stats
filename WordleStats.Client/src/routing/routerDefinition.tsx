import { createBrowserRouter } from 'react-router-dom';
import { IApplicationContext } from '../ApplicationContext.ts';
import { homeRoute, loginRoute, logoutRoute } from './routes.ts';
import { Layout } from '../components/Layout.tsx';
import { protectedLoader } from '../components/Auth/loaders.ts';
import { PublicPage } from '../components/PublicPage.tsx';
import { loginAction } from '../components/Auth/LoginPage/action.ts';
import { loginLoader } from '../components/Auth/LoginPage/loader.ts';
import { LoginPage } from '../components/Auth/LoginPage/LoginPage.tsx';
import { logoutAction } from '../components/Auth/actions.ts';
import { ErrorPage } from '../components/ErrorPage.tsx';
import { UnauthenticatedLayout } from '../components/Auth/UnauthenticatedLayout.tsx';

const routerDefinition = (applicationContext: IApplicationContext) => createBrowserRouter([
    {
        path: homeRoute,
        Component: Layout,
        loader: protectedLoader(applicationContext),
        ErrorBoundary: ErrorPage,
        children: [
            {
                index: true,
                Component: PublicPage,
            }
        ],
    },
    {
        Component: UnauthenticatedLayout,
        ErrorBoundary: ErrorPage,
        children: [
            {
                path: loginRoute,
                action: loginAction(applicationContext),
                loader: loginLoader(applicationContext),
                Component: LoginPage
            },
            {
                path: logoutRoute,
                action: logoutAction(applicationContext)
            }
        ]
    }
]);

export default routerDefinition;