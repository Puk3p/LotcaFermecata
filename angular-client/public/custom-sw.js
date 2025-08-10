self.addEventListener('push', (event) => {
  const raw = event.data ? event.data.json() : {};
  const p = raw.notification || raw; // acceptă și {notification:{...}}

  const title = p.title || 'Notificare';
  const options = {
    body: p.body || '',
    icon: p.icon || '/assets/icons/icon-192x192.png',
    badge: p.badge || '/assets/icons/badge-72x72.png',
    image: p.image,
    tag: p.tag,
    renotify: !!p.renotify,
    requireInteraction: !!p.requireInteraction,
    vibrate: p.vibrate || [100, 50, 100],
    timestamp: p.timestamp || Date.now(),
    actions: p.actions || [],
    data: p.data || {}
  };

  event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();

  const action = event.action; // "open" sau "ack"
  const url = (event.notification.data && event.notification.data.url) || '/orders/active';

  // exemplu: butonul „Am preluat”
  if (action === 'ack') {
    // TODO: lovește un endpoint dacă vrei să marchezi „preluat”
    // event.waitUntil(fetch('/api/orders/ack', { method: 'POST', body: JSON.stringify({ id: event.notification.data.orderId }) }));
  }

  // Focalizează o filă existentă sau deschide una nouă
  event.waitUntil((async () => {
    const allClients = await clients.matchAll({ type: 'window', includeUncontrolled: true });
    const existing = allClients.find(c => c.url.includes('/orders'));
    if (existing) { existing.focus(); existing.postMessage({ type: 'OPEN_ORDER', data: event.notification.data }); }
    else { clients.openWindow(url); }
  })());
});
