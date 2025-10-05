import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AvailabilityService } from '../../services/availability.service';
import { ServiceManagementService } from '../../services/service-management.service';
import { 
  Service, 
  AvailabilityPattern, 
  CreateAvailabilityPatternRequest,
  AvailableSlot,
  ServiceException,
  CreateExceptionRequest
} from '../../models/models';

@Component({
  selector: 'app-availability-manager',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './availability-manager.component.html',
  styleUrls: ['./availability-manager.component.css']
})
export class AvailabilityManagerComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private availabilityService = inject(AvailabilityService);
  private serviceManagementService = inject(ServiceManagementService);

  serviceId: string | null = null;
  service: Service | null = null;
  patterns: AvailabilityPattern[] = [];
  exceptions: ServiceException[] = [];

  isLoadingPatterns = true;
  isLoadingExceptions = true;
  isDeletingPattern: string | null = null;
  isDeletingException: string | null = null;

  // Pattern Modal
  showPatternModal = false;
  editingPattern: AvailabilityPattern | null = null;
  patternForm: FormGroup;
  isSubmittingPattern = false;
  selectedDays: string[] = [];

  // Exception Modal
  showExceptionModal = false;
  editingException: ServiceException | null = null;
  exceptionForm: FormGroup;
  isSubmittingException = false;

  // Constants
  daysOfWeek = [
    { value: '1', label: 'Monday' },
    { value: '2', label: 'Tuesday' },
    { value: '3', label: 'Wednesday' },
    { value: '4', label: 'Thursday' },
    { value: '5', label: 'Friday' },
    { value: '6', label: 'Saturday' },
    { value: '0', label: 'Sunday' }
  ];

  constructor() {
    this.patternForm = this.fb.group({
      slotType: ['', Validators.required],
      slotDuration: ['', [Validators.required, Validators.min(1)]],
      startTime: [''],
      endTime: [''],
      startDate: ['', Validators.required],
      endDate: ['']
    });

    this.exceptionForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      exceptionType: ['', Validators.required],
      startDateTime: ['', Validators.required],
      endDateTime: ['', Validators.required],
      recurringYearly: [false]
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.serviceId = id;
      this.loadService();
      this.loadPatterns();
      this.loadExceptions();
    } else {
      this.goBack();
    }
  }

  async loadService() {
    if (!this.serviceId) return;

    try {
      this.service = await this.serviceManagementService.getService(this.serviceId);
    } catch (error) {
      console.error('Error loading service:', error);
      alert('Failed to load service');
      this.goBack();
    }
  }

  async loadPatterns() {
    if (!this.serviceId) return;

    try {
      this.isLoadingPatterns = true;
      this.patterns = await this.availabilityService.getAvailabilityPatterns(this.serviceId);
    } catch (error) {
      console.error('Error loading patterns:', error);
      alert('Failed to load availability patterns');
    } finally {
      this.isLoadingPatterns = false;
    }
  }

  async loadExceptions() {
    if (!this.serviceId) return;

    try {
      this.isLoadingExceptions = true;
      this.exceptions = await this.availabilityService.getExceptions(this.serviceId);
    } catch (error) {
      console.error('Error loading exceptions:', error);
      alert('Failed to load exceptions');
    } finally {
      this.isLoadingExceptions = false;
    }
  }

  // Pattern Methods
  openPatternModal() {
    this.editingPattern = null;
    this.selectedDays = [];
    this.patternForm.reset();
    this.showPatternModal = true;
  }

  editPattern(pattern: AvailabilityPattern) {
    this.editingPattern = pattern;
    this.selectedDays = pattern.daysOfWeek ? pattern.daysOfWeek.split(',') : [];
    this.patternForm.patchValue({
      slotType: pattern.slotType,
      slotDuration: pattern.slotDuration,
      startTime: pattern.startTime,
      endTime: pattern.endTime,
      startDate: pattern.startDate.split('T')[0],
      endDate: pattern.endDate ? pattern.endDate.split('T')[0] : ''
    });
    this.showPatternModal = true;
  }

  closePatternModal() {
    this.showPatternModal = false;
    this.editingPattern = null;
    this.selectedDays = [];
    this.patternForm.reset();
  }

  async savePattern() {
    if (this.patternForm.invalid || !this.serviceId) {
      this.markPatternFieldsAsTouched();
      return;
    }

    this.isSubmittingPattern = true;

    try {
      const formValue = this.patternForm.value;
      const patternRequest: CreateAvailabilityPatternRequest = {
        slotType: formValue.slotType,
        slotDuration: formValue.slotDuration,
        startTime: formValue.startTime || undefined,
        endTime: formValue.endTime || undefined,
        daysOfWeek: this.selectedDays.length > 0 ? this.selectedDays.join(',') : undefined,
        startDate: formValue.startDate,
        endDate: formValue.endDate || undefined
      };

      if (this.editingPattern) {
        await this.availabilityService.updateAvailabilityPattern(this.serviceId, this.editingPattern.id, patternRequest);
      } else {
        await this.availabilityService.createAvailabilityPattern(this.serviceId, patternRequest);
      }

      this.closePatternModal();
      this.loadPatterns();
    } catch (error) {
      console.error('Error saving pattern:', error);
      alert(`Failed to ${this.editingPattern ? 'update' : 'create'} pattern`);
    } finally {
      this.isSubmittingPattern = false;
    }
  }

  async deletePattern(patternId: string) {
    if (!confirm('Are you sure you want to delete this availability pattern?') || !this.serviceId) {
      return;
    }

    try {
      this.isDeletingPattern = patternId;
      await this.availabilityService.deleteAvailabilityPattern(this.serviceId, patternId);
      this.patterns = this.patterns.filter(p => p.id !== patternId);
    } catch (error) {
      console.error('Error deleting pattern:', error);
      alert('Failed to delete pattern');
    } finally {
      this.isDeletingPattern = null;
    }
  }

  // Exception Methods
  openExceptionModal() {
    this.editingException = null;
    this.exceptionForm.reset();
    this.showExceptionModal = true;
  }

  editException(exception: ServiceException) {
    this.editingException = exception;
    this.exceptionForm.patchValue({
      title: exception.title,
      description: exception.description,
      exceptionType: exception.exceptionType,
      startDateTime: this.formatDateTimeForInput(exception.startDateTime),
      endDateTime: this.formatDateTimeForInput(exception.endDateTime),
      recurringYearly: exception.recurringYearly
    });
    this.showExceptionModal = true;
  }

  closeExceptionModal() {
    this.showExceptionModal = false;
    this.editingException = null;
    this.exceptionForm.reset();
  }

  async saveException() {
    if (this.exceptionForm.invalid || !this.serviceId) {
      this.markExceptionFieldsAsTouched();
      return;
    }

    this.isSubmittingException = true;

    try {
      const formValue = this.exceptionForm.value;
      const exceptionRequest: CreateExceptionRequest = {
        title: formValue.title,
        description: formValue.description,
        exceptionType: formValue.exceptionType,
        startDateTime: formValue.startDateTime,
        endDateTime: formValue.endDateTime,
        recurringYearly: formValue.recurringYearly
      };

      if (this.editingException) {
        await this.availabilityService.updateException(this.serviceId, this.editingException.id, exceptionRequest);
      } else {
        await this.availabilityService.createException(this.serviceId, exceptionRequest);
      }

      this.closeExceptionModal();
      this.loadExceptions();
    } catch (error) {
      console.error('Error saving exception:', error);
      alert(`Failed to ${this.editingException ? 'update' : 'create'} exception`);
    } finally {
      this.isSubmittingException = false;
    }
  }

  async deleteException(exceptionId: string) {
    if (!confirm('Are you sure you want to delete this exception?') || !this.serviceId) {
      return;
    }

    try {
      this.isDeletingException = exceptionId;
      await this.availabilityService.deleteException(this.serviceId, exceptionId);
      this.exceptions = this.exceptions.filter(e => e.id !== exceptionId);
    } catch (error) {
      console.error('Error deleting exception:', error);
      alert('Failed to delete exception');
    } finally {
      this.isDeletingException = null;
    }
  }

  // Helper Methods
  isDaySelected(day: string): boolean {
    return this.selectedDays.includes(day);
  }

  toggleDay(day: string) {
    if (this.isDaySelected(day)) {
      this.selectedDays = this.selectedDays.filter(d => d !== day);
    } else {
      this.selectedDays.push(day);
    }
  }

  isPatternFieldInvalid(fieldName: string): boolean {
    const field = this.patternForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  isExceptionFieldInvalid(fieldName: string): boolean {
    const field = this.exceptionForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  private markPatternFieldsAsTouched() {
    Object.keys(this.patternForm.controls).forEach(key => {
      this.patternForm.get(key)?.markAsTouched();
    });
  }

  private markExceptionFieldsAsTouched() {
    Object.keys(this.exceptionForm.controls).forEach(key => {
      this.exceptionForm.get(key)?.markAsTouched();
    });
  }

  getPatternDisplayName(pattern: AvailabilityPattern): string {
    const typeNames: { [key: string]: string } = {
      'Minute': 'Minute-based',
      'Hour': 'Hour-based',
      'Day': 'Day-based',
      'Week': 'Week-based',
      'Month': 'Month-based'
    };
    return typeNames[pattern.slotType] || pattern.slotType;
  }

  formatDaysOfWeek(daysOfWeek: string): string {
    const dayNumbers = daysOfWeek.split(',');
    const dayNames = dayNumbers.map(num => {
      const day = this.daysOfWeek.find(d => d.value === num);
      return day ? day.label : num;
    });
    return dayNames.join(', ');
  }

  formatDateRange(startDate: string, endDate?: string): string {
    const start = new Date(startDate).toLocaleDateString();
    if (endDate) {
      const end = new Date(endDate).toLocaleDateString();
      return `${start} - ${end}`;
    }
    return `${start} - Ongoing`;
  }

  formatExceptionPeriod(exception: ServiceException): string {
    const start = new Date(exception.startDateTime);
    const end = new Date(exception.endDateTime);
    
    if (start.toDateString() === end.toDateString()) {
      return `${start.toLocaleDateString()} ${start.toLocaleTimeString()} - ${end.toLocaleTimeString()}`;
    } else {
      return `${start.toLocaleDateString()} ${start.toLocaleTimeString()} - ${end.toLocaleDateString()} ${end.toLocaleTimeString()}`;
    }
  }

  private formatDateTimeForInput(dateTime: string): string {
    const date = new Date(dateTime);
    return date.toISOString().slice(0, 16);
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}