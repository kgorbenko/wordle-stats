import { LoaderFunctionArgs, redirect } from 'react-router-dom';
import { IApplicationContext } from '../../../ApplicationContext.ts';
import { homeRoute } from '../../../routing/routes.ts';
import { ILoginActionData, ILoginFormData } from './formData.ts';
import { ILoginRequest, loginAsync } from '../../../serverClient.ts';

export const loginAction = (context: IApplicationContext) => async ({ request }: LoaderFunctionArgs) => {
    const formData: ILoginFormData = await request.json();

    const loginRequest: ILoginRequest = {
        userName: formData.userName,
        password: formData.password
    }

    const result = await loginAsync(loginRequest);

    if (result !== undefined) {
        context.setUser({ userName: result.userName, token: result.token });
        return redirect(formData.redirectTo ?? homeRoute);
    }

    const error: ILoginActionData = {
        message: 'Invalid login or password'
    };

    return error;
}