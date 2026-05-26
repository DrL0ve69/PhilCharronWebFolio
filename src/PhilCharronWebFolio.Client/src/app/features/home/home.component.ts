import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  template: `
    <div class="home-container">
      <section class="hero">
        <h1 i18n>Welcome to Phil Charron's Portfolio</h1>
        <p i18n>Explore my work, skills, and expertise in modern software architecture.</p>
        <div class="actions">
          <button class="btn-primary" routerLink="/projects" i18n>View Projects</button>
          <button class="btn-secondary" routerLink="/contact" i18n>Contact Me</button>
        </div>
      </section>
    </div>
  `,
  styles: `
    .home-container {
      min-height: 100vh;
      display: flex;
      justify-content: center;
      align-items: center;
      text-align: center;
      padding: 2rem;
      background: var(--bg-color, #f8f9fa);
    }

    .hero {
      max-width: 800px;
    }

    h1 {
      font-size: 3rem;
      margin-bottom: 1.5rem;
      color: var(--text-color, #333);
    }

    p {
      font-size: 1.25rem;
      margin-bottom: 2rem;
      color: var(--text-secondary, #666);
    }

    .actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
    }

    .btn-primary, .btn-secondary {
      padding: 0.75rem 1.5rem;
      border-radius: 0.5rem;
      font-weight: 600;
      cursor: pointer;
      transition: transform 0.2s;
    }

    .btn-primary {
      background: var(--primary-color, #007bff);
      color: white;
      border: none;
    }

    .btn-secondary {
      background: transparent;
      color: var(--primary-color, #007bff);
      border: 2px solid var(--primary-color, #007bff);
    }

    .btn-primary:hover, .btn-secondary:hover {
      transform: translateY(-2px);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent {}
