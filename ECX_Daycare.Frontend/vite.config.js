import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';
// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
            '@components': path.resolve(__dirname, './src/components'),
            '@layouts': path.resolve(__dirname, './src/layouts'),
            '@pages': path.resolve(__dirname, './src/pages'),
            '@services': path.resolve(__dirname, './src/services'),
            '@hooks': path.resolve(__dirname, './src/hooks'),
            '@types': path.resolve(__dirname, './src/types'),
            '@utils': path.resolve(__dirname, './src/utils'),
            '@routes': path.resolve(__dirname, './src/routes'),
            '@features': path.resolve(__dirname, './src/features'),
        },
    },
    server: {
        port: 5173,
        open: true,
    },
    build: {
        outDir: 'dist',
        sourcemap: false,
        minify: 'terser',
        rollupOptions: {
            output: {
                manualChunks: function (id) {
                    var feature = id.match(/[\\/]src[\\/]features[\\/]([^\\/]+)/);
                    if (feature)
                        return "feature-".concat(feature[1]);
                    if (id.includes('/node_modules/')) {
                        if (/[\\/]node_modules[\\/](react|react-dom|scheduler)[\\/]/.test(id))
                            return 'vendor-react';
                        if (id.includes('/node_modules/lucide-react/'))
                            return 'vendor-icons';
                        if (id.includes('/node_modules/axios/'))
                            return 'vendor-http';
                        return 'vendor';
                    }
                },
            },
        },
    },
});
