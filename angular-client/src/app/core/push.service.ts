import { Injectable } from '@angular/core';
import { SwPush } from '@angular/service-worker';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class PushService {
  private readonly VAPID_PUBLIC_KEY = 'BCghwNA42-_rNkje_eJGTAcLqjbEHjl13GERGSPF0PGsZ6CDh9VsFIbjKBLhxkFweCNZgZoXOLG_FvOp9HgxblU';

  constructor(private swPush: SwPush, private http: HttpClient) {}

  async init(role: 'BAR'|'BUCATARIE', userId: string) {
    try {
      const perm = await Notification.requestPermission();
      if (perm !== 'granted') return;

      const sub = await this.swPush.requestSubscription({
        serverPublicKey: this.VAPID_PUBLIC_KEY
      });

      await this.http.post('/api/notifications/subscribe', {
        userId,
        role,
        subscription: sub
      }).toPromise();

      this.swPush.messages.subscribe(msg => console.log('Push message:', msg));
      this.swPush.notificationClicks.subscribe(e => console.log('click', e));
    } catch (e) {
      console.error('Push init failed:', e);
    }
  }


  async resyncPush(userId: string, role: 'BAR'|'BUCATARIE') {
    try {
      // ia SW + subscripția curentă (nu cere alta dacă există)
      const reg = await navigator.serviceWorker.ready;
      const old = await reg.pushManager.getSubscription();
      if (old) { try { await old.unsubscribe(); } catch {} }
      let sub = await reg.pushManager.getSubscription();

      if (!sub) {
        // dacă nu există încă, cere una cu cheia ta VAPID
        sub = await this.swPush.requestSubscription({
          serverPublicKey: this.VAPID_PUBLIC_KEY
        });
      }

      // retrimite la backend cu user/rolul real -> va face UPDATE
      await this.http.post('/api/notifications/subscribe', {
        userId, role, subscription: sub
      }).toPromise();
    } catch (e) {
      console.error('[Push] resync failed:', e);
    }
  }

}
