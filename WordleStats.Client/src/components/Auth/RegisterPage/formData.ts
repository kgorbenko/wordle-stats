import * as Yup from 'yup';
import { nameof } from '../../../utils.ts';

export interface IRegisterFormData {
    readonly redirectTo?: string;
    readonly token: string;
    readonly userName: string;
    readonly password: string;
    readonly confirmPassword: string;
}

export interface IRegisterActionData {
    readonly message: string
}

const userNameMaxLength = 32;
const passwordMinLength = 8;

export const registerFormDataValidationSchema: Yup.ObjectSchema<IRegisterFormData> = Yup.object({
    redirectTo: Yup.string().strict(true),
    token: Yup.string().strict(true).required('Token is required'),
    userName: Yup.string().strict(true).required('Login is required')
        .max(userNameMaxLength, `Login should be no longer than ${userNameMaxLength} characters`),
    password: Yup.string().strict(true).required('Password is required')
        .min(passwordMinLength, `Password should be at least ${passwordMinLength} characters long`),
    confirmPassword: Yup.string().strict(true).required('Password confirmation is required')
        .oneOf([Yup.ref(nameof<IRegisterFormData>('password'))], 'Please type same password as above')
}).strict(true).required();