import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Api } from '../../api/api';
import { ApiConfiguration } from '../../api/api-configuration';
import { Router } from '@angular/router';
import { SessionService } from '../../services/session.service';
import { WishlistItem } from '../../api/models';
import { wishlistNameItemsPost } from '../../api/functions';

@Component({
  selector: 'app-wishlist-items-page',
  imports: [],
  templateUrl: './wishlist-items-page.html',
  styleUrl: './wishlist-items-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WishlistItemsPage implements OnInit {
  ngOnInit(): void {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl

    if(!this.sessionService.isParent) {
      this.router.navigate(["/login"])
    } else {
      this.loadData()
    }
  }

  protected readonly error = signal<string>('')
  protected readonly loading = signal<boolean>(true)

  protected readonly api = inject(Api)
  protected readonly apiConfiguration = inject(ApiConfiguration)
  protected readonly router = inject(Router)
  protected readonly sessionService = inject(SessionService)

  protected readonly wishlistItems = signal<WishlistItem[]>([])

  protected async loadData() {
    this.loading.set(true)

    const items = await this.api.invoke(wishlistNameItemsPost, {name: this.sessionService.wishlistName(), body: {
      pin: this.sessionService.pin()
    }})

    this.wishlistItems.set(items)

    this.loading.set(false)
  }
}
