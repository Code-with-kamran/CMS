// Services/SlotBuilder.cs
using CMS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMS.Services
{
    public class SlotBuilder
    {
        private readonly ApplicationDbContext _context;

        public SlotBuilder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TimeOnly>> BuildSlotsAsync(int doctorId, DateOnly date)
        {
            var dayOfWeek = date.DayOfWeek;

            // Get doctor's weekly availability for this day
            var availability = await _context.DoctorWeeklyAvailabilities
                .Where(a => a.DoctorId == doctorId
                         && a.IsWorkingDay
                         && a.DayOfWeek == dayOfWeek)
                .FirstOrDefaultAsync();

            if (availability == null)
                return Enumerable.Empty<TimeOnly>();

            // Get booked slots for this doctor on this date
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == date.ToDateTime(TimeOnly.MinValue))
                .Select(a => TimeOnly.FromTimeSpan(a.AppointmentDate.TimeOfDay))
                .ToListAsync();

            // Generate time slots
            var slots = new List<TimeOnly>();
            var slotDuration = availability.SlotDuration > TimeSpan.Zero
                ? availability.SlotDuration
                : TimeSpan.FromMinutes(30);

            var currentTime = TimeOnly.FromTimeSpan(availability.StartTime);
            var endTime = TimeOnly.FromTimeSpan(availability.EndTime);

            while (currentTime.AddMinutes(slotDuration.TotalMinutes) <= endTime)
            {
                // Skip if slot is already booked
                if (!bookedSlots.Contains(currentTime))
                {
                    // If it's today, only show future slots
                    if (date == DateOnly.FromDateTime(DateTime.Today))
                    {
                        var now = TimeOnly.FromDateTime(DateTime.Now);
                        if (currentTime > now)
                            slots.Add(currentTime);
                    }
                    else
                    {
                        slots.Add(currentTime);
                    }
                }

                currentTime = currentTime.AddMinutes(slotDuration.TotalMinutes);
            }

            return slots;
        }
    }
}
