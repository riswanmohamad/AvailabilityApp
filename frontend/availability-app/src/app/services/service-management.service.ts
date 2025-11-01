import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { 
  Service, 
  CreateServiceRequest,
  UpdateServiceRequest,
  ServiceImage,
  SharableLink, 
  ApiResponse 
} from '../models/models';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class ServiceManagementService {
  private apiUrl = `${environment.apiUrl}/services`; // Update with your API URL

  constructor(private http: HttpClient) {}

  async getServices(): Promise<Service[]> {
    const response = await firstValueFrom(this.http.get<ApiResponse<Service[]>>(this.apiUrl));
    return response.data || [];
  }

  async getService(id: string): Promise<Service | null> {
    try {
      const response = await firstValueFrom(this.http.get<ApiResponse<Service>>(`${this.apiUrl}/${id}`));
      return response.data || null;
    } catch {
      return null;
    }
  }

  async createService(service: CreateServiceRequest): Promise<Service | null> {
    try {
      const response = await firstValueFrom(this.http.post<ApiResponse<Service>>(this.apiUrl, service));
      return response.data || null;
    } catch {
      return null;
    }
  }

  async updateService(id: string, service: UpdateServiceRequest): Promise<Service | null> {
    try {
      const response = await firstValueFrom(this.http.put<ApiResponse<Service>>(`${this.apiUrl}/${id}`, service));
      return response.data || null;
    } catch {
      return null;
    }
  }

  async deleteService(id: string): Promise<boolean> {
    try {
      const response = await firstValueFrom(this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`));
      return response.data || false;
    } catch {
      return false;
    }
  }

  async generateSharableLink(serviceId: string): Promise<SharableLink | null> {
    try {
      const response = await firstValueFrom(this.http.post<ApiResponse<SharableLink>>(`${this.apiUrl}/${serviceId}/sharable-link`, {}));
      return response.data || null;
    } catch {
      return null;
    }
  }

  async regenerateSharableLink(serviceId: string): Promise<SharableLink | null> {
    try {
      const response = await firstValueFrom(this.http.post<ApiResponse<SharableLink>>(`${this.apiUrl}/${serviceId}/regenerate-link`, {}));
      return response.data || null;
    } catch {
      return null;
    }
  }

  // Image methods
  async uploadImage(serviceId: string, file: File): Promise<ServiceImage | null> {
    try {
      const formData = new FormData();
      formData.append('file', file);
      
      const response = await firstValueFrom(
        this.http.post<ApiResponse<ServiceImage>>(`${this.apiUrl}/${serviceId}/images`, formData)
      );
      return response.data || null;
    } catch {
      return null;
    }
  }

  async getImages(serviceId: string): Promise<ServiceImage[]> {
    try {
      const response = await firstValueFrom(
        this.http.get<ApiResponse<ServiceImage[]>>(`${this.apiUrl}/${serviceId}/images`)
      );
      return response.data || [];
    } catch {
      return [];
    }
  }

  async deleteImage(serviceId: string, imageId: string): Promise<boolean> {
    try {
      const response = await firstValueFrom(
        this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${serviceId}/images/${imageId}`)
      );
      return response.data || false;
    } catch {
      return false;
    }
  }

  // Observable versions for components that prefer them
  getServicesObservable(): Observable<ApiResponse<Service[]>> {
    return this.http.get<ApiResponse<Service[]>>(this.apiUrl);
  }

  getServiceObservable(id: string): Observable<ApiResponse<Service>> {
    return this.http.get<ApiResponse<Service>>(`${this.apiUrl}/${id}`);
  }

  createServiceObservable(service: CreateServiceRequest): Observable<ApiResponse<Service>> {
    return this.http.post<ApiResponse<Service>>(this.apiUrl, service);
  }

  updateServiceObservable(id: string, service: UpdateServiceRequest): Observable<ApiResponse<Service>> {
    return this.http.put<ApiResponse<Service>>(`${this.apiUrl}/${id}`, service);
  }

  deleteServiceObservable(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}