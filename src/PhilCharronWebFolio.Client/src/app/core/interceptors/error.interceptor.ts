import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
//import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  //const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'Une erreur est survenue.';
      
      if (error.status === 404) message = 'Ressource non trouvée.';
      if (error.status === 403) message = 'Accès refusé.';

      //notificationService.showError(message);
      return throwError(() => error);
    })
  );
};