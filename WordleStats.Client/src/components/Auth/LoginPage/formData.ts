import * as Yup from 'yup';

export interface ILoginFormData {
    readonly redirectTo?: string;
    readonly userName: string;
    readonly password: string;
}

export interface ILoginActionData {
    readonly message: string;
}

export const loginFormDataValidationSchema: Yup.ObjectSchema<ILoginFormData> = Yup.object({
    redirectTo: Yup.string().strict(true),
    userName: Yup.string().strict(true).required('Login is required'),
    password: Yup.string().strict(true).required('Password is required')
}).strict(true).required();