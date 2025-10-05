import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ServiceManagementService } from '../../services/service-management.service';
import { CreateServiceRequest, UpdateServiceRequest, Service, ServiceImage } from '../../models/models';

@Component({
  selector: 'app-service-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './service-form.component.html',
  styleUrls: ['./service-form.component.css']
})
export class ServiceFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private serviceManagementService = inject(ServiceManagementService);

  serviceForm: FormGroup;
  isEditing = false;
  isSubmitting = false;
  serviceId: string | null = null;
  selectedFile: File | null = null;
  currentService: Service | null = null;
  images: ServiceImage[] = [];
  isUploading = false;

  constructor() {
    this.serviceForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      duration: [60, [Validators.min(1), Validators.max(1440)]]
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.serviceId = id;
      this.isEditing = true;
      this.loadService();
    }
  }

  async loadService() {
    if (!this.serviceId) return;

    try {
      const service = await this.serviceManagementService.getService(this.serviceId);
      if (service) {
        this.currentService = service;
        this.images = service.images || [];
        this.serviceForm.patchValue({
          title: service.title,
          description: service.description,
          duration: service.duration
        });
      }
    } catch (error) {
      console.error('Error loading service:', error);
      alert('Failed to load service');
      this.goBack();
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        alert('Please select a valid image file (JPEG, PNG, GIF, or WebP)');
        return;
      }

      // Validate file size (5MB)
      if (file.size > 5 * 1024 * 1024) {
        alert('File size must be less than 5MB');
        return;
      }

      this.selectedFile = file;
    }
  }

  async uploadImage() {
    if (!this.selectedFile || !this.serviceId) return;

    this.isUploading = true;
    try {
      const uploadedImage = await this.serviceManagementService.uploadImage(this.serviceId, this.selectedFile);
      if (uploadedImage) {
        this.images.push(uploadedImage);
        this.selectedFile = null;
        // Clear the file input
        const fileInput = document.getElementById('imageUpload') as HTMLInputElement;
        if (fileInput) fileInput.value = '';
      }
    } catch (error) {
      console.error('Error uploading image:', error);
      alert('Failed to upload image');
    } finally {
      this.isUploading = false;
    }
  }

  async deleteImage(imageId: string) {
    if (!this.serviceId || !confirm('Are you sure you want to delete this image?')) return;

    try {
      const success = await this.serviceManagementService.deleteImage(this.serviceId, imageId);
      if (success) {
        this.images = this.images.filter(img => img.id !== imageId);
      }
    } catch (error) {
      console.error('Error deleting image:', error);
      alert('Failed to delete image');
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.serviceForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  async onSubmit() {
    if (this.serviceForm.invalid) {
      this.markAllFieldsAsTouched();
      return;
    }

    this.isSubmitting = true;

    try {
      const formValue = this.serviceForm.value;

      if (this.isEditing && this.serviceId) {
        const updateRequest: UpdateServiceRequest = {
          title: formValue.title,
          description: formValue.description,
          duration: formValue.duration
        };
        await this.serviceManagementService.updateService(this.serviceId, updateRequest);
      } else {
        const createRequest: CreateServiceRequest = {
          title: formValue.title,
          description: formValue.description,
          duration: formValue.duration
        };
        const newService = await this.serviceManagementService.createService(createRequest);
        if (newService && this.selectedFile) {
          // If there's a file selected, upload it after creating the service
          this.serviceId = newService.id;
          await this.uploadImage();
        }
      }

      this.router.navigate(['/dashboard']);
    } catch (error) {
      console.error('Error saving service:', error);
      alert(`Failed to ${this.isEditing ? 'update' : 'create'} service`);
    } finally {
      this.isSubmitting = false;
    }
  }

  private markAllFieldsAsTouched() {
    Object.keys(this.serviceForm.controls).forEach(key => {
      this.serviceForm.get(key)?.markAsTouched();
    });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}