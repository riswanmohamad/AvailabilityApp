import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AvailabilityService } from '../../services/availability.service';
import { PublicService, AvailableSlot } from '../../models/models';

@Component({
  selector: 'app-public-service-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './public-service-view.component.html',
  styleUrls: ['./public-service-view.component.css']
})
export class PublicServiceViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private availabilityService = inject(AvailabilityService);

  service: PublicService | null = null;
  availableSlots: AvailableSlot[] = [];
  groupedSlots: { day: string; slots: AvailableSlot[] }[] = [];
  selectedSlot: AvailableSlot | null = null;
  
  isLoading = true;
  isLoadingSlots = false;
  
  currentWeekStart: Date = new Date();
  
  ngOnInit() {
    const token = this.route.snapshot.paramMap.get('token');
    if (token) {
      this.loadPublicService(token);
      this.initializeWeek();
    } else {
      this.isLoading = false;
    }
  }

  async loadPublicService(token: string) {
    try {
      this.isLoading = true;
      this.service = await this.availabilityService.getPublicService(token);
      this.loadAvailableSlots();
    } catch (error) {
      console.error('Error loading public service:', error);
      this.service = null;
    } finally {
      this.isLoading = false;
    }
  }

  async loadAvailableSlots() {
    if (!this.service) return;

    try {
      this.isLoadingSlots = true;
      const weekEnd = new Date(this.currentWeekStart);
      weekEnd.setDate(weekEnd.getDate() + 6);

      // For public view, we use the available slots from the service
      // In a real implementation, you might have a separate endpoint for public availability
      this.availableSlots = this.service.availableSlots || [];
      this.groupSlotsByDay();
    } catch (error) {
      console.error('Error loading available slots:', error);
      this.availableSlots = [];
      this.groupedSlots = [];
    } finally {
      this.isLoadingSlots = false;
    }
  }

  initializeWeek() {
    const today = new Date();
    const dayOfWeek = today.getDay();
    const mondayDate = new Date(today);
    mondayDate.setDate(today.getDate() - (dayOfWeek === 0 ? 6 : dayOfWeek - 1));
    this.currentWeekStart = mondayDate;
  }

  navigateWeek(direction: number) {
    const newWeekStart = new Date(this.currentWeekStart);
    newWeekStart.setDate(newWeekStart.getDate() + (direction * 7));
    this.currentWeekStart = newWeekStart;
    this.selectedSlot = null;
    this.loadAvailableSlots();
  }

  groupSlotsByDay() {
    const weekEnd = new Date(this.currentWeekStart);
    weekEnd.setDate(weekEnd.getDate() + 6);

    // Filter slots for the current week
    const weekSlots = this.availableSlots.filter(slot => {
      const slotDate = new Date(slot.startDateTime);
      return slotDate >= this.currentWeekStart && slotDate <= weekEnd;
    });

    // Group by day
    const groups: { [key: string]: AvailableSlot[] } = {};
    
    weekSlots.forEach(slot => {
      const slotDate = new Date(slot.startDateTime);
      const dayKey = slotDate.toDateString();
      
      if (!groups[dayKey]) {
        groups[dayKey] = [];
      }
      groups[dayKey].push(slot);
    });

    // Convert to array and sort
    this.groupedSlots = Object.keys(groups)
      .sort((a, b) => new Date(a).getTime() - new Date(b).getTime())
      .map(dayKey => ({
        day: this.formatDayHeader(new Date(dayKey)),
        slots: groups[dayKey].sort((a, b) => 
          new Date(a.startDateTime).getTime() - new Date(b.startDateTime).getTime()
        )
      }));
  }

  selectSlot(slot: AvailableSlot) {
    if (slot.isAvailable) {
      this.selectedSlot = slot;
    }
  }

  clearSelection() {
    this.selectedSlot = null;
  }

  formatWeekRange(weekStart: Date): string {
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 6);
    
    const options: Intl.DateTimeFormatOptions = { 
      month: 'short', 
      day: 'numeric',
      year: weekStart.getFullYear() !== new Date().getFullYear() ? 'numeric' : undefined
    };
    
    return `${weekStart.toLocaleDateString(undefined, options)} - ${weekEnd.toLocaleDateString(undefined, options)}`;
  }

  formatDayHeader(date: Date): string {
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);

    if (date.toDateString() === today.toDateString()) {
      return 'Today, ' + date.toLocaleDateString(undefined, { weekday: 'long', month: 'short', day: 'numeric' });
    } else if (date.toDateString() === tomorrow.toDateString()) {
      return 'Tomorrow, ' + date.toLocaleDateString(undefined, { weekday: 'long', month: 'short', day: 'numeric' });
    } else {
      return date.toLocaleDateString(undefined, { weekday: 'long', month: 'short', day: 'numeric' });
    }
  }

  formatSlotTime(slot: AvailableSlot): string {
    const start = new Date(slot.startDateTime);
    const end = new Date(slot.endDateTime);
    
    const timeOptions: Intl.DateTimeFormatOptions = { 
      hour: 'numeric', 
      minute: '2-digit',
      hour12: true
    };
    
    return `${start.toLocaleTimeString(undefined, timeOptions)} - ${end.toLocaleTimeString(undefined, timeOptions)}`;
  }

  formatSelectedSlot(slot: AvailableSlot): string {
    const start = new Date(slot.startDateTime);
    const dateOptions: Intl.DateTimeFormatOptions = { 
      weekday: 'long', 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    };
    const timeOptions: Intl.DateTimeFormatOptions = { 
      hour: 'numeric', 
      minute: '2-digit',
      hour12: true
    };
    
    return `${start.toLocaleDateString(undefined, dateOptions)} at ${start.toLocaleTimeString(undefined, timeOptions)}`;
  }
}