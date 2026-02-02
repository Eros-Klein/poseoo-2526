import { SessionService } from './../../services/session.service';
import { ApiConfiguration } from './../../api/api-configuration';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Field, form, required } from '@angular/forms/signals';
import { PinCheckReq } from '../../api/models';
import { Api } from '../../api/api';
import { environment } from '../../../environments/environment';
import { verifyPin } from '../../api/functions';
import { Router } from '@angular/router';

interface LoginReq {
  wishlistName: string,
  pin: string
}

@Component({
  selector: 'app-login-page',
  imports: [Field],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage implements OnInit {

  ngOnInit(): void {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl
  }

  protected readonly error = signal<string>('')
  protected readonly loginObject = signal<LoginReq>({
    pin: "",
    wishlistName: ""
  })

  protected readonly api = inject(Api)
  protected readonly apiConfiguration = inject(ApiConfiguration)
  protected readonly router = inject(Router)
  protected readonly sessionService = inject(SessionService)

  protected readonly loginForm = form(this.loginObject, (schemaPath) => {
    required(schemaPath.pin, { message: "Pin is required" })
    required(schemaPath.wishlistName, {message: "WishlistName is required"})
  })

  protected async onSubmit(event: Event) {
    event.preventDefault()

    if(this.loginForm.wishlistName().invalid() || this.loginForm.pin().invalid()){
      this.error.set("Please correct failing validations")
      return;
    }

    try {
      const formData = this.loginObject()
      const toSend: PinCheckReq = { pin: formData.pin }

      const res = await this.api.invoke(verifyPin, {
        body: toSend,
        name: formData.wishlistName
      })

      this.sessionService.pin.set(toSend.pin)
      this.sessionService.wishlistName.set(formData.wishlistName)

      if(res == "parent") {
        this.sessionService.role.set('parent')
        this.router.navigate(['/parent'])
      } else if(res == "child") {
        this.sessionService.role.set('child')
        this.router.navigate(['/add-item'])
      } else {
        this.sessionService.role.set('none')
      }
    }
    catch (e: any) {
      this.error.set('Error saving: ' + (e.message || JSON.stringify(e)));
    }
  }
}
