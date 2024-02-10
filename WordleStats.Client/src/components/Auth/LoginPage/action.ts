import { LoaderFunctionArgs, redirect } from 'react-router-dom';
import { IApplicationContext } from '../../../ApplicationContext.ts';
import { homeRoute } from '../../../routing/routes.ts';

export const loginAction = (context: IApplicationContext) => async ({ request }: LoaderFunctionArgs) => {
    const formData = await request.formData();
    const username = formData.get("username") as string | null;

    if (!username) {
        return {
            error: "You must provide a username to log in",
        };
    }

    context.setUser({ userName: username, token: username });

    const redirectTo = formData.get("redirectTo") as string | null;
    return redirect(redirectTo || homeRoute);
}