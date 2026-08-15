import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { SuperheroService } from '../../core/services/superhero.service';
import { ALIGNMENTS, STAT_KEYS, SuperheroRequest } from '../../shared/models/superhero.models';

/** Admin create/edit form. The same component serves both, keyed off the optional route id. */
@Component({
  selector: 'hero-superhero-form',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './superhero-form.component.html',
  styleUrl: './superhero-form.component.scss',
})
export class SuperheroFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly superheroes = inject(SuperheroService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  /** Absent when creating. Bound from the route via withComponentInputBinding(). */
  readonly id = input<string | undefined>();

  protected readonly alignments = ALIGNMENTS;
  protected readonly statKeys = STAT_KEYS;

  protected readonly isEdit = computed(() => !!this.id());
  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);

  // Ranges mirror the API validators and the DB CHECK constraints (0-100).
  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    realName: [''],
    universe: ['', [Validators.required, Validators.maxLength(50)]],
    alignment: ['Hero', [Validators.required]],
    description: [''],
    imageUrl: [''],
    powerLevel: [50, [Validators.required, Validators.min(0), Validators.max(100)]],
    intelligence: [50, [Validators.required, Validators.min(0), Validators.max(100)]],
    strength: [50, [Validators.required, Validators.min(0), Validators.max(100)]],
    speed: [50, [Validators.required, Validators.min(0), Validators.max(100)]],
    durability: [50, [Validators.required, Validators.min(0), Validators.max(100)]],
    combat: [50, [Validators.required, Validators.min(0), Validators.max(100)]],
  });

  ngOnInit(): void {
    // Route inputs are populated before ngOnInit, so this.id() is safe to read here.
    if (this.isEdit()) {
      this.loadExisting();
    }
  }

  private loadExisting(): void {
    const heroId = Number(this.id());
    this.loading.set(true);

    this.superheroes.getById(heroId).subscribe({
      next: (hero) => {
        this.form.patchValue({
          name: hero.name,
          realName: hero.realName ?? '',
          universe: hero.universe,
          alignment: hero.alignment,
          description: hero.description ?? '',
          imageUrl: hero.imageUrl ?? '',
          powerLevel: hero.powerLevel,
          intelligence: hero.intelligence,
          strength: hero.strength,
          speed: hero.speed,
          durability: hero.durability,
          combat: hero.combat,
        });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigate(['/superheroes']);
      },
    });
  }

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const raw = this.form.getRawValue();

    // Empty text inputs become null rather than "" - the API models these columns as nullable.
    const request: SuperheroRequest = {
      name: raw.name.trim(),
      realName: raw.realName.trim() || null,
      universe: raw.universe.trim(),
      alignment: raw.alignment,
      description: raw.description.trim() || null,
      imageUrl: raw.imageUrl.trim() || null,
      powerLevel: raw.powerLevel,
      intelligence: raw.intelligence,
      strength: raw.strength,
      speed: raw.speed,
      durability: raw.durability,
      combat: raw.combat,
    };

    const request$ = this.isEdit()
      ? this.superheroes.update(Number(this.id()), request)
      : this.superheroes.create(request);

    request$.subscribe({
      next: (hero) => {
        this.notifications.success(this.isEdit() ? `${hero.name} updated.` : `${hero.name} created.`);
        void this.router.navigate(['/superheroes', hero.id]);
      },
      // A 409 (duplicate name) or 400 is already toasted by the error interceptor.
      error: () => this.submitting.set(false),
    });
  }

  protected invalid(control: string): boolean {
    const field = this.form.get(control);
    return !!field && field.invalid && (field.dirty || field.touched);
  }
}
