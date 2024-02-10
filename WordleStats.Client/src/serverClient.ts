export interface IRegisterRequest {
    readonly token: string;
    readonly userName: string;
    readonly password: string;
}

export interface ILoginRequest {
    readonly userName: string;
    readonly password: string;
}

export interface ILoginResponse {
    readonly token: string;
    readonly userName: string;
}

const registerUrl = 'https://localhost:5001/api/auth/register';
const loginUrl = 'https://localhost:5001/api/auth/login';

export async function registerAsync(request: IRegisterRequest, signal?: AbortSignal): Promise<ILoginResponse | undefined> {
    return await tryMakePostRequestAsync(registerUrl, request, signal);
}

export async function loginAsync(request: ILoginRequest, signal?: AbortSignal): Promise<ILoginResponse | undefined> {
    return await tryMakePostRequestAsync(loginUrl, request, signal);
}

async function tryMakePostRequestAsync<TBody extends object, TResponse>(url: string, body?: TBody, signal?: AbortSignal): Promise<TResponse | undefined> {
    try {
        return await makePostRequestAsync(url, body, signal);
    } catch (e) {
        console.error(e);
        return undefined;
    }
}

async function makePostRequestAsync<TBody extends object, TResponse>(url: string, body?: TBody, signal?: AbortSignal): Promise<TResponse | undefined> {
    const response = await fetch(url, {
        method: 'POST',
        body: body !== undefined ? JSON.stringify(body) : undefined,
        headers: {
            'Content-Type': 'application/json'
        },
        signal: signal
    });

    return response.ok
        ? response.json()
        : undefined;
}