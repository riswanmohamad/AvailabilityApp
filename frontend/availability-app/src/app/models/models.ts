// Auth Models
export interface User {
  id: string;
  email?: string;
  firstName: string;
  lastName: string;
  businessName?: string;
  phoneNumber?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  businessName?: string;
  phoneNumber?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

// Service Models
export interface Service {
  id: string;
  title: string;
  description?: string;
  duration?: number;
  durationUnit: string; // 'minutes' | 'hours' | 'days' | 'months'
  createdAt: string;
  updatedAt: string;
  sharableToken?: string;
  images: ServiceImage[];
}

export interface CreateServiceRequest {
  title: string;
  description?: string;
  duration?: number;
  durationUnit: string; // 'minutes' | 'hours' | 'days' | 'months'
}

export interface UpdateServiceRequest {
  title: string;
  description?: string;
  duration?: number;
  durationUnit: string; // 'minutes' | 'hours' | 'days' | 'months'
}

// Image Models
export interface ServiceImage {
  id: string;
  serviceId: string;
  url: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  displayOrder: number;
  createdAt: string;
}

// Availability Models
export interface AvailabilityPattern {
  id: string;
  slotType: string;
  slotDuration: number;
  startTime?: string;
  endTime?: string;
  daysOfWeek?: string;
  startDate: string;
  endDate?: string;
}

export interface CreateAvailabilityPatternRequest {
  slotType: string;
  slotDuration: number;
  startTime?: string;
  endTime?: string;
  daysOfWeek?: string;
  startDate: string;
  endDate?: string;
}

export interface AvailableSlot {
  id: string;
  startDateTime: string;
  endDateTime: string;
  slotType: string;
  isAvailable: boolean;
}

// Exception Models
export interface ServiceException {
  id: string;
  title: string;
  description?: string;
  startDateTime: string;
  endDateTime: string;
  exceptionType: string;
  recurringYearly: boolean;
}

export interface CreateExceptionRequest {
  title: string;
  description?: string;
  startDateTime: string;
  endDateTime: string;
  exceptionType: string;
  recurringYearly: boolean;
}

// Public Models
export interface PublicService {
  title: string;
  description?: string;
  duration?: number;
  providerName: string;
  businessName?: string;
  availableSlots: AvailableSlot[];
  images: ServiceImage[];
}

export interface SharableLink {
  token: string;
  publicUrl: string;
  createdAt: string;
}

// API Response
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}