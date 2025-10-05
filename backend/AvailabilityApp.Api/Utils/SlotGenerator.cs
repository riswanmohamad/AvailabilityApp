using AvailabilityApp.Api.Models;

namespace AvailabilityApp.Api.Utils
{
    public interface ISlotGenerator
    {
        List<AvailableSlot> GenerateSlots(AvailabilityPattern pattern, DateTime startDate, DateTime endDate);
    }

    public class SlotGenerator : ISlotGenerator
    {
        public List<AvailableSlot> GenerateSlots(AvailabilityPattern pattern, DateTime startDate, DateTime endDate)
        {
            var slots = new List<AvailableSlot>();

            switch (pattern.SlotType.ToLower())
            {
                case "minute":
                    slots = GenerateMinuteSlots(pattern, startDate, endDate);
                    break;
                case "hour":
                    slots = GenerateHourlySlots(pattern, startDate, endDate);
                    break;
                case "day":
                    slots = GenerateDailySlots(pattern, startDate, endDate);
                    break;
                case "week":
                    slots = GenerateWeeklySlots(pattern, startDate, endDate);
                    break;
                case "month":
                    slots = GenerateMonthlySlots(pattern, startDate, endDate);
                    break;
            }

            return slots;
        }

        private List<AvailableSlot> GenerateMinuteSlots(AvailabilityPattern pattern, DateTime startDate, DateTime endDate)
        {
            var slots = new List<AvailableSlot>();
            var daysOfWeek = ParseDaysOfWeek(pattern.DaysOfWeek);

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (daysOfWeek.Contains((int)date.DayOfWeek))
                {
                    var startTime = pattern.StartTime ?? TimeSpan.Zero;
                    var endTime = pattern.EndTime ?? TimeSpan.FromHours(24);

                    var currentTime = startTime;
                    while (currentTime < endTime)
                    {
                        var slotStart = date.Add(currentTime);
                        var slotEnd = slotStart.AddMinutes(pattern.SlotDuration);

                        if (slotEnd.TimeOfDay <= endTime)
                        {
                            slots.Add(new AvailableSlot
                            {
                                Id = Guid.NewGuid(),
                                ServiceId = pattern.ServiceId,
                                PatternId = pattern.Id,
                                StartDateTime = slotStart,
                                EndDateTime = slotEnd,
                                SlotType = pattern.SlotType,
                                IsAvailable = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        currentTime = currentTime.Add(TimeSpan.FromMinutes(pattern.SlotDuration));
                    }
                }
            }

            return slots;
        }

        private List<AvailableSlot> GenerateHourlySlots(AvailabilityPattern pattern, DateTime startDate, DateTime endDate)
        {
            var slots = new List<AvailableSlot>();
            var daysOfWeek = ParseDaysOfWeek(pattern.DaysOfWeek);

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (daysOfWeek.Contains((int)date.DayOfWeek))
                {
                    var startTime = pattern.StartTime ?? TimeSpan.Zero;
                    var endTime = pattern.EndTime ?? TimeSpan.FromHours(24);

                    var currentTime = startTime;
                    while (currentTime < endTime)
                    {
                        var slotStart = date.Add(currentTime);
                        var slotEnd = slotStart.AddMinutes(pattern.SlotDuration);

                        if (slotEnd.TimeOfDay <= endTime)
                        {
                            slots.Add(new AvailableSlot
                            {
                                Id = Guid.NewGuid(),
                                ServiceId = pattern.ServiceId,
                                PatternId = pattern.Id,
                                StartDateTime = slotStart,
                                EndDateTime = slotEnd,
                                SlotType = pattern.SlotType,
                                IsAvailable = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        currentTime = currentTime.Add(TimeSpan.FromMinutes(pattern.SlotDuration));
                    }
                }
            }

            return slots;
        }

        private List<AvailableSlot> GenerateDailySlots(AvailabilityPattern pattern, DateTime startDate, DateTime endDate)
        {
            var slots = new List<AvailableSlot>();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                slots.Add(new AvailableSlot
                {
                    Id = Guid.NewGuid(),
                    ServiceId = pattern.ServiceId,
                    PatternId = pattern.Id,
                    StartDateTime = date,
                    EndDateTime = date.AddDays(1).AddSeconds(-1),
                    SlotType = pattern.SlotType,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return slots;
        }

        private List<AvailableSlot> GenerateWeeklySlots(AvailabilityPattern pattern, DateTime startDate, DateTime endDate)
        {
            var slots = new List<AvailableSlot>();
            var startOfWeek = GetStartOfWeek(startDate);

            for (var weekStart = startOfWeek; weekStart <= endDate; weekStart = weekStart.AddDays(7))
            {
                var weekEnd = weekStart.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

                slots.Add(new AvailableSlot
                {
                    Id = Guid.NewGuid(),
                    ServiceId = pattern.ServiceId,
                    PatternId = pattern.Id,
                    StartDateTime = weekStart,
                    EndDateTime = weekEnd,
                    SlotType = pattern.SlotType,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return slots;
        }

        private List<AvailableSlot> GenerateMonthlySlots(AvailabilityPattern pattern, DateTime startDate, DateTime endDate)
        {
            var slots = new List<AvailableSlot>();
            var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);

            while (currentMonth <= endDate)
            {
                var monthEnd = currentMonth.AddMonths(1).AddSeconds(-1);

                slots.Add(new AvailableSlot
                {
                    Id = Guid.NewGuid(),
                    ServiceId = pattern.ServiceId,
                    PatternId = pattern.Id,
                    StartDateTime = currentMonth,
                    EndDateTime = monthEnd,
                    SlotType = pattern.SlotType,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                });

                currentMonth = currentMonth.AddMonths(1);
            }

            return slots;
        }

        private List<int> ParseDaysOfWeek(string? daysOfWeek)
        {
            if (string.IsNullOrEmpty(daysOfWeek))
                return new List<int> { 0, 1, 2, 3, 4, 5, 6 }; // All days

            return daysOfWeek.Split(',')
                .Where(d => int.TryParse(d.Trim(), out _))
                .Select(d => int.Parse(d.Trim()))
                .ToList();
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var daysFromSunday = (int)date.DayOfWeek;
            return date.AddDays(-daysFromSunday).Date;
        }
    }
}