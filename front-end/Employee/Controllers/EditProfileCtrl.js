angular.module('Smartboard.Employee')
    .controller('EmplyeeEditProfileCtrl', [
        '$rootScope',
        '$scope',
        '$state',
        '_',
        'EmployeeService',
        'AddEditService',
        'employeeData',
        'alertMsg',
        function ($rootScope, $scope, $state, _, EmployeeService, AddEditService, employeeData, alertMsg) {
            var vm = this;
            vm.employee = employeeData.employee;
            vm.signupEmployeeEditForm = signupEmployeeEditForm;

            console.log(vm.employee);

            vm.formLanguage = {
                index: null,
                isShownEditButtons: false,
                isShownAddButton: true,
                add: addLanguage,
                edit: editLanguage,
                save: saveLanguage,
                remove: removeLanguage,
                cancel: cancelLanguageEdition
            };

            console.log(vm.employee.birthDate);

            vm.formSkill = {
                isPrimary: false,
                index: null,
                isShownEditButtons: false,
                isShownAddButton: true,
                add: addSkill,
                edit: editSkill,
                save: saveSkill,
                remove: removeSkill,
                cancel: cancelSkillEdition
            };

            vm.birthDate = {
                isOpen: false,
                dateOptions: {
                    formatYear: 'yy',
                    startingDay: 1
                },
                datepickerFormat: $rootScope.dateFormat,
                maxDate: new Date(),
                openDatepicker: function () {
                    this.isOpen = true;
                }
            };

            function signupEmployeeEditForm() {
                console.log(vm.employee);
                $scope.employeeEditForm.$setSubmitted();
                clearAllForms();
                setValidity(true);
                if ($scope.employeeEditForm.$valid) {
                    EmployeeService.postEmployee(vm.employee)
                        .then(function (result) {
                            $scope.setShowAlert(true, 'success', 'Employee' + ' ' + vm.employee.firstName + ' ' + vm.employee.lastName + ' ' + alertMsg.successEditAlert);
                            $state.go('Main.Admin.Dashboard');
                        }, function (error) {
                            alert(error.status + ' - ' + error.statusText);
                            setValidity(false);
                        })
                }
                else {
                    setValidity(false);
                }
            }

            function clearAllForms() {
                AddEditService.clearLanguageForm(vm.formLanguage);
                AddEditService.clearSkillForm(vm.formSkill);
            }

            function setValidity(value) {
                $scope.language.selectedLanguageId.$setValidity('required', value);
                $scope.language.languageLevel.$setValidity('required', value);
                $scope.skill.skillName.$setValidity('required', value);
                $scope.skill.skillLevel.$setValidity('required', value);
                $scope.skill.skillExperience.$setValidity('required', value);
                $scope.skill.skillLastUsed.$setValidity('required', value);
            }

            function addLanguage() {
                if ($scope.language.$valid) {
                    var obj = {
                        name: vm.formLanguage.name,
                        level: vm.formLanguage.level
                    };
                    vm.employee.foreignLanguages.push(obj);
                    AddEditService.clearLanguageForm(vm.formLanguage);
                    AddEditService.setUntouchedLanguageForm($scope.language);
                }
            }

            function saveLanguage() {
                var language = vm.employee.foreignLanguages[vm.formLanguage.index];
                if ($scope.language.$valid) {
                    language.name = vm.formLanguage.name;
                    language.level = vm.formLanguage.level;

                    AddEditService.clearLanguageForm(vm.formLanguage);
                    AddEditService.setUntouchedLanguageForm($scope.language);

                    vm.formLanguage.isShownEditButtons = false;
                    vm.formLanguage.isShownAddButton = true;
                }
            }

            function editLanguage(index) {
                var language = vm.employee.foreignLanguages[index];
                vm.formLanguage.name = language.name;
                vm.formLanguage.level = language.level;
                vm.formLanguage.index = index;
                vm.formLanguage.isShownEditButtons = true;
                vm.formLanguage.isShownAddButton = false;
				return true;
            }

            function cancelLanguageEdition() {
                AddEditService.clearLanguageForm(vm.formLanguage);
                AddEditService.setUntouchedLanguageForm($scope.language);
                vm.formLanguage.isShownEditButtons = false;
                vm.formLanguage.isShownAddButton = true;
            }

            function removeLanguage(index) {
                cancelLanguageEdition();
                vm.employee.foreignLanguages.splice(index, 1);
            }

            function addSkill() {
                if ($scope.skill.$valid) {
                    var obj = {
                        name: vm.formSkill.name,
                        levelOfExperience: vm.formSkill.levelOfExperience,
                        yearsOfExperience: vm.formSkill.yearsOfExperience,
                        lastYearUsed: vm.formSkill.lastYearUsed,
                        isPrimary: vm.formSkill.isPrimary
                    };
                    vm.employee.skills.push(obj);

                    sortSkills();

                    AddEditService.clearSkillForm(vm.formSkill);
                    AddEditService.setUntouchedSkillForm($scope.skill);
                }
            }

            function editSkill(index) {
                var skill = vm.employee.skills[index];
                vm.formSkill.name = skill.name;
                vm.formSkill.levelOfExperience = skill.levelOfExperience;
                vm.formSkill.yearsOfExperience = skill.yearsOfExperience;
                vm.formSkill.lastYearUsed = skill.lastYearUsed;
                vm.formSkill.isPrimary = skill.isPrimary;
                vm.formSkill.index = index;

                vm.formSkill.isShownEditButtons = true;
                vm.formSkill.isShownAddButton = false;
            }

            function saveSkill() {
                var skill = vm.employee.skills[vm.formSkill.index];
                if ($scope.skill.$valid) {
                    skill.name = vm.formSkill.name;
                    skill.levelOfExperience = vm.formSkill.levelOfExperience;
                    skill.yearsOfExperience = vm.formSkill.yearsOfExperience;
                    skill.lastYearUsed = vm.formSkill.lastYearUsed;
                    skill.isPrimary = vm.formSkill.isPrimary;

                    sortSkills();

                    AddEditService.clearSkillForm(vm.formSkill);
                    AddEditService.setUntouchedSkillForm($scope.skill);

                    vm.formSkill.isShownEditButtons = false;
                    vm.formSkill.isShownAddButton = true;
                }
            }

            function removeSkill(index) {
                cancelSkillEdition();
                vm.employee.skills.splice(index, 1);
            }

            function cancelSkillEdition() {
                AddEditService.clearSkillForm(vm.formSkill);
                AddEditService.setUntouchedSkillForm($scope.skill);
                vm.formSkill.isShownEditButtons = false;
                vm.formSkill.isShownAddButton = true;
            }

            function sortSkills() {
                vm.employee.skills = _.sortByOrder(vm.employee.skills, ['isPrimary'], ['desc']);
            }
        }
    ]);
