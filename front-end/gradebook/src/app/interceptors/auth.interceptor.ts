import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  console.log('Auth interceptor running for URL:', req.url);
  console.log('Token available:', !!token);
  
  if (token) {
    // Clone the request and add the authorization header
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    console.log('Added auth token to request');
    
    // Pass the cloned request with the token to the next handler
    return next(authReq);
  }
  
  console.log('No token available, forwarding original request');
  
  // If no token is available, just forward the original request
  return next(req);
}; 