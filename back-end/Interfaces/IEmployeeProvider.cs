using System.Collections.Generic;
using SmartBoardContracts.Models.CreateViewModels.Employee;
using SmartBoardContracts.Models.ViewModels.Employee;
using SmartBoardContracts.Models.Validation;

namespace SmartBoardApplication.Interfaces.Providers
{
    public interface IEmployeeProvider
    {
        IEnumerable<EmployeeViewModel> Get();
        int Add(EmployeeCreateViewModel employee);
        int Update(EmployeeCreateViewModel model);
        int SelfUpdate(EmployeeCreateViewModel employee); // employee updates his profile
        bool SetProfilePhoto(string uploadedPhoroUri, string photoUri, int employeeId);
        IEnumerable<int> GetProjectsIds(int employeeId);
        EmployeeViewModel Get(int employeeId);
        EmployeeViewModel GetEmployeesManager(int employeeId);
        IEnumerable<EmployeeViewModel> GetManagers(int employeeId);
        IEnumerable<EmployeeViewModel> GetSubordinates(int employeeId);

        bool Remove(int employeeId);
        bool Erase();
        string GetIdByUserId(string userId);

        string IfNameExist(EmployeeCheckNameModel model);
    }
}
