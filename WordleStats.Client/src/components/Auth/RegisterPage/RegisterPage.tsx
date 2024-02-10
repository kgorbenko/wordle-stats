import * as React from 'react';
import classNames from 'classnames';
import { createSearchParams, Link as RouterLink, useFetcher, useSearchParams } from 'react-router-dom';
import { Alert, Stack, TextField, Typography, Link } from '@mui/material';
import { IRegisterActionData, IRegisterFormData, registerFormDataValidationSchema } from './formData.ts';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { LoadingButton } from '@mui/lab';
import { loginRoute } from '../../../routing/routes.ts';

import './RegisterPage.scss';

function makeLoginLink(from: string | undefined) {
    if (from === undefined) {
        return loginRoute;
    }

    const searchParams = createSearchParams({
        from: from
    });

    return `${loginRoute}?${searchParams}`;
}

export const RegisterPage: React.FC = () => {
    const [params] = useSearchParams();
    const from = params.get("from") ?? undefined;

    const fetcher = useFetcher();

    const isSubmitting = fetcher.state === 'submitting';
    const fetcherData: IRegisterActionData | undefined = fetcher.data;

    const {
        register,
        handleSubmit,
        formState
    } = useForm<IRegisterFormData>({
        resolver: yupResolver(registerFormDataValidationSchema),
        disabled: isSubmitting
    });

    const onSubmit = React.useCallback(
        (formData: IRegisterFormData) => {
            fetcher.submit(JSON.stringify(formData), { method: 'POST', encType: "application/json" });
        },
        [fetcher]
    );

    const tokenMessage = formState.errors.token?.message;
    const userNameMessage = formState.errors.userName?.message;
    const passwordMessage = formState.errors.password?.message;
    const confirmPasswordMessage = formState.errors.confirmPassword?.message;

    return (
        <div className="register-page">
            {
                <Alert
                    severity="error"
                    className={classNames(
                        'register-error', {
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
                        {...register('token')}
                        label="Token"
                        helperText={tokenMessage ?? ' '}
                        error={tokenMessage !== undefined}
                        fullWidth
                    />
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
                    <TextField
                        {...register('confirmPassword')}
                        label="Password"
                        type="password"
                        helperText={confirmPasswordMessage ?? ' '}
                        error={confirmPasswordMessage !== undefined}
                        fullWidth
                    />
                    <LoadingButton
                        type="submit"
                        variant="contained"
                        loading={isSubmitting}
                        disabled={isSubmitting}
                    >
                        Sign Up
                    </LoadingButton>
                    <Typography variant="body2">
                        Already have an account? <Link component={RouterLink} to={makeLoginLink(from)} replace>Sign in</Link>
                    </Typography>
                </Stack>
            </form>
        </div>
    );
}