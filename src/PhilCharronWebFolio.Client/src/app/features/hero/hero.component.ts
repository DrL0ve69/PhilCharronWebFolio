import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-hero',
  changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: './hero.component.html',
    styleUrls: ['./hero.component.scss']
  })

export class HeroComponent {
  readonly badges = ['WCAG 2.2 AA', 'axe DevTools', 'NVDA', 'Azure DevOps', 'WET-BOEW'];
}