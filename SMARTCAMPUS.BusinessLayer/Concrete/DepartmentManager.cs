using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class DepartmentManager : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<Department>>> GetDepartmentsAsync()
        {
             var departments = await _unitOfWork.Departments.GetAll().ToListAsync();
             return Response<List<Department>>.Success(departments, 200);
        }
    }
}
