import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { TeamService } from '../../core/services/team.service';
import { TeamListItem } from '../../shared/models/power-team.models';

@Component({
  selector: 'hero-team-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './team-list.component.html',
  styleUrl: './team-list.component.scss',
})
export class TeamListComponent implements OnInit {
  private readonly teams = inject(TeamService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly notifications = inject(NotificationService);

  protected readonly isAdmin = this.auth.isAdmin;

  protected readonly items = signal<TeamListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly universe = signal('');
  protected readonly formOpen = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    universe: ['', [Validators.required, Validators.maxLength(50)]],
    description: [''],
    foundedDate: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.teams.getAll(this.universe() || undefined).subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected onUniverseChange(value: string): void {
    this.universe.set(value);
    this.load();
  }

  protected openCreate(): void {
    this.form.reset({ name: '', universe: '', description: '', foundedDate: '' });
    this.formOpen.set(true);
  }

  protected save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const raw = this.form.getRawValue();

    this.teams
      .create({
        name: raw.name.trim(),
        universe: raw.universe.trim(),
        description: raw.description.trim() || null,
        // The date input gives "" when empty; the API expects null, not an empty string.
        foundedDate: raw.foundedDate || null,
      })
      .subscribe({
        next: () => {
          this.notifications.success('Team created.');
          this.saving.set(false);
          this.formOpen.set(false);
          this.load();
        },
        error: () => this.saving.set(false),
      });
  }

  protected invalid(control: 'name' | 'universe'): boolean {
    const field = this.form.controls[control];
    return field.invalid && (field.dirty || field.touched);
  }
}
