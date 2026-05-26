
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-bug-report',
  imports: [ReactiveFormsModule],
  templateUrl: './bug-report.html'
})
export class BugReportComponent {
  private fb = inject(FormBuilder);
  
  bugForm = this.fb.group({
    title: ['', Validators.required],
    description: ['', Validators.required],
    isAccessibilityIssue: [false],
    wcagCriteria: [''], // Conditionnel
    severity: ['Medium']
  });

  onSubmit() {
    console.log('Envoi du rapport:', this.bugForm.value);
    // Appel API ici...
  }
}