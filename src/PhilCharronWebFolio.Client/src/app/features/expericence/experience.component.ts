import { Component } from '@angular/core';
import { CrtcProject } from './project-card.model';
   
@Component({
  selector: 'app-experience',
  templateUrl: 'experience.component.html',
  styleUrls: ['experience.component.scss']
})

export class ExperienceComponent {
  readonly projects: CrtcProject[] = [
    {
      name: 'ListPortal',
      problem: 'Violations WCAG critiques — contraste, clavier, ARIA absents',
      tools: ['WAVE', 'axe DevTools', 'NVDA', 'WET-BOEW'],
      actions: ['Audit complet', 'Priorisation par sévérité', 'Correction et validation'],
      result: 'Conformité 100 % — zéro violation axe DevTools',
      wcagScore: 100
    },
    {
      name: 'VCR',
      problem: 'Filtres dynamiques non accessibles aux lecteurs d\'écran',
      tools: ['jQuery', 'aria-live', 'Session Storage', 'NVDA'],
      actions: ['Scripts réutilisables', 'Régions aria-live', 'Tests NVDA'],
      result: 'Navigation NVDA fonctionnelle, filtres annoncés correctement',
      wcagScore: 100
    },
    {
      name: 'MMA',
      problem: 'Dette technique importante, architecture non structurée',
      tools: ['C#', 'LINQ', 'Repository Pattern', 'Clean Architecture'],
      actions: ['Refonte back-end', 'Repository Pattern', 'Réduction dette technique'],
      result: 'Base de code maintenable et testable',
      wcagScore: 100
    }
  ];
}