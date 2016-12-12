using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using SmartBoardContracts.Models.CreateViewModels;
using System.Web.Http.Results;
using SmartBoardApplication.Interfaces;
using SmartBoardApplication.Interfaces.Providers;
using SmartBoardContext;
using SmartBoardContext.Configuration.CustomSections.OAuth;
using SmartBoardContracts.Models.CreateViewModels.Employee;
using SmartBoardContracts.Models.ViewModels.Employee;
using SmartBoardDomain.Models;
using SmartBoardDomain.Models.Employee;
using SmartBoardContracts.Models.Validation;

namespace SmartBoardWebApi.Controllers
{
    [RoutePrefix("api/employees")]
    public class EmployeeController : ApiController
    {
        private readonly IEmployeeProvider _employeeServiceProvider;
        private readonly IEmployeeStatisticProvider _employeeStatisticProvider;
        private readonly IEmployeeContactsProvider _employeeContactsProvider;

        public EmployeeController(IEmployeeProvider employeeServiceProvider,
                                IEmployeeContactsProvider employeeContactsProvider, 
                                IEmployeeStatisticProvider employeeStatisticProvider)
        {
            _employeeServiceProvider = employeeServiceProvider;
            _employeeContactsProvider = employeeContactsProvider;
            _employeeStatisticProvider = employeeStatisticProvider;
        }

        #region Get methods
        [HttpGet]
        [Authorize]
        [Route("")]
        public IHttpActionResult Get()
        {
            if (!User.IsInRole(Roles.Admin.ToString()) && !User.IsInRole(Roles.Manager.ToString()))
            {
                return ThrowForbidden();
            }
            IEnumerable<EmployeeViewModel> res = this._employeeServiceProvider.Get();

            return Ok<IEnumerable<EmployeeViewModel>>(res);
        }

        [HttpGet]
        [Authorize]
        [Route("id/{userId}")]
        public IHttpActionResult GetIdByUserId(string userId)
        {
            string result = _employeeServiceProvider.GetIdByUserId(userId);

            if (result != null)
            {
                return Ok<string>(result);
            }

            return BadRequest("please, provide correct UserId");
        }

        [HttpGet]
        [Authorize]
        [Route("{id:int}/projects/ids")]
        public IHttpActionResult GetEmployeeProjectIds(int id)
        {
            return Ok<IEnumerable<int>>(_employeeServiceProvider.GetProjectsIds(id));
        }


        [HttpGet]
        [Authorize]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            EmployeeViewModel result = _employeeServiceProvider.Get(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok<EmployeeViewModel>(result);
        }


        [Route("statistic")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetEmployeeStatictic()
        {
            EmployeeStatisticViewModel statistic = _employeeStatisticProvider.GetStatistic();
            return Ok(statistic);
        }

        [Route("managers/{id:int}")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult GetAllResourseManagers(int id)
        {
            return Ok(_employeeServiceProvider.GetManagers(id));
        }

        [Route("managers/sub/{id:int}")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult GetSubordinates(int id)
        {
            return Ok(_employeeServiceProvider.GetSubordinates(id));
        }

        [Route("managers/main/{id:int}")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult GetEmployeesManager(int id)
        {
            return Ok(_employeeServiceProvider.GetEmployeesManager(id));
        }
        
        #endregion

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("")]
        public IHttpActionResult Post(EmployeeCreateViewModel employee)
        {
            //SetDefaultProfilePhoroUri(ref employee);

            if (!String.IsNullOrEmpty(employee.Contacts.Email))
            {

                EmailAddressAttribute emailAddressAttribute = new EmailAddressAttribute();
                if (!emailAddressAttribute.IsValid(employee.Contacts.Email))
                {
                    return BadRequest(String.Format(emailAddressAttribute.ErrorMessage, employee.Contacts.Email));
                }
            }

            int resultEmployee;

            if (User.IsInRole(Roles.Admin.ToString()))
            {
                resultEmployee = _employeeServiceProvider.Add(employee);
            }
            else
            {
                if (User.IsInRole(Roles.Employee.ToString()))
                {
                    // call self update (with property restrictions)
                    resultEmployee = _employeeServiceProvider.SelfUpdate(employee);
                }
                else
                {
                    return ThrowForbidden();
                }
            }

            if (resultEmployee == 0)
            {
                return BadRequest("error occured while trying to add new employee");
            }

            return Ok<int>(resultEmployee);
        }

        [HttpPut]
        [Authorize]
        [Route("{userId:int}")]
        public IHttpActionResult SaveEmployee(int userId, [FromBody] EmployeeCreateViewModel model)
         {
            model.Id = userId;
            var result = _employeeServiceProvider.Update(model);

            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        [Route("{userId:int}/contacts")]
        public IHttpActionResult SaveContacts(int userId, [FromBody] ContactsCreateViewModel model)
        {
            if (!String.IsNullOrEmpty(model.Email))
            {
                EmailAddressAttribute emailAddressAttribute = new EmailAddressAttribute();
                if (!emailAddressAttribute.IsValid(model.Email))
                {
                    return BadRequest(String.Format(emailAddressAttribute.ErrorMessage, model.Email));
                }
            }

            EmployeeContacts contacts = _employeeContactsProvider.SaveContacts(userId, model);

            if (contacts == null)
            {
                return BadRequest();
            }

            return Ok(contacts.EmployeeId);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin, Manager")]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            bool result = _employeeServiceProvider.Remove(id);
            if (result)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin, Manager")]
        [Route("")]
        public IHttpActionResult Delete()
        {
            bool result = _employeeServiceProvider.Erase();
            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        [Route("checkname")]
        public IHttpActionResult CheckNameAndEmail(EmployeeCheckNameModel model)
        {
            model.LastName = _employeeServiceProvider.IfNameExist(model);
            model.Email = $"{model.FirstName.ToLower()}_{model.LastName.ToLower()}@smartexlab.com";

            return Ok(model);
        }

        #region Private Methods
        private ResponseMessageResult ThrowForbidden()
        {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.Forbidden);

            return ResponseMessage(msg);
        }

        private void SetDefaultProfilePhoroUri(ref EmployeeCreateViewModel employee)
        {
            ConfigSection config = (ConfigSection)ConfigurationManager.GetSection("WebApi");
            Section configSection = config.Sections["server"];
            string defaultProfilePhotoFolder = configSection.GetComponent("ProfilePhotosServerPath").Value;
            string fullProfilePhotoPath = HttpContext.Current.Server.MapPath("~/" + defaultProfilePhotoFolder + "/");

            employee.PhotoUri = fullProfilePhotoPath + "default.jpg";
        }
        #endregion
    }

}

