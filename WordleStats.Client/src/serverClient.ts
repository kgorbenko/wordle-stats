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

const registerUrl = '/api/auth/register';
const loginUrl = '/api/auth/login';

export async function registerAsync(request: IRegisterRequest): Promise<ILoginResponse | undefined> {
    return await tryMakePostRequestAsync(registerUrl, request);
}

export async function loginAsync(request: ILoginRequest): Promise<ILoginResponse | undefined> {
    return await tryMakePostRequestAsync(loginUrl, request);
}

async function tryMakePostRequestAsync<TBody extends object, TResponse>(url: string, body?: TBody): Promise<TResponse | undefined> {
    try {
        return await makePostRequestAsync(url, body);
    } catch (e) {
        console.error(e);
        return undefined;
    }
}

async function makePostRequestAsync<TBody extends object, TResponse>(url: string, body?: TBody): Promise<TResponse | undefined> {
    const response = await fetch(url, {
        method: 'POST',
        body: body !== undefined ? JSON.stringify(body) : undefined,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    return response.ok
        ? response.json()
        : undefined;
}