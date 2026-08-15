import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { PowerService } from '../../core/services/power.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { PowerListItem } from '../../shared/models/power-team.models';

@Component({
  selector: 'hero-power-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, ConfirmDialogComponent],
  templateUrl: './power-list.component.html',
  styleUrl: './power-list.component.scss',
})
export class PowerListComponent implements OnInit {
  private readonly powers = inject(PowerService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly notifications = inject(NotificationService);

  protected readonly isAdmin = this.auth.isAdmin;

  protected readonly items = signal<PowerListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly editing = signal<PowerListItem | null>(null);
  protected readonly pendingDelete = signal<PowerListItem | null>(null);
  protected readonly formOpen = signal(false);

  protected readonly assignedCount = computed(
    () => this.items().filter((p) => p.superheroCount > 0).length,
  );

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.powers.getAll().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected openCreate(): void {
    this.editing.set(null);
    this.form.reset({ name: '', description: '' });
    this.formOpen.set(true);
  }

  protected openEdit(power: PowerListItem): void {
    this.editing.set(power);
    this.form.reset({ name: power.name, description: power.description ?? '' });
    this.formOpen.set(true);
  }

  protected closeForm(): void {
    this.formOpen.set(false);
    this.editing.set(null);
  }

  protected save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const raw = this.form.getRawValue();
    const request = { name: raw.name.trim(), description: raw.description.trim() || null };
    const current = this.editing();

    const request$ = current
      ? this.powers.update(current.id, request)
      : this.powers.create(request);

    request$.subscribe({
      next: () => {
        this.notifications.success(current ? 'Power updated.' : 'Power created.');
        this.saving.set(false);
        this.closeForm();
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  protected confirmDelete(): void {
    const power = this.pendingDelete();
    if (!power) {
      return;
    }

    this.powers.delete(power.id).subscribe({
      next: () => {
        this.notifications.success(`${power.name} was deleted.`);
        this.pendingDelete.set(null);
        this.load();
      },
      // A 409 (power still assigned to heroes) is surfaced by the error interceptor.
      error: () => this.pendingDelete.set(null),
    });
  }

  protected invalidName(): boolean {
    const field = this.form.controls.name;
    return field.invalid && (field.dirty || field.touched);
  }
}
