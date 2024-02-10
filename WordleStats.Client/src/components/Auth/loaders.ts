import { LoaderFunctionArgs, redirect } from 'react-router-dom';
import { IApplicationContext } from '../../ApplicationContext.ts';
import { loginRoute } from '../../routing/routes.ts';

export const protectedLoader = (context: IApplicationContext) => ({ request }: LoaderFunctionArgs) => {
    if (context.state.user === undefined) {
        const params = new URLSearchParams();
        params.set("from", new URL(request.url).pathname);
        return redirect(loginRoute + '?' + params.toString());
    }
    return null;
}