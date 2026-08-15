import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { IconComponent } from '../../shared/components/icon.component';

@Component({
  selector: 'hero-register',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, IconComponent],
  templateUrl: './register.component.html',
  styleUrl: './auth-page.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly submitting = signal(false);
  protected readonly showPassword = signal(false);

  protected togglePassword(): void {
    this.showPassword.update((shown) => !shown);
  }

  // These rules mirror ClsRegisterRequestValidator on the API exactly. Client-side validation is
  // only for fast feedback - the API re-validates, because anything enforced in the browser
  // can be bypassed.
  protected readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern(/^[a-zA-Z0-9._-]+$/)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(128),
        Validators.pattern(/(?=.*[A-Z])/),
        Validators.pattern(/(?=.*[a-z])/),
        Validators.pattern(/(?=.*[0-9])/),
      ],
    ],
  });

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);

    this.auth.register(this.form.getRawValue()).subscribe({
      next: (response) => {
        this.notifications.success(`Welcome, ${response.username}.`);
        void this.router.navigate(['/superheroes']);
      },
      error: () => this.submitting.set(false),
    });
  }

  protected invalid(control: 'username' | 'email' | 'password'): boolean {
    const field = this.form.controls[control];
    return field.invalid && (field.dirty || field.touched);
  }

  protected passwordError(): string {
    const errors = this.form.controls.password.errors;
    if (!errors) {
      return '';
    }
    if (errors['required']) {
      return 'Password is required.';
    }
    if (errors['minlength']) {
      return 'Password must be at least 8 characters.';
    }
    return 'Password needs an uppercase letter, a lowercase letter and a digit.';
  }

  protected usernameError(): string {
    const errors = this.form.controls.username.errors;
    if (!errors) {
      return '';
    }
    if (errors['required']) {
      return 'Username is required.';
    }
    if (errors['minlength']) {
      return 'Username must be at least 3 characters.';
    }
    return 'Only letters, numbers, dot, underscore and hyphen are allowed.';
  }
}
