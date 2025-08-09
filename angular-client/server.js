const express = require('express');
const path = require('path');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();

const API_URL = process.env.API_URL || 'https://localhost:7227';
const PORT = process.env.PORT || 63140;

console.log('> Target API_URL =', API_URL);

// 1) Proxy /api -> backend
// 1) PROXY /api -> backend (păstrăm /api în path!)
app.use('/api', (req, res, next) => {
  console.log('[Proxy] incoming:', req.method, req.url);
  next();
}, createProxyMiddleware({
  target: API_URL,
  changeOrigin: true,
  secure: false,
  logLevel: 'debug',
  // IMPORTANT: reatașează /api, pentru că Express l-a tăiat
  pathRewrite: (path, req) => '/api' + path,
  onProxyRes(proxyRes, req) {
    console.log('[Proxy] response:', req.method, req.originalUrl, '->', proxyRes.statusCode);
  },
  onError(err, req, res) {
    console.error('[Proxy] error for', req.method, req.originalUrl, err.message);
    res.status(502).send('Proxy error');
  },
}));


// 2) Static Angular build (calea corectă pentru “application builder”)
const distPath = path.join(__dirname, 'dist', 'angular-client', 'browser');
app.use(express.static(distPath));

// 3) SPA fallback – doar NON /api
app.get(/^(?!\/api(\/|$)).*/, (req, res) => {
  res.sendFile(path.join(distPath, 'index.html'));
});

app.listen(PORT, () => {
  console.log(`> SPA pe http://localhost:${PORT}`);
  console.log(`> Proxy /api -> ${API_URL}`);
});
