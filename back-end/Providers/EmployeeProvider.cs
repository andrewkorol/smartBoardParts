using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SmartBoardApplication.Interfaces.Providers;
using SmartBoardContracts.Interfaces;
using SmartBoardContracts.Interfaces.Service.Mappings;
using SmartBoardContracts.Models.CreateViewModels.Employee;
using SmartBoardContracts.Models.ViewModels.Employee;
using SmartBoardContracts.Models.ViewModels.Project;
using SmartBoardData.Interfaces.Repositories;
using SmartBoardDomain.Models.Employee;
using SmartBoardContracts.Models.Validation;

namespace SmartBoardApplication.Providers
{
    public class EmployeeProvider : IEmployeeProvider
    {
        private readonly IEmployeeContactsProvider _employeeContactsProvider;
        private readonly IEmployeeSkillRepository _employeeSkillRepository; 
        private readonly IEmployeeLanguageProvider _employeeLanguageProvider;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ISkillProvider _skillProvider;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeMappingServiceProvider _mappingServiceProvider;

        private readonly IPositionProvider _positionServiceProvider;
        private readonly IProjectMappingServiceProvider _projectMappingServiceProvider;
        private readonly IVacationProvider _vacationServiceProvider;
        private readonly IEmployeeProjectAssignmentRepository _employeeProjectAssignmentRepository;
        private readonly IEmployeeProjectAssignmentMappingServiceProvider _employeeProjectAssignmentMappingServiceProvider;

        public EmployeeProvider(IUnitOfWork uow,
                                                IProjectRepository projRepo,
                                                IEmployeeRepository employeeRepo,
                                                IEmployeeMappingServiceProvider mappingServiceProvider,
 
                                                IPositionProvider positionServiceProvider,
                                                IProjectMappingServiceProvider projectManagementServiceProvider,
                                                IVacationProvider vacationServiceProvider,
                                                IEmployeeSkillRepository employeeSkillRepository,
                                                IEmployeeProjectAssignmentRepository employeeProjectAssignmentRepository,
                                                IEmployeeProjectAssignmentMappingServiceProvider employeeProjectAssignmentMappingServiceProvider,
                                                IEmployeeLanguageProvider employeeLanguageProvider, 
                                                IEmployeeContactsProvider employeeContactsProvider, 
                                                ISkillProvider skillProvider)
        {
            _employeeRepository = employeeRepo;
            _projectRepository = projRepo;
            _unitOfWork = uow;
            _mappingServiceProvider = mappingServiceProvider;

            _positionServiceProvider = positionServiceProvider;
            _projectMappingServiceProvider = projectManagementServiceProvider;
            _vacationServiceProvider = vacationServiceProvider;
            _employeeSkillRepository = employeeSkillRepository;
            _employeeProjectAssignmentRepository = employeeProjectAssignmentRepository;
            _employeeProjectAssignmentMappingServiceProvider = employeeProjectAssignmentMappingServiceProvider;
            _employeeLanguageProvider = employeeLanguageProvider;
            _employeeContactsProvider = employeeContactsProvider;
            _skillProvider = skillProvider;
        }

        public int Add(EmployeeCreateViewModel employeeCreateViewModel)
        {
            Employee resultEmployee = null;
            Employee emp = _mappingServiceProvider.MapToSourceModel(employeeCreateViewModel);
            
            if (emp.Id == 0)
            {
                emp.VacationInfo = new EmployeeVacationInfo()
                {
                    DaysLeft = 0,
                    HoursPaidPerMonthByCompany = employeeCreateViewModel.VacationHoursPaidPerYearByCompany,
                    DaysDroppedOn = 0,
                    DaysEncouraged = 0,
                    IsFullTime = employeeCreateViewModel.IsFullTime
                };
                foreach (EmployeeSkill employeeSkill in emp.Skills)
                {
                    if (_skillProvider.IsSkillExist(employeeSkill.Skill))
                    {
                        employeeSkill.Skill = null;
                    }
                }
            }
            else
            {
                // update VacationPaid property of already exists VacationInfo object
                emp.VacationInfo = _vacationServiceProvider.GetEmployeeVacationInfo(emp.Id);
                if (emp.VacationInfo == null)
                {
                    throw new InvalidOperationException("check entity configuration, employee Vacation info is missing");
                }
                emp.VacationInfo.HoursPaidPerMonthByCompany = employeeCreateViewModel.VacationHoursPaidPerYearByCompany;
                emp.VacationInfo.IsFullTime = employeeCreateViewModel.IsFullTime;
            }

            resultEmployee = emp.Id == 0 ? _employeeRepository.Insert(emp) : _employeeRepository.Update(emp);

            if (resultEmployee != null)
            {
                _unitOfWork.SaveChanges();
            }

            return resultEmployee.Id;
        }

        public int Update(EmployeeCreateViewModel model)
        {
            Employee employee = new Employee()
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                BirthDate = Convert.ToDateTime(model.BirthDate),
                EmploymentDate = Convert.ToDateTime(model.EmploymentDate)
                
            };
            Employee updatedEmployee = _employeeRepository.Update(employee);
            EmployeeVacationInfo vacationInfo = _vacationServiceProvider.GetEmployeeVacationInfo(updatedEmployee.Id);
            vacationInfo.IsFullTime = model.IsFullTime;
            vacationInfo.HoursPaidPerMonthByCompany = model.VacationHoursPaidPerYearByCompany;
            EmployeeVacationInfo updatedVacationInfo = _vacationServiceProvider.UpdateVacationInfo(vacationInfo);

            if (updatedEmployee != null && updatedVacationInfo != null)
            {
                _unitOfWork.SaveChanges();
            }

            return updatedEmployee.Id;
        }

        public IEnumerable<EmployeeViewModel> Get()
        {
            IEnumerable<Employee> employees = _employeeRepository.Get();
            List<EmployeeViewModel> employeesViewModels = new List<EmployeeViewModel>();

            foreach (var employee in employees)
            {
                EmployeeViewModel employeeViewModel = _mappingServiceProvider.MapToViewModel(employee);
                FulFillEmployeeViewModel(ref employeeViewModel);
                employeesViewModels.Add(employeeViewModel);
            }

            return employeesViewModels;
        }

        public EmployeeViewModel Get(int employeeId)
        {
            var employee = _employeeRepository.Get(employeeId);

            if (employee == null)
            {
                return null;
            }

            EmployeeViewModel employeeViewModel = _mappingServiceProvider.MapToViewModel(employee);
            FulFillEmployeeViewModel(ref employeeViewModel);

            return employeeViewModel;
        }

        public string GetIdByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            Employee result = _employeeRepository.GetByUserID(userId);
            return result == null ? string.Empty : result.Id.ToString();
        }

        public IEnumerable<EmployeeViewModel> GetManagers(int employeeId)
        {
            return _employeeRepository.GetManagers(employeeId).Select(x => _mappingServiceProvider.MapToViewModel(x));
        }

        public IEnumerable<EmployeeViewModel> GetSubordinates(int employeeId)
        {
            return _employeeRepository.GetManagerSubordinates(employeeId).Select(x => _mappingServiceProvider.MapToViewModel(x));
        }

        public EmployeeViewModel GetEmployeesManager(int employeeId)
        {
            var employee = _employeeRepository.GetEmployeesManager(employeeId);

            if (employee == null)
            {
                return null;
            }

            return _mappingServiceProvider.MapToViewModel(employee);
        }

        public bool Remove(int employeeId)
        {
            var result = _employeeRepository.Delete(employeeId);
            if (result)
            {
                _unitOfWork.SaveChanges();
            }
            return result;
        }

        public bool Erase()
        {
            try
            {
                _employeeRepository.Erase();
                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception) { return false; }

        }

        public IEnumerable<int> GetProjectsIds(int employeeId)
        {
            return employeeId <= 0 ? new List<int>() : _projectRepository.GetEmployeeProjectsIds(employeeId);
        }

        public int SelfUpdate(EmployeeCreateViewModel employee)
        {
            Employee employeeToSelfUpdate = _mappingServiceProvider.MapToSourceModel(employee);
            Employee resultEmployee = _employeeRepository.SelfUpdate(employeeToSelfUpdate);
            if (resultEmployee != null)
            {
                _unitOfWork.SaveChanges();
            }

            return resultEmployee.Id;
        }

		public bool SetProfilePhoto(string uploadedPhoroUri, string photoUri, int employeeId)
		{
			Employee employee = _employeeRepository.Get(employeeId);
			if (employee == null)
			{
				return false;
			}
			try
			{
				string generatedPhotoUri = photoUri + "/" + GeneratePhotoUri(employee);
				if (File.Exists(generatedPhotoUri))
				{
					File.Delete(generatedPhotoUri);
				}
				File.Move(uploadedPhoroUri, generatedPhotoUri); // renaming uploaded file 	
				employee.PhotoUri = generatedPhotoUri;
                _unitOfWork.SaveChanges();
				return true;
			}
			catch (Exception)
			{
				return false;
			}

		}

        public string IfNameExist(EmployeeCheckNameModel model)
        {
            int lastNameEndCounter = _employeeRepository.NameCount(model.FirstName, model.LastName);

            return lastNameEndCounter == 0 ? model.LastName : model.LastName + lastNameEndCounter;
        }

        #region Private Methods

        private void FulFillEmployeeViewModel(ref EmployeeViewModel employeeViewModel)
        {
            employeeViewModel.Positions = _positionServiceProvider.GetByEmployeeId(employeeViewModel.Id);
            employeeViewModel.Skills = _employeeSkillRepository.GetViewModelByEmployeeId(employeeViewModel.Id);
            employeeViewModel.Contacts = _employeeContactsProvider.GetContacts(employeeViewModel.Id);
            employeeViewModel.Languages = _employeeLanguageProvider.GetEmployeeLanguages(employeeViewModel.Id);
            SetPreviousAndCurrentProjects(ref employeeViewModel, _projectRepository.GetEmployeeProjectsViewModel(employeeViewModel.Id), _employeeProjectAssignmentRepository.GetByEmployeeId(employeeViewModel.Id));
            SetCurrentPosition(ref employeeViewModel);
            SetVacationHoursPaidByCompany(ref employeeViewModel);
            SetIsFullTime(ref employeeViewModel);
        }

        private void SetCurrentPosition(ref EmployeeViewModel employee)
        {
            employee.EmployeePosition = employee.Positions.FirstOrDefault(x => x.EndDate == null);
        }

        private void SetVacationHoursPaidByCompany(ref EmployeeViewModel employee)
        {
            employee.VacationHoursPaidPerYearByCompany = _vacationServiceProvider.GetEmployeeVacationInfo(employee.Id).HoursPaidPerMonthByCompany;
        }

        private void SetIsFullTime(ref EmployeeViewModel employee)
        {
            employee.IsFullTime = _vacationServiceProvider.GetEmployeeVacationInfo(employee.Id).IsFullTime;
        }

        private void SetPreviousAndCurrentProjects(ref EmployeeViewModel employeeViewModel, IEnumerable<ProjectViewModel> projects, IEnumerable<EmployeeProjectAssignment> projectAssignments)
        {
            List<EmployeeAssignmentsViewModel> currentAssignments = new List<EmployeeAssignmentsViewModel>();
            List<EmployeeAssignmentsViewModel> previousAssignments = new List<EmployeeAssignmentsViewModel>();

            foreach (var project in projects)
            {
                IEnumerable<EmployeeProjectAssignment> employeeAssignments = projectAssignments.Where(x => x.ProjectId == project.Id).ToList();
                EmployeeAssignmentsViewModel employeeAssignmentViewModel = new EmployeeAssignmentsViewModel()
                {
                    EndDate = project.EndDate,
                    StartDate = project.StartDate,
                    ProjectId = project.Id,
                    ProjectTitle = project.Title,
                    Assignments = employeeAssignments.Select(_employeeProjectAssignmentMappingServiceProvider.MapToViewModel)
                };
                if (employeeAssignments.Any(x => !x.EndDate.HasValue))
                {
                    currentAssignments.Add(employeeAssignmentViewModel);
                }
                else
                {
                    previousAssignments.Add(employeeAssignmentViewModel);
                }
            }
            employeeViewModel.Projects.Current = currentAssignments;
            employeeViewModel.Projects.Previous = previousAssignments;
        }

	    private string GeneratePhotoUri(Employee employee)
	    {
			StringBuilder str = new StringBuilder();
			return str.AppendFormat("{0}_{1}.jpg", employee.Id.ToString(), employee.FirstName[0] + employee.LastName).ToString();
			/*
			 Employee:
			 * Boris Malaichik, id = 3
			 * output = 3_BMalaichik.jpg
			 */
	    }

        #endregion

    }
}
