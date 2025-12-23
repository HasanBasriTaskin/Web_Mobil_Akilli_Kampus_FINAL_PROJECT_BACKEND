using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EventManager : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQRCodeService _qrCodeService;
        private readonly IWalletService _walletService;
        private readonly UserManager<User> _userManager;

        public EventManager(IUnitOfWork unitOfWork, IQRCodeService qrCodeService, IWalletService walletService, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _qrCodeService = qrCodeService;
            _walletService = walletService;
            _userManager = userManager;
        }

        public async Task<Response<PagedResponse<EventListDto>>> GetEventsAsync(EventFilterDto filter, int page = 1, int pageSize = 20)
        {
            var totalCount = await _unitOfWork.Events.GetEventsCountAsync(filter.CategoryId, filter.FromDate, filter.ToDate, filter.IsFree, null);
            var events = await _unitOfWork.Events.GetEventsFilteredAsync(filter.CategoryId, filter.FromDate, filter.ToDate, filter.IsFree, null, page, pageSize);

            var dtos = events.Select(e => new EventListDto
            {
                Id = e.Id,
                Title = e.Title,
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name ?? "Bilinmeyen",
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Location = e.Location,
                Price = e.Price,
                Capacity = e.Capacity,
                RegisteredCount = e.RegisteredCount
            }).ToList();

            var pagedResponse = new PagedResponse<EventListDto>(dtos, page, pageSize, totalCount);
            return Response<PagedResponse<EventListDto>>.Success(pagedResponse, 200);
        }

        public async Task<Response<EventDto>> GetEventByIdAsync(int id, string? userId = null)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);
            if (ev == null)
                return Response<EventDto>.Fail("Etkinlik bulunamadı", 404);

            var dto = new EventDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                CategoryId = ev.CategoryId,
                CategoryName = ev.Category?.Name ?? "Bilinmeyen",
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
                IsRegistered = userId != null && ev.Registrations.Any(r => r.UserId == userId && r.IsActive)
            };

            return Response<EventDto>.Success(dto, 200);
        }

        public async Task<Response<EventDto>> CreateEventAsync(string organizerId, EventCreateDto dto)
        {
            if (dto.StartDate < DateTime.UtcNow)
                return Response<EventDto>.Fail("Geçmiş tarihte etkinlik oluşturulamaz", 400);

            if (dto.EndDate < dto.StartDate)
                return Response<EventDto>.Fail("Bitiş tarihi başlangıç tarihinden önce olamaz", 400);

            var category = await _unitOfWork.EventCategories.GetByIdAsync(dto.CategoryId);
            if (category == null || !category.IsActive)
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

            await _unitOfWork.Events.AddAsync(ev);
            await _unitOfWork.CommitAsync();

            return await GetEventByIdAsync(ev.Id);
        }

        public async Task<Response<EventDto>> UpdateEventAsync(string organizerId, int id, EventUpdateDto dto)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null || ev.CreatedByUserId != organizerId)
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
            _unitOfWork.Events.Update(ev);
            await _unitOfWork.CommitAsync();

            return await GetEventByIdAsync(ev.Id);
        }

        public async Task<Response<NoDataDto>> DeleteEventAsync(string organizerId, int id, bool force = false)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);
            if (ev == null || ev.CreatedByUserId != organizerId)
                return Response<NoDataDto>.Fail("Etkinlik bulunamadı veya yetkiniz yok", 404);

            var hasRegistrations = ev.Registrations.Any(r => r.IsActive);
            if (hasRegistrations && !force)
                return Response<NoDataDto>.Fail("Bu etkinliğe kayıtlı katılımcılar var. Silmek için force=true kullanın.", 400);

            ev.IsActive = false;
            ev.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Events.Update(ev);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> PublishEventAsync(int id)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null)
                return Response<NoDataDto>.Fail("Etkinlik bulunamadı", 404);

            ev.IsActive = true;
            ev.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Events.Update(ev);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> CancelEventAsync(int id, string reason)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);
            if (ev == null)
                return Response<NoDataDto>.Fail("Etkinlik bulunamadı", 404);

            ev.IsActive = false;
            ev.UpdatedDate = DateTime.UtcNow;

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

            _unitOfWork.Events.Update(ev);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<EventRegistrationDto>> RegisterAsync(string userId, int eventId)
        {
            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId);
                if (ev == null || !ev.IsActive)
                    return Response<EventRegistrationDto>.Fail("Etkinlik bulunamadı veya kayıt yapılamaz", 404);

                var existingReg = await _unitOfWork.EventRegistrations.IsUserRegisteredAsync(eventId, userId);
                if (existingReg)
                    return Response<EventRegistrationDto>.Fail("Bu etkinliğe zaten kayıtlısınız", 400);

                if (ev.RegisteredCount >= ev.Capacity)
                    return Response<EventRegistrationDto>.Fail("Etkinlik kapasitesi dolu. Bekleme listesine katılabilirsiniz.", 400);

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

                try
                {
                    await _unitOfWork.EventRegistrations.AddAsync(registration);
                    ev.RegisteredCount++;
                    _unitOfWork.Events.Update(ev);
                    await _unitOfWork.CommitAsync();

                    // Kayıt başarılı, QR kodu güncelle
                    registration.QRCode = _qrCodeService.GenerateQRCode("EVENT", registration.Id);
                    _unitOfWork.EventRegistrations.Update(registration);
                    await _unitOfWork.CommitAsync();

                    var user = await _userManager.FindByIdAsync(userId);

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
                catch (DbUpdateConcurrencyException)
                {
                    // Optimistic Locking çakışması - yeniden dene
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        // Ödeme yapıldıysa iade et
                        if (ev.Price > 0)
                        {
                            await _walletService.RefundAsync(
                                userId,
                                ev.Price,
                                ReferenceType.EventRegistration,
                                null,
                                $"Etkinlik kaydı başarısız (eşzamanlılık hatası): {ev.Title}");
                        }
                        return Response<EventRegistrationDto>.Fail("Yoğun talep nedeniyle kayıt yapılamadı. Lütfen tekrar deneyin.", 409);
                    }
                    // Kısa bekleme ve yeniden deneme
                    await Task.Delay(100 * retryCount);
                }
            }

            return Response<EventRegistrationDto>.Fail("Kayıt işlemi başarısız", 500);
        }

        public async Task<Response<NoDataDto>> CancelRegistrationAsync(string userId, int eventId)
        {
            var registration = await _unitOfWork.EventRegistrations.GetByEventAndUserAsync(eventId, userId);
            if (registration == null)
                return Response<NoDataDto>.Fail("Kayıt bulunamadı", 404);

            if (registration.CheckedIn)
                return Response<NoDataDto>.Fail("Giriş yapılmış kayıtlar iptal edilemez", 400);

            var ev = await _unitOfWork.Events.GetByIdAsync(eventId);

            if (ev != null && ev.Price > 0)
            {
                await _walletService.RefundAsync(
                    userId,
                    ev.Price,
                    ReferenceType.EventRegistration,
                    registration.Id,
                    $"Etkinlik kaydı iptali: {ev.Title}");
            }

            registration.IsActive = false;
            registration.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.EventRegistrations.Update(registration);

            if (ev != null)
            {
                ev.RegisteredCount--;
                _unitOfWork.Events.Update(ev);
            }

            await _unitOfWork.CommitAsync();
            await PromoteFromWaitlistAsync(eventId);

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<EventRegistrationDto>>> GetMyRegistrationsAsync(string userId)
        {
            var registrations = await _unitOfWork.EventRegistrations.GetByUserIdAsync(userId);

            var dtos = registrations.Select(r => new EventRegistrationDto
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event?.Title ?? "Bilinmeyen",
                UserId = r.UserId,
                UserName = "",
                RegistrationDate = r.RegistrationDate,
                QRCode = r.QRCode,
                CheckedIn = r.CheckedIn,
                CheckedInAt = r.CheckedInAt
            }).ToList();

            return Response<List<EventRegistrationDto>>.Success(dtos, 200);
        }

        public async Task<Response<EventWaitlistDto>> JoinWaitlistAsync(string userId, int eventId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (ev == null || !ev.IsActive)
                return Response<EventWaitlistDto>.Fail("Etkinlik bulunamadı", 404);

            var existingReg = await _unitOfWork.EventRegistrations.IsUserRegisteredAsync(eventId, userId);
            if (existingReg)
                return Response<EventWaitlistDto>.Fail("Bu etkinliğe zaten kayıtlısınız", 400);

            var existingWaitlist = await _unitOfWork.EventWaitlists.IsUserInWaitlistAsync(eventId, userId);
            if (existingWaitlist)
                return Response<EventWaitlistDto>.Fail("Zaten bekleme listesindesiniz", 400);

            var position = await _unitOfWork.EventWaitlists.GetMaxPositionAsync(eventId) + 1;

            var waitlist = new EventWaitlist
            {
                EventId = eventId,
                UserId = userId,
                AddedAt = DateTime.UtcNow,
                QueuePosition = position,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.EventWaitlists.AddAsync(waitlist);
            await _unitOfWork.CommitAsync();

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
            var waitlist = await _unitOfWork.EventWaitlists.GetByEventAndUserAsync(eventId, userId);
            if (waitlist == null)
                return Response<NoDataDto>.Fail("Bekleme listesinde değilsiniz", 404);

            waitlist.IsActive = false;
            waitlist.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.EventWaitlists.Update(waitlist);
            await _unitOfWork.CommitAsync();

            await UpdateWaitlistPositionsAsync(eventId);

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<EventCheckInResultDto>> CheckInAsync(string qrCode)
        {
            var registration = await _unitOfWork.EventRegistrations.GetByQRCodeAsync(qrCode);
            if (registration == null || !registration.IsActive)
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
                    EventTitle = registration.Event?.Title ?? "Bilinmeyen",
                    IsValid = false,
                    Message = "Giriş zaten yapılmış"
                }, 200);

            var now = DateTime.UtcNow;
            if (registration.Event != null && (now < registration.Event.StartDate.AddHours(-1) || now > registration.Event.EndDate))
                return Response<EventCheckInResultDto>.Success(new EventCheckInResultDto
                {
                    RegistrationId = registration.Id,
                    UserName = registration.User?.UserName ?? "Bilinmeyen",
                    EventTitle = registration.Event?.Title ?? "Bilinmeyen",
                    IsValid = false,
                    Message = "Etkinlik saati dışında giriş yapılamaz"
                }, 200);

            registration.CheckedIn = true;
            registration.CheckedInAt = DateTime.UtcNow;
            registration.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.EventRegistrations.Update(registration);
            await _unitOfWork.CommitAsync();

            return Response<EventCheckInResultDto>.Success(new EventCheckInResultDto
            {
                RegistrationId = registration.Id,
                UserName = registration.User?.UserName ?? "Bilinmeyen",
                EventTitle = registration.Event?.Title ?? "Bilinmeyen",
                IsValid = true,
                Message = "Giriş başarılı! Hoş geldiniz."
            }, 200);
        }

        public async Task<Response<List<EventRegistrationDto>>> GetEventRegistrationsAsync(int eventId)
        {
            var registrations = await _unitOfWork.EventRegistrations.GetByEventIdAsync(eventId);

            var dtos = registrations.Select(r => new EventRegistrationDto
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event?.Title ?? "Bilinmeyen",
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "Bilinmeyen",
                RegistrationDate = r.RegistrationDate,
                QRCode = r.QRCode,
                CheckedIn = r.CheckedIn,
                CheckedInAt = r.CheckedInAt
            }).ToList();

            return Response<List<EventRegistrationDto>>.Success(dtos, 200);
        }

        private async Task PromoteFromWaitlistAsync(int eventId)
        {
            var nextInLine = await _unitOfWork.EventWaitlists.GetNextInQueueAsync(eventId);
            if (nextInLine != null)
            {
                // Notification gönderimi için işaretleme
                nextInLine.IsNotified = true;
                _unitOfWork.EventWaitlists.Update(nextInLine);
                await _unitOfWork.CommitAsync();
            }
        }

        private async Task UpdateWaitlistPositionsAsync(int eventId)
        {
            var waitlistItems = await _unitOfWork.EventWaitlists.GetByEventIdAsync(eventId);
            int pos = 1;
            foreach (var item in waitlistItems.OrderBy(w => w.AddedAt))
            {
                item.QueuePosition = pos++;
                _unitOfWork.EventWaitlists.Update(item);
            }
            await _unitOfWork.CommitAsync();
        }
    }
}
