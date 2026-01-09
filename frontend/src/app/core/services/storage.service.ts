import { Injectable } from '@angular/core';

// Make this a singleton app wide:
@Injectable({
  providedIn: 'root'
})
export class StorageService {
  
  setItem(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  getItem(key: string): string | null {
    return localStorage.getItem(key);
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }
}
