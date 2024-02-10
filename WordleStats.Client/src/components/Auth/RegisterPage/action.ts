import { LoaderFunctionArgs, redirect } from 'react-router-dom';
import { IApplicationContext } from '../../../ApplicationContext.ts';
import { homeRoute } from '../../../routing/routes.ts';
import { IRegisterActionData, IRegisterFormData } from './formData.ts';
import { IRegisterRequest, registerAsync } from '../../../serverClient.ts';

export const registerAction = (context: IApplicationContext) => async ({ request }: LoaderFunctionArgs) => {
    const formData: IRegisterFormData = await request.json();

    const registerRequest: IRegisterRequest = {
        token: formData.token,
        userName: formData.userName,
        password: formData.password
    };

    const result = await registerAsync(registerRequest);

    if (result !== undefined) {
        context.setUser({ userName: result.userName, token: result.token });
        return redirect(formData.redirectTo ?? homeRoute);
    }

    const error: IRegisterActionData = {
        message: 'Provided token is not valid'
    };

    return error;
}