import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { 
  AvailabilityPattern,
  CreateAvailabilityPatternRequest,
  AvailableSlot,
  ServiceException,
  CreateExceptionRequest,
  PublicService,
  ApiResponse 
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class AvailabilityService {
  private apiUrl = 'https://localhost:7000/api'; 

  constructor(private http: HttpClient) {}

  // Availability Patterns - Async methods
  async getAvailabilityPatterns(serviceId: string): Promise<AvailabilityPattern[]> {
    const response = await firstValueFrom(
      this.http.get<AvailabilityPattern[]>(`${this.apiUrl}/services/${serviceId}/availability-patterns`)
    );
    return response;
  }

  async createAvailabilityPattern(serviceId: string, pattern: CreateAvailabilityPatternRequest): Promise<AvailabilityPattern> {
    const response = await firstValueFrom(
      this.http.post<AvailabilityPattern>(`${this.apiUrl}/services/${serviceId}/availability-patterns`, pattern)
    );
    return response;
  }

  async updateAvailabilityPattern(serviceId: string, patternId: string, pattern: CreateAvailabilityPatternRequest): Promise<AvailabilityPattern> {
    const response = await firstValueFrom(
      this.http.put<AvailabilityPattern>(`${this.apiUrl}/services/${serviceId}/availability-patterns/${patternId}`, pattern)
    );
    return response;
  }

  async deleteAvailabilityPattern(serviceId: string, patternId: string): Promise<void> {
    await firstValueFrom(
      this.http.delete(`${this.apiUrl}/services/${serviceId}/availability-patterns/${patternId}`)
    );
  }

  // Available Slots
  async getAvailableSlots(serviceId: string, startDate?: string, endDate?: string): Promise<AvailableSlot[]> {
    let url = `${this.apiUrl}/services/${serviceId}/available-slots`;
    const params = new URLSearchParams();
    
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    
    if (params.toString()) {
      url += `?${params.toString()}`;
    }

    const response = await firstValueFrom(this.http.get<AvailableSlot[]>(url));
    return response;
  }

  async regenerateSlots(serviceId: string): Promise<void> {
    await firstValueFrom(
      this.http.post(`${this.apiUrl}/services/${serviceId}/regenerate-slots`, {})
    );
  }

  // Exceptions
  async getExceptions(serviceId: string): Promise<ServiceException[]> {
    const response = await firstValueFrom(
      this.http.get<ServiceException[]>(`${this.apiUrl}/services/${serviceId}/exceptions`)
    );
    return response;
  }

  async createException(serviceId: string, exception: CreateExceptionRequest): Promise<ServiceException> {
    const response = await firstValueFrom(
      this.http.post<ServiceException>(`${this.apiUrl}/services/${serviceId}/exceptions`, exception)
    );
    return response;
  }

  async updateException(serviceId: string, exceptionId: string, exception: CreateExceptionRequest): Promise<ServiceException> {
    const response = await firstValueFrom(
      this.http.put<ServiceException>(`${this.apiUrl}/services/${serviceId}/exceptions/${exceptionId}`, exception)
    );
    return response;
  }

  async deleteException(serviceId: string, exceptionId: string): Promise<void> {
    await firstValueFrom(
      this.http.delete(`${this.apiUrl}/services/${serviceId}/exceptions/${exceptionId}`)
    );
  }

  // Public Service
  async getPublicService(token: string): Promise<PublicService> {
    const response = await firstValueFrom(
      this.http.get<PublicService>(`${this.apiUrl}/public/services/${token}`)
    );
    return response;
  }

  // Observable versions for reactive components
  getAvailabilityPatternsObservable(serviceId: string): Observable<AvailabilityPattern[]> {
    return this.http.get<AvailabilityPattern[]>(`${this.apiUrl}/services/${serviceId}/availability-patterns`);
  }

  getAvailableSlotsObservable(serviceId: string, startDate?: string, endDate?: string): Observable<AvailableSlot[]> {
    let url = `${this.apiUrl}/services/${serviceId}/available-slots`;
    const params = new URLSearchParams();
    
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    
    if (params.toString()) {
      url += `?${params.toString()}`;
    }

    return this.http.get<AvailableSlot[]>(url);
  }

  getExceptionsObservable(serviceId: string): Observable<ServiceException[]> {
    return this.http.get<ServiceException[]>(`${this.apiUrl}/services/${serviceId}/exceptions`);
  }
}