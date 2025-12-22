using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EventManager : IEventService
    {
        private readonly CampusContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly IWalletService _walletService;

        public EventManager(CampusContext context, IQRCodeService qrCodeService, IWalletService walletService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _walletService = walletService;
        }

        public async Task<Response<PagedResponse<EventListDto>>> GetEventsAsync(EventFilterDto filter, int page = 1, int pageSize = 20)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Where(e => e.IsActive)
                .AsQueryable();

            // Filtreler
            if (filter.CategoryId.HasValue)
                query = query.Where(e => e.CategoryId == filter.CategoryId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(e => e.StartDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(e => e.EndDate <= filter.ToDate.Value);

            if (filter.IsFree.HasValue)
                query = query.Where(e => filter.IsFree.Value ? e.Price == 0 : e.Price > 0);

            var totalCount = await query.CountAsync();

            var events = await query
                .OrderBy(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventListDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.Name,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Location = e.Location,
                    Price = e.Price,
                    Capacity = e.Capacity,
                    RegisteredCount = e.RegisteredCount
                })
                .ToListAsync();

            var pagedResponse = new PagedResponse<EventListDto>(events, page, pageSize, totalCount);
            return Response<PagedResponse<EventListDto>>.Success(pagedResponse, 200);
        }

        public async Task<Response<EventDto>> GetEventByIdAsync(int id, string? userId = null)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.CreatedBy)
                .Include(e => e.Registrations.Where(r => r.IsActive))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return Response<EventDto>.Fail("Etkinlik bulunamadı", 404);

            var dto = new EventDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                CategoryId = ev.CategoryId,
                CategoryName = ev.Category.Name,
                CreatedByUserId = ev.CreatedByUserId,
                CreatedByName = ev.CreatedBy?.UserName ?? "Bilinmeyen",
                StartDate = ev.StartDate,
                EndDate = ev.EndDate,
                Location = ev.Location,
                Price = ev.Price,
                Capacity = ev.Capacity,
                RegisteredCount = ev.RegisteredCount,
                IsActive = ev.IsActive,
                ImageUrl = ev.ImageUrl,
                IsRegistered = userId != null && ev.Registrations.Any(r => r.UserId == userId)
            };

            return Response<EventDto>.Success(dto, 200);
        }

        public async Task<Response<EventDto>> CreateEventAsync(string organizerId, EventCreateDto dto)
        {
            // Tarih kontrolü
            if (dto.StartDate < DateTime.UtcNow)
                return Response<EventDto>.Fail("Geçmiş tarihte etkinlik oluşturulamaz", 400);

            if (dto.EndDate < dto.StartDate)
                return Response<EventDto>.Fail("Bitiş tarihi başlangıç tarihinden önce olamaz", 400);

            // Kategori kontrolü
            var categoryExists = await _context.EventCategories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
            if (!categoryExists)
                return Response<EventDto>.Fail("Geçersiz kategori", 400);

            var ev = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                CreatedByUserId = organizerId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                Price = dto.Price,
                Capacity = dto.Capacity,
                RegisteredCount = 0,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Events.AddAsync(ev);
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(ev.Id);
        }

        public async Task<Response<EventDto>> UpdateEventAsync(string organizerId, int id, EventUpdateDto dto)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.CreatedByUserId == organizerId);
            if (ev == null)
                return Response<EventDto>.Fail("Etkinlik bulunamadı veya yetkiniz yok", 404);

            if (!string.IsNullOrEmpty(dto.Title)) ev.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) ev.Description = dto.Description;
            if (dto.CategoryId.HasValue) ev.CategoryId = dto.CategoryId.Value;
            if (dto.StartDate.HasValue) ev.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) ev.EndDate = dto.EndDate.Value;
            if (!string.IsNullOrEmpty(dto.Location)) ev.Location = dto.Location;
            if (dto.Price.HasValue) ev.Price = dto.Price.Value;
            if (dto.Capacity.HasValue) ev.Capacity = dto.Capacity.Value;
            if (dto.ImageUrl != null) ev.ImageUrl = dto.ImageUrl;

            ev.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(ev.Id);
        }

        public async Task<Response<NoDataDto>> DeleteEventAsync(string organizerId, int id, bool force = false)
        {
            var ev = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id && e.CreatedByUserId == organizerId);

            if (ev == null)
                return Response<NoDataDto>.Fail("Etkinlik bulunamadı veya yetkiniz yok", 404);

            var hasRegistrations = ev.Registrations.Any(r => r.IsActive);
            if (hasRegistrations && !force)
                return Response<NoDataDto>.Fail("Bu etkinliğe kayıtlı katılımcılar var. Silmek için force=true kullanın.", 400);

            ev.IsActive = false;
            ev.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> PublishEventAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return Response<NoDataDto>.Fail("Etkinlik bulunamadı", 404);

            ev.IsActive = true;
            ev.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> CancelEventAsync(int id, string reason)
        {
            var ev = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return Response<NoDataDto>.Fail("Etkinlik bulunamadı", 404);

            ev.IsActive = false;
            ev.UpdatedDate = DateTime.UtcNow;

            // Ücretli etkinlik ise iadeleri yap
            if (ev.Price > 0)
            {
                foreach (var reg in ev.Registrations.Where(r => r.IsActive))
                {
                    await _walletService.RefundAsync(
                        reg.UserId,
                        ev.Price,
                        ReferenceType.EventRegistration,
                        reg.Id,
                        $"Etkinlik iptali: {ev.Title}");
                }
            }

            await _context.SaveChangesAsync();
            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<EventRegistrationDto>> RegisterAsync(string userId, int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Registrations)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

            if (ev == null)
                return Response<EventRegistrationDto>.Fail("Etkinlik bulunamadı veya kayıt yapılamaz", 404);

            // Zaten kayıtlı mı?
            var existingRegistration = ev.Registrations.FirstOrDefault(r => r.UserId == userId && r.IsActive);
            if (existingRegistration != null)
                return Response<EventRegistrationDto>.Fail("Bu etkinliğe zaten kayıtlısınız", 400);

            // Kapasite kontrolü
            if (ev.RegisteredCount >= ev.Capacity)
                return Response<EventRegistrationDto>.Fail("Etkinlik kapasitesi dolu. Bekleme listesine katılabilirsiniz.", 400);

            // Ücret kontrolü
            if (ev.Price > 0)
            {
                var deductResult = await _walletService.DeductAsync(
                    userId,
                    ev.Price,
                    ReferenceType.EventRegistration,
                    null,
                    $"Etkinlik kaydı: {ev.Title}");

                if (!deductResult.IsSuccessful)
                    return Response<EventRegistrationDto>.Fail(deductResult.Errors?.FirstOrDefault() ?? "Ödeme başarısız", 400);
            }

            var registration = new EventRegistration
            {
                EventId = eventId,
                UserId = userId,
                RegistrationDate = DateTime.UtcNow,
                QRCode = _qrCodeService.GenerateQRCode("EVENT", 0),
                CheckedIn = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.EventRegistrations.AddAsync(registration);
            ev.RegisteredCount++;
            await _context.SaveChangesAsync();

            // QR kodu güncelle
            registration.QRCode = _qrCodeService.GenerateQRCode("EVENT", registration.Id);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return Response<EventRegistrationDto>.Success(new EventRegistrationDto
            {
                Id = registration.Id,
                EventId = eventId,
                EventTitle = ev.Title,
                UserId = userId,
                UserName = user?.UserName ?? "Bilinmeyen",
                RegistrationDate = registration.RegistrationDate,
                QRCode = registration.QRCode,
                CheckedIn = registration.CheckedIn,
                CheckedInAt = registration.CheckedInAt
            }, 201);
        }

        public async Task<Response<NoDataDto>> CancelRegistrationAsync(string userId, int eventId)
        {
            var registration = await _context.EventRegistrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId && r.IsActive);

            if (registration == null)
                return Response<NoDataDto>.Fail("Kayıt bulunamadı", 404);

            if (registration.CheckedIn)
                return Response<NoDataDto>.Fail("Giriş yapılmış kayıtlar iptal edilemez", 400);

            // İade işlemi
            if (registration.Event.Price > 0)
            {
                await _walletService.RefundAsync(
                    userId,
                    registration.Event.Price,
                    ReferenceType.EventRegistration,
                    registration.Id,
                    $"Etkinlik kaydı iptali: {registration.Event.Title}");
            }

            registration.IsActive = false;
            registration.UpdatedDate = DateTime.UtcNow;
            registration.Event.RegisteredCount--;
            await _context.SaveChangesAsync();

            // Waitlist'ten birini kaydet (opsiyonel)
            await PromoteFromWaitlistAsync(eventId);

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<EventRegistrationDto>>> GetMyRegistrationsAsync(string userId)
        {
            var registrations = await _context.EventRegistrations
                .Include(r => r.Event)
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderByDescending(r => r.Event.StartDate)
                .Select(r => new EventRegistrationDto
                {
                    Id = r.Id,
                    EventId = r.EventId,
                    EventTitle = r.Event.Title,
                    UserId = r.UserId,
                    UserName = "",
                    RegistrationDate = r.RegistrationDate,
                    QRCode = r.QRCode,
                    CheckedIn = r.CheckedIn,
                    CheckedInAt = r.CheckedInAt
                })
                .ToListAsync();

            return Response<List<EventRegistrationDto>>.Success(registrations, 200);
        }

        public async Task<Response<EventWaitlistDto>> JoinWaitlistAsync(string userId, int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null || !ev.IsActive)
                return Response<EventWaitlistDto>.Fail("Etkinlik bulunamadı", 404);

            // Zaten kayıtlı mı?
            var existingReg = await _context.EventRegistrations.AnyAsync(r => r.EventId == eventId && r.UserId == userId && r.IsActive);
            if (existingReg)
                return Response<EventWaitlistDto>.Fail("Bu etkinliğe zaten kayıtlısınız", 400);

            // Zaten waitlist'te mi?
            var existingWaitlist = await _context.EventWaitlists.AnyAsync(w => w.EventId == eventId && w.UserId == userId && w.IsActive);
            if (existingWaitlist)
                return Response<EventWaitlistDto>.Fail("Zaten bekleme listesinde", 400);

            // Sıra numarası
            var position = await _context.EventWaitlists.CountAsync(w => w.EventId == eventId && w.IsActive) + 1;

            var waitlist = new EventWaitlist
            {
                EventId = eventId,
                UserId = userId,
                AddedAt = DateTime.UtcNow,
                QueuePosition = position,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.EventWaitlists.AddAsync(waitlist);
            await _context.SaveChangesAsync();

            return Response<EventWaitlistDto>.Success(new EventWaitlistDto
            {
                Id = waitlist.Id,
                EventId = eventId,
                EventTitle = ev.Title,
                UserId = userId,
                QueuePosition = position,
                AddedAt = waitlist.AddedAt
            }, 201);
        }

        public async Task<Response<NoDataDto>> LeaveWaitlistAsync(string userId, int eventId)
        {
            var waitlist = await _context.EventWaitlists
                .FirstOrDefaultAsync(w => w.EventId == eventId && w.UserId == userId && w.IsActive);

            if (waitlist == null)
                return Response<NoDataDto>.Fail("Bekleme listesinde değilsiniz", 404);

            waitlist.IsActive = false;
            waitlist.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Sıraları güncelle
            await UpdateWaitlistPositionsAsync(eventId);

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<EventCheckInResultDto>> CheckInAsync(string qrCode)
        {
            var registration = await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.QRCode == qrCode && r.IsActive);

            if (registration == null)
                return Response<EventCheckInResultDto>.Success(new EventCheckInResultDto
                {
                    IsValid = false,
                    Message = "Geçersiz QR kod"
                }, 200);

            if (registration.CheckedIn)
                return Response<EventCheckInResultDto>.Success(new EventCheckInResultDto
                {
                    RegistrationId = registration.Id,
                    UserName = registration.User?.UserName ?? "Bilinmeyen",
                    EventTitle = registration.Event.Title,
                    IsValid = false,
                    Message = "Giriş zaten yapılmış"
                }, 200);

            // Tarih kontrolü
            var now = DateTime.UtcNow;
            if (now < registration.Event.StartDate.AddHours(-1) || now > registration.Event.EndDate)
                return Response<EventCheckInResultDto>.Success(new EventCheckInResultDto
                {
                    RegistrationId = registration.Id,
                    UserName = registration.User?.UserName ?? "Bilinmeyen",
                    EventTitle = registration.Event.Title,
                    IsValid = false,
                    Message = "Etkinlik saati dışında giriş yapılamaz"
                }, 200);

            registration.CheckedIn = true;
            registration.CheckedInAt = DateTime.UtcNow;
            registration.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<EventCheckInResultDto>.Success(new EventCheckInResultDto
            {
                RegistrationId = registration.Id,
                UserName = registration.User?.UserName ?? "Bilinmeyen",
                EventTitle = registration.Event.Title,
                IsValid = true,
                Message = "Giriş başarılı! Hoş geldiniz."
            }, 200);
        }

        public async Task<Response<List<EventRegistrationDto>>> GetEventRegistrationsAsync(int eventId)
        {
            var registrations = await _context.EventRegistrations
                .Include(r => r.User)
                .Include(r => r.Event)
                .Where(r => r.EventId == eventId && r.IsActive)
                .OrderBy(r => r.RegistrationDate)
                .Select(r => new EventRegistrationDto
                {
                    Id = r.Id,
                    EventId = r.EventId,
                    EventTitle = r.Event.Title,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.UserName : "Bilinmeyen",
                    RegistrationDate = r.RegistrationDate,
                    QRCode = r.QRCode,
                    CheckedIn = r.CheckedIn,
                    CheckedInAt = r.CheckedInAt
                })
                .ToListAsync();

            return Response<List<EventRegistrationDto>>.Success(registrations, 200);
        }

        private async Task PromoteFromWaitlistAsync(int eventId)
        {
            var nextInLine = await _context.EventWaitlists
                .Where(w => w.EventId == eventId && w.IsActive)
                .OrderBy(w => w.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextInLine != null)
            {
                // Otomatik kayıt veya notification gönderimi
                // Şimdilik placeholder
            }
        }

        private async Task UpdateWaitlistPositionsAsync(int eventId)
        {
            var waitlistItems = await _context.EventWaitlists
                .Where(w => w.EventId == eventId && w.IsActive)
                .OrderBy(w => w.AddedAt)
                .ToListAsync();

            for (int i = 0; i < waitlistItems.Count; i++)
            {
                waitlistItems[i].QueuePosition = i + 1;
            }

            await _context.SaveChangesAsync();
        }
    }
}
