const CACHE_NAME = 'glamstudio-v1';
const OFFLINE_URL = '/Client/Home/Offline';

// Pliki do zachowania w pamięci podręcznej
const ASSETS_TO_CACHE = [
    OFFLINE_URL,
    '/css/site.css',
    '/GlamStudio.styles.css',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/jquery/dist/jquery.min.js',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/js/site.js',
    '/images/logo_menu_nawigacyjne.png',
    '/images/logo.png'
];

// 1. Instalacja
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            console.log('[Service Worker] Caching static assets');
            return cache.addAll(ASSETS_TO_CACHE);
        })
    );
    self.skipWaiting();
});

// 2. Aktywacja
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keyList) => {
            return Promise.all(keyList.map((key) => {
                if (key !== CACHE_NAME) {
                    console.log('[Service Worker] Removing old cache', key);
                    return caches.delete(key);
                }
            }));
        })
    );
    self.clients.claim();
});

// 3. Pobieranie
self.addEventListener('fetch', (event) => {
    if (event.request.method !== 'GET') return;

    event.respondWith(
        fetch(event.request)
            .catch(() => {
                return caches.match(event.request).then((response) => {
                    if (response) {
                        return response;
                    }
                    if (event.request.headers.get('accept').includes('text/html')) {
                        return caches.match(OFFLINE_URL);
                    }
                });
            })
    );
});