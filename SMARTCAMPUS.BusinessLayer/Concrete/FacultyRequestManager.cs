using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.FacultyRequest;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class FacultyRequestManager : IFacultyRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FacultyRequestManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<IEnumerable<AvailableSectionDto>>> GetAvailableSectionsAsync(int facultyId)
        {
            // Akademisyenin bölümünü bul
            var faculty = await _unitOfWork.Faculties.GetByIdAsync(facultyId);
            if (faculty == null)
                return Response<IEnumerable<AvailableSectionDto>>.Fail("Akademisyen bulunamadı", 404);

            // Bölümdeki tüm aktif section'ları getir
            var sections = await _unitOfWork.CourseSections.GetSectionsByDepartmentAsync(faculty.DepartmentId);
            
            // Akademisyenin mevcut isteklerini al
            var myRequests = await _unitOfWork.FacultyRequests.GetRequestsByFacultyAsync(facultyId);
            var pendingRequestSectionIds = myRequests
                .Where(r => r.Status == "Pending")
                .Select(r => r.SectionId)
                .ToHashSet();
            var approvedRequestSectionIds = myRequests
                .Where(r => r.Status == "Approved")
                .Select(r => r.SectionId)
                .ToHashSet();

            var result = sections.Select(s => new AvailableSectionDto
            {
                SectionId = s.Id,
                CourseId = s.CourseId,
                CourseCode = s.Course.Code,
                CourseName = s.Course.Name,
                SectionNumber = s.SectionNumber,
                Semester = s.Semester,
                Year = s.Year,
                Capacity = s.Capacity,
                AlreadyRequested = pendingRequestSectionIds.Contains(s.Id),
                AlreadyAssigned = s.InstructorId == facultyId || approvedRequestSectionIds.Contains(s.Id)
            }).ToList();

            return Response<IEnumerable<AvailableSectionDto>>.Success(result, 200);
        }

        public async Task<Response<FacultyRequestDto>> RequestSectionAsync(int facultyId, CreateFacultyRequestDto dto)
        {
            // Akademisyeni kontrol et
            var faculty = await _unitOfWork.Faculties.GetFacultyWithUserAsync(facultyId);
            if (faculty == null)
                return Response<FacultyRequestDto>.Fail("Akademisyen bulunamadı", 404);

            // Section'ı kontrol et
            var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(dto.SectionId);
            if (section == null)
                return Response<FacultyRequestDto>.Fail("Section bulunamadı", 404);

            // Bölüm kontrolü
            if (section.Course.DepartmentId != faculty.DepartmentId)
                return Response<FacultyRequestDto>.Fail("Bu ders bölümünüze ait değil", 403);

            // Zaten bekleyen istek var mı?
            var hasPending = await _unitOfWork.FacultyRequests.HasPendingRequestAsync(facultyId, dto.SectionId);
            if (hasPending)
                return Response<FacultyRequestDto>.Fail("Bu ders için zaten bekleyen bir isteğiniz var", 400);

            // Zaten bu derse atanmış mı?
            if (section.InstructorId == facultyId)
                return Response<FacultyRequestDto>.Fail("Bu derse zaten atanmışsınız", 400);

            // İstek oluştur
            var request = new FacultyCourseSectionRequest
            {
                FacultyId = facultyId,
                SectionId = dto.SectionId,
                Status = "Pending",
                RequestDate = DateTime.UtcNow
            };

            await _unitOfWork.FacultyRequests.AddAsync(request);
            await _unitOfWork.CommitAsync();

            var resultDto = new FacultyRequestDto
            {
                Id = request.Id,
                FacultyId = facultyId,
                FacultyName = faculty.User.FullName,
                FacultyTitle = faculty.Title,
                FacultyEmail = faculty.User.Email ?? "",
                DepartmentName = faculty.Department?.Name ?? "",
                SectionId = dto.SectionId,
                CourseCode = section.Course.Code,
                CourseName = section.Course.Name,
                SectionNumber = section.SectionNumber,
                Status = "Pending",
                RequestDate = request.RequestDate
            };

            return Response<FacultyRequestDto>.Success(resultDto, 201);
        }

        public async Task<Response<IEnumerable<FacultyRequestDto>>> GetMyRequestsAsync(int facultyId)
        {
            var requests = await _unitOfWork.FacultyRequests.GetRequestsByFacultyAsync(facultyId);

            var result = requests.Select(r => new FacultyRequestDto
            {
                Id = r.Id,
                FacultyId = r.FacultyId,
                SectionId = r.SectionId,
                CourseCode = r.Section.Course.Code,
                CourseName = r.Section.Course.Name,
                SectionNumber = r.Section.SectionNumber,
                Status = r.Status,
                RequestDate = r.RequestDate,
                ResponseDate = r.ResponseDate,
                AdminNote = r.AdminNote
            }).ToList();

            return Response<IEnumerable<FacultyRequestDto>>.Success(result, 200);
        }

        public async Task<Response<IEnumerable<FacultyRequestDto>>> GetAllPendingRequestsAsync()
        {
            var requests = await _unitOfWork.FacultyRequests.GetPendingRequestsAsync();

            var result = requests.Select(r => new FacultyRequestDto
            {
                Id = r.Id,
                FacultyId = r.FacultyId,
                FacultyName = r.Faculty.User.FullName,
                FacultyTitle = r.Faculty.Title,
                FacultyEmail = r.Faculty.User.Email ?? "",
                DepartmentName = r.Faculty.Department?.Name ?? "",
                SectionId = r.SectionId,
                CourseCode = r.Section.Course.Code,
                CourseName = r.Section.Course.Name,
                SectionNumber = r.Section.SectionNumber,
                Status = r.Status,
                RequestDate = r.RequestDate
            }).ToList();

            return Response<IEnumerable<FacultyRequestDto>>.Success(result, 200);
        }

        public async Task<Response<NoDataDto>> ApproveRequestAsync(int requestId, string adminId, string? note)
        {
            var request = await _unitOfWork.FacultyRequests.GetRequestWithDetailsAsync(requestId);
            if (request == null)
                return Response<NoDataDto>.Fail("İstek bulunamadı", 404);

            if (request.Status != "Pending")
                return Response<NoDataDto>.Fail("Bu istek zaten işlenmiş", 400);

            // Section'a instructor'ı ata
            var section = await _unitOfWork.CourseSections.GetByIdAsync(request.SectionId);
            if (section != null)
            {
                section.InstructorId = request.FacultyId;
                _unitOfWork.CourseSections.Update(section);
            }

            // İsteği onayla
            request.Status = "Approved";
            request.ResponseDate = DateTime.UtcNow;
            request.AdminNote = note;
            request.ProcessedByAdminId = adminId;

            _unitOfWork.FacultyRequests.Update(request);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> RejectRequestAsync(int requestId, string adminId, string? note)
        {
            var request = await _unitOfWork.FacultyRequests.GetRequestWithDetailsAsync(requestId);
            if (request == null)
                return Response<NoDataDto>.Fail("İstek bulunamadı", 404);

            if (request.Status != "Pending")
                return Response<NoDataDto>.Fail("Bu istek zaten işlenmiş", 400);

            // İsteği reddet
            request.Status = "Rejected";
            request.ResponseDate = DateTime.UtcNow;
            request.AdminNote = note;
            request.ProcessedByAdminId = adminId;

            _unitOfWork.FacultyRequests.Update(request);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
