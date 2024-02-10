import * as React from 'react';
import classNames from 'classnames';
import { useFetcher, useSearchParams } from 'react-router-dom';
import { Alert, Stack, TextField } from '@mui/material';
import { ILoginActionData, ILoginFormData, loginFormDataValidationSchema } from './formData.ts';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { LoadingButton } from '@mui/lab';

import './LoginPage.scss';

export const LoginPage: React.FC = () => {
    const [params] = useSearchParams();
    const from = params.get("from") ?? undefined;

    const fetcher = useFetcher();

    const isSubmitting = fetcher.state === 'submitting';
    const fetcherData: ILoginActionData | undefined = fetcher.data;

    const {
        register,
        handleSubmit,
        formState
    } = useForm<ILoginFormData>({
        resolver: yupResolver(loginFormDataValidationSchema),
        disabled: isSubmitting
    });

    const onSubmit = React.useCallback(
        (formData: ILoginFormData) => {
            fetcher.submit(JSON.stringify(formData), { method: 'POST', encType: "application/json" });
        },
        [fetcher]
    );

    const userNameMessage = formState.errors.userName?.message;
    const passwordMessage = formState.errors.password?.message;

    return (
        <div className="login-page">
            {
                <Alert
                    severity="error"
                    className={classNames(
                        'login-error', {
                        'visible': fetcherData !== undefined && !isSubmitting
                    })}
                >
                    {fetcherData?.message ?? ' '}
                </Alert>
            }

            <form onSubmit={handleSubmit(onSubmit)}>
                <input
                    {...register('redirectTo')}
                    type="hidden"
                    value={from}
                />

                <Stack
                    direction="column"
                    spacing={2}
                >
                    <TextField
                        {...register('userName')}
                        label="Login"
                        helperText={userNameMessage ?? ' '}
                        error={userNameMessage !== undefined}
                        fullWidth
                    />
                    <TextField
                        {...register('password')}
                        label="Password"
                        type="password"
                        helperText={passwordMessage ?? ' '}
                        error={passwordMessage !== undefined}
                        fullWidth
                    />
                    <LoadingButton
                        type="submit"
                        variant="contained"
                        loading={isSubmitting}
                        disabled={isSubmitting}
                    >
                        Sign In
                    </LoadingButton>
                </Stack>
            </form>
        </div>);
}