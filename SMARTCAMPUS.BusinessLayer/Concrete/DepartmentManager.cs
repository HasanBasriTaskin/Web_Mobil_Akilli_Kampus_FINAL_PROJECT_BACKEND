using AutoMapper;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class DepartmentManager : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentManager(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<DepartmentDto>>> GetDepartmentsAsync()
        {
            try
            {
                var departments = await _unitOfWork.Departments.GetAllAsync();
                var departmentDtos = _mapper.Map<List<DepartmentDto>>(departments);
                return Response<List<DepartmentDto>>.Success(departmentDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<List<DepartmentDto>>.Fail($"Error retrieving departments: {ex.Message}", 500);
            }
        }
    }
}
