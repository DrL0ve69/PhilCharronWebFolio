import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ENVIRONMENT } from '../../../core/tokens/environment.token';
import { Observable } from 'rxjs';

export interface AccessibilityAudit {
  id: string;
  projectName: string;
  projectUrl: string;
  auditDate: string;
  isCompleted: boolean;
}

export interface CreateAuditRequest {
  projectName: string;
  projectUrl: string;
}

@Injectable({ providedIn: 'root' })
export class AccessibilityAuditService {
  readonly #http = inject(HttpClient);
  readonly #env = inject(ENVIRONMENT);
  readonly apiUrl = `${this.#env.apiUrl}/api/v1/accessibilityaudits`;

  createAudit(request: CreateAuditRequest): Observable<{ id: string }> {
    return this.#http.post<{ id: string }>(this.apiUrl, request);
  }

  // Other methods for findings, etc.
}
