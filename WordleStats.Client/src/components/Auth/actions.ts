import { IApplicationContext } from '../../ApplicationContext.ts';
import { redirect } from 'react-router-dom';
import { homeRoute } from '../../routing/routes.ts';

export const logoutAction = (applicationContext: IApplicationContext) => async () => {
    applicationContext.setUser(undefined);
    return redirect(homeRoute);
}