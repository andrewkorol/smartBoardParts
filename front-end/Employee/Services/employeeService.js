app.factory('EmployeeService', [
        '$http',
        '$window',
        '$q',
        '_',
        'DateHelperService',
        'webConfig',
        function ($http, $window, $q, _, DateHelperService, webConfig) {
            return {
                getEmployee: getEmployee,
                getEmployees: getEmployees,
                postEmployee: postEmployee,
                deleteEmployee: deleteEmployee,
                getDashboardData: getDashboardData,
                getEmployeesDashboard: getEmployeesDashboard,
                getProjectsIds: getProjectsIds,
                addEditPosition: addEditPosition,
                addEditLanguage: addEditLanguage,
                addEditSkill: addEditSkill,
                deletePosition: deletePosition,
                deleteLanguage: deleteLanguage,
                deleteSkill: deleteSkill,
                SaveEmployee: SaveEmployee,
				CheckPerson: CheckPerson,
				GetManagers: GetManagers,
				getListOfSubordinates: getListOfSubordinates,
				getHeadManager: getHeadManager,
				ChangeRole: ChangeRole
            };

            function getEmployee(id) {//on the bask-end check, if user is manager or not, and return subordinates/headmanager
                return $http.get(webConfig.apiServer + '/employees/' + id)
                    .then($http.getDataFromResult).then(function (result) {
                        for (var i in result.positions) {
                            if (!result.positions[i].endDate) {
                                result.positions[i].endDate = "Present";
                            } else {
                                result.positions[i].endDate = new Date(result.positions[i].endDate);
                            }
                            result.positions[i].startDate = new Date(result.positions[i].startDate);
                        }
                        if (result.birthDate) {
                            result.birthDate = new Date(result.birthDate);
                        }
                        if (result.employmentDate) {
                            result.employmentDate = new Date(result.employmentDate);
                        }
                        _.forEach(result.projects.current, function (n) {
                            _.forEach(n.assignments, function (n) {
                                if (!n.endDate) {
                                    n.endDate = 'Present'
                                } else {
                                    n.endDate = new Date(n.endDate);
                                }
                                n.startDate = new Date(n.startDate);
                            });
                        });
                        _.forEach(result.projects.previous, function (n) {
                            _.forEach(n.assignments, function (n) {
                                n.endDate = new Date(n.endDate);
                                n.startDate = new Date(n.startDate);
                            });
                        });
                        return result;
                    });
            }

            function getEmployees() {
                return $http.get(webConfig.apiServer + '/employees')
                    .then($http.getDataFromResult);
            }

            function postEmployee(employee) {
                employee.birthDate = DateHelperService.toUTC(employee.birthDate);
                employee.employmentDate = DateHelperService.toUTC(employee.employmentDate);
                for (var i in employee.positions) {
                    employee.positions[i].startDate = DateHelperService.toUTC(employee.positions[i].startDate);
                    if (employee.positions[i].endDate != 'Present') {
                        employee.positions[i].endDate = DateHelperService.toUTC(employee.positions[i].endDate);
                    }
                }
                return $http.post(webConfig.apiServer + '/employees', employee)
                    .then($http.getDataFromResult);
            }

            function deleteEmployee(id) {
                return $http.delete(webConfig.apiServer + '/employees/' + id)
                    .then($http.getDataFromResult);
            }

            function getDashboardData(id) {
                return $http.get(webConfig.apiServer + '/dashboard/' + id)
                    .then($http.getDataFromResult);
            }

            function getEmployeesDashboard() {
                return $http.get(webConfig.apiServer + '/dashboard/employees')
                    .then($http.getDataFromResult);
            }

            function getProjectsIds(id) {
                return $http.get(webConfig.apiServer + '/employees/' + id + '/projects/ids')
                    .then($http.getDataFromResult);
            }

            function addEditPosition(position) {
                return $q.resolve('ok');
            }

            function addEditLanguage(language) {
                return $q.resolve('ok');
            }

            function addEditSkill(skill) {
                return $q.resolve('ok');
            }

            function deletePosition(id) {
                return $q.resolve('ok');
            }

            function deleteLanguage(id) {
                return $q.resolve('ok');
            }

            function deleteSkill(id) {
                return $q.resolve('ok');
            }

            function SaveEmployee(userId, body) {
                return $http.put(webConfig.apiServer + '/employees/' + userId, body)
                    .then($http.getDataFromResult);
            }

            function CheckPerson(user) {
				return $http.post(webConfig.apiServer + '/employees' + '/checkname', user)
					.then($http.getDataFromResult);
            }

            function GetManagers(managerId) {
                return $http.get(webConfig.apiServer + '/employees' + '/managers/' + managerId)
                .then($http.getDataFromResult);
            }

            function getListOfSubordinates(managerId) {
                return $http.get(webConfig.apiServer + '/employees' + '/managers'+ '/sub/' + managerId)
                .then($http.getDataFromResult);
            }

            function getHeadManager(employeeId) {
                return $http.get(webConfig.apiServer + '/employees/' + '/managers' + '/main/' + employeeId)
                .then($http.getDataFromResult);
            }

            function ChangeRole(userRole) {
                return $http.get(webConfig.authServer + '/roles/' + userRole.roleName + '/' + userRole.userId)
                    .then($http.getDataFromResult);
            }
        }]);
