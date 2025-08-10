import { Injectable } from '@angular/core';
import { SwPush } from '@angular/service-worker';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class PushService {
  private readonly VAPID_PUBLIC_KEY = 'BCghwNA42-_rNkje_eJGTAcLqjbEHjl13GERGSPF0PGsZ6CDh9VsFIbjKBLhxkFweCNZgZoXOLG_FvOp9HgxblU';

  constructor(private swPush: SwPush, private http: HttpClient) {}

  async init(role: 'BAR'|'BUCATARIE', userId: string) {
    try {
      console.log('[Push] init start', { role, userId, isEnabled: this.swPush.isEnabled, perm: Notification.permission });
      const perm = await Notification.requestPermission();
      
      console.log('[Push] permission result =', perm);
      if (perm !== 'granted') return;

      const sub = await this.swPush.requestSubscription({
        serverPublicKey: this.VAPID_PUBLIC_KEY
      });
      console.log('[Push] got subscription', sub?.endpoint);

      await this.http.post('/api/notifications/subscribe', {
        userId,
        role,
        subscription: sub
      }).toPromise();
      console.log('[Push] subscribe sent OK');
      this.swPush.messages.subscribe(msg => console.log('Push message:', msg));
      this.swPush.notificationClicks.subscribe(e => console.log('click', e));
    } catch (e) {
      console.error('Push init failed:', e);
    }
  }


  async resyncPush(userId: string, role: 'BAR'|'BUCATARIE') {
  try {
    // dacă nu e granted, nu forțăm nimic aici
    if (Notification.permission !== 'granted') return;

    const reg = await navigator.serviceWorker.ready;
    let sub = await reg.pushManager.getSubscription();
    if (!sub) {
      // nu crea alta aici ca să nu declanșezi prompturi; ieși linistit
      return;
    }

    await this.http.post('/api/notifications/subscribe', {
      userId, role, subscription: sub
    }).toPromise();
  } catch (e) {
    console.warn('[Push] resync failed:', e);
  }
}


}
