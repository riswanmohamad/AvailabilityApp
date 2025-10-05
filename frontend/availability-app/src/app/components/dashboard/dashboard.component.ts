import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ServiceManagementService } from '../../services/service-management.service';
import { Service, SharableLink } from '../../models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private serviceManagementService = inject(ServiceManagementService);

  services: Service[] = [];
  isLoading = true;
  isDeletingService: string | null = null;
  
  // Public link modal
  showPublicLinkModal = false;
  selectedService: Service | null = null;
  publicLink = '';
  linkCopied = false;
  isRegenerating = false;

  ngOnInit() {
    this.loadServices();
  }

  get activeServices(): number {
    // Since we removed isActive, let's count all services for now
    return this.services.length;
  }

  get totalViews(): number {
    // Since we removed viewCount, return 0 for now
    return 0;
  }

  async loadServices() {
    try {
      this.isLoading = true;
      this.services = await this.serviceManagementService.getServices();
    } catch (error) {
      console.error('Error loading services:', error);
      // In a real app, you'd show a proper error message
      alert('Failed to load services');
    } finally {
      this.isLoading = false;
    }
  }

  async deleteService(serviceId: string) {
    if (!confirm('Are you sure you want to delete this service?')) {
      return;
    }

    try {
      this.isDeletingService = serviceId;
      await this.serviceManagementService.deleteService(serviceId);
      this.services = this.services.filter(s => s.id !== serviceId);
    } catch (error) {
      console.error('Error deleting service:', error);
      alert('Failed to delete service');
    } finally {
      this.isDeletingService = null;
    }
  }

  async viewPublicLink(service: Service) {
    try {
      this.selectedService = service;
      const link = await this.serviceManagementService.generateSharableLink(service.id);
      if (link) {
        this.publicLink = `${window.location.origin}/public/${link.token}`;
        this.showPublicLinkModal = true;
        this.linkCopied = false;
      }
    } catch (error) {
      console.error('Error getting public link:', error);
      alert('Failed to get public link');
    }
  }

  closePublicLinkModal() {
    this.showPublicLinkModal = false;
    this.selectedService = null;
    this.publicLink = '';
    this.linkCopied = false;
  }

  async copyToClipboard(text: string) {
    try {
      await navigator.clipboard.writeText(text);
      this.linkCopied = true;
      setTimeout(() => this.linkCopied = false, 2000);
    } catch (error) {
      console.error('Failed to copy to clipboard:', error);
      // Fallback for older browsers
      const textArea = document.createElement('textarea');
      textArea.value = text;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand('copy');
      document.body.removeChild(textArea);
      this.linkCopied = true;
      setTimeout(() => this.linkCopied = false, 2000);
    }
  }

  async regenerateLink() {
    if (!this.selectedService) return;

    try {
      this.isRegenerating = true;
      const newLink = await this.serviceManagementService.regenerateSharableLink(this.selectedService.id);
      if (newLink) {
        this.publicLink = `${window.location.origin}/public/${newLink.token}`;
      }
    } catch (error) {
      console.error('Error regenerating link:', error);
      alert('Failed to regenerate link');
    } finally {
      this.isRegenerating = false;
    }
  }
}