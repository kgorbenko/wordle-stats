import { createBrowserRouter } from 'react-router-dom';
import { IApplicationContext } from '../ApplicationContext.ts';
import { homeRoute, loginRoute, logoutRoute, registerRoute } from './routes.ts';
import { Layout } from '../components/Layout.tsx';
import { protectedLoader, unauthenticatedLoader } from '../components/Auth/loaders.ts';
import { PublicPage } from '../components/PublicPage.tsx';
import { loginAction } from '../components/Auth/LoginPage/action.ts';
import { LoginPage } from '../components/Auth/LoginPage/LoginPage.tsx';
import { logoutAction } from '../components/Auth/actions.ts';
import { ErrorPage } from '../components/ErrorPage.tsx';
import { UnauthenticatedLayout } from '../components/Auth/UnauthenticatedLayout.tsx';
import { registerAction } from '../components/Auth/RegisterPage/action.ts';
import { RegisterPage } from '../components/Auth/RegisterPage/RegisterPage.tsx';

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
        loader: unauthenticatedLoader(applicationContext),
        children: [
            {
                path: loginRoute,
                action: loginAction(applicationContext),
                Component: LoginPage
            },
            {
                path: registerRoute,
                action: registerAction(applicationContext),
                Component: RegisterPage
            },
            {
                path: logoutRoute,
                action: logoutAction(applicationContext)
            }
        ]
    }
]);

export default routerDefinition;