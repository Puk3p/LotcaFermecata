self.addEventListener('push', function (event) {
  const data = event.data ? event.data.json() : {};

  const title = data.title || 'Notificare';
  const options = {
    body: data.body || '',
    icon: '/assets/icons/icon-192x192.png',
    badge: '/assets/icons/badge-72x72.png'
  };

  event.waitUntil(
    self.registration.showNotification(title, options)
  );
});
