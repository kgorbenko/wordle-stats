import { redirect } from 'react-router-dom';
import { IApplicationContext } from '../../../ApplicationContext.ts';
import { homeRoute } from '../../../routing/routes.ts';

export const loginLoader = (applicationContext: IApplicationContext) => () => {
    if (applicationContext.state.user !== undefined) {
        return redirect(homeRoute);
    }
    return null;
}