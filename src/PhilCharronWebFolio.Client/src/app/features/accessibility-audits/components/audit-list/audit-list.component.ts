import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AccessibilityAuditService, CreateAuditRequest, AccessibilityAudit } from '../../services/accessibility-audit.service';

@Component({
  selector: 'app-audit-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <main class="audit-container">
      <h1 class="title">Rapports d'Audit d'Accessibilité</h1>

      <section class="form-section" aria-labelledby="create-audit-title">
        <h2 id="create-audit-title">Créer un nouvel audit</h2>
        <form [formGroup]="auditForm" (ngSubmit)="onSubmit()" class="audit-form">
          <div class="form-group">
            <label for="projectName">Nom du projet <span class="required" aria-hidden="true">*</span></label>
            <input id="projectName" type="text" formControlName="projectName"
                   aria-required="true"
                   [attr.aria-invalid]="auditForm.get('projectName')?.invalid && auditForm.get('projectName')?.touched">
            @if (auditForm.get('projectName')?.invalid && auditForm.get('projectName')?.touched) {
              <span class="error-message" role="alert">Le nom du projet est requis.</span>
            }
          </div>

          <div class="form-group">
            <label for="projectUrl">URL du projet <span class="required" aria-hidden="true">*</span></label>
            <input id="projectUrl" type="url" formControlName="projectUrl"
                   aria-required="true"
                   [attr.aria-invalid]="auditForm.get('projectUrl')?.invalid && auditForm.get('projectUrl')?.touched">
            @if (auditForm.get('projectUrl')?.invalid && auditForm.get('projectUrl')?.touched) {
              <span class="error-message" role="alert">Une URL valide est requise.</span>
            }
          </div>

          <button type="submit" [disabled]="auditForm.invalid" class="btn-submit">
            Lancer l'audit
          </button>
        </form>
      </section>

      <section class="list-section" aria-labelledby="audit-list-title">
        <h2 id="audit-list-title">Audits récents</h2>
        <div class="audit-grid">
          @for (audit of audits(); track audit.id) {
            <article class="audit-card" tabindex="0">
              <h3>{{ audit.projectName }}</h3>
              <p>URL: <a [href]="audit.projectUrl" target="_blank">{{ audit.projectUrl }}</a></p>
              <p>Date: {{ audit.auditDate | date:'shortDate' }}</p>
              <span class="status-badge" [class.completed]="audit.isCompleted">
                {{ audit.isCompleted ? 'Complété' : 'En cours' }}
              </span>
            </article>
          } @empty {
            <p class="empty-msg">Aucun audit trouvé. Commencez par en créer un.</p>
          }
        </div>
      </section>
    </main>
  `,
  styles: [`
    .audit-container { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .title { font-size: 2.5rem; margin-bottom: 2rem; color: var(--primary-color); }
    .form-section, .list-section { margin-bottom: 3rem; }
    .audit-form { display: grid; gap: 1.5rem; max-width: 600px; }
    .form-group { display: flex; flex-direction: column; gap: 0.5rem; }
    .required { color: red; }
    .error-message { color: var(--error-color, #d32f2f); font-size: 0.875rem; font-weight: bold; }
    .btn-submit { padding: 0.75rem 1.5rem; cursor: pointer; background: var(--primary-color); color: white; border: none; border-radius: 4px; font-weight: bold; }
    .btn-submit:disabled { opacity: 0.5; cursor: not-allowed; }
    .audit-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 1rem; }
    .audit-card { border: 2px solid var(--border-color); padding: 1rem; border-radius: 8px; transition: border-color 0.2s; }
    .audit-card:focus { outline: 3px solid var(--focus-color, #005a9c); border-color: transparent; }
    .status-badge { padding: 0.25rem 0.5rem; border-radius: 4px; background: #eee; font-size: 0.75rem; }
    .status-badge.completed { background: #c8e6c9; color: #2e7d32; }
  `]
})
export class AuditListComponent {
  readonly #fb = inject(FormBuilder);
  readonly #auditService = inject(AccessibilityAuditService);

  audits = signal<AccessibilityAudit[]>([]);

  auditForm = this.#fb.group({
    projectName: ['', [Validators.required]],
    projectUrl: ['', [Validators.required, Validators.pattern('https?://.+')]],
  });

  onSubmit() {
    if (this.auditForm.valid) {
      const request: CreateAuditRequest = this.auditForm.value as CreateAuditRequest;
      this.#auditService.createAudit(request).subscribe(() => {
        this.auditForm.reset();
        // Ideally, reload the list
      });
    }
  }
}
