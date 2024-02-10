import { createTheme } from '@mui/material';

const theme = createTheme({
    components: {
        MuiTextField: {
            defaultProps: {
                size: 'small',
                InputLabelProps: {
                    shrink: true
                }
            }
        }
    }
});

export default theme;