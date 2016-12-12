var app = angular.module('Smartboard', [
        'ui.router',
        'ui.uploader',
        'Smartboard.Manager',
        'Smartboard.Admin',
        'Smartboard.Employee',
        'Smartboard.Client',
		'ngTinyScrollbar',
        'angular-loading-bar',
        'ngToast',
        'draganddrop',
        'ui.bootstrap',
        'gm.datepickerMultiSelect',
        'angucomplete-alt'
    ])
    .run(function ($rootScope, $state, $q, NotificationService, UserService, webConfig, VacationStatus, ROLES) {
        $rootScope.dateFormat = webConfig.dateFormat;
        $rootScope.VacationStatus = VacationStatus;
        var role = UserService.getRole();
        var id = UserService.getId();
        var token = UserService.getToken();
        if (role && id) {
            NotificationService.getVacationNotifications(id)
                .then(function (result) {
                    console.log(result);
                    NotificationService.setNotificationNumber(result);
                }, function (error) {
                    console.log('error getting notifications');
                });
        }
        $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
            //console.log('to ' + toState.name);
            //console.log('from ' + fromState.name);
            var role = UserService.getRole();
            var splitted = toState.name.split('.');
            var stateRole = splitted[1];
            if (!role && toState.name != 'Main.Login') {
                $state.go('Main.Login');
                event.preventDefault();
            }
            if (stateRole == ROLES.ADMIN || stateRole == ROLES.MANAGER || stateRole == ROLES.EMPLOYEE || stateRole == ROLES.CLIENT) {
                // to prevent access to another entity
                // example: if logged in like Manager, prevent url: '/Employee/Dashboard'
                if (stateRole != role && role) {
                    event.preventDefault();
                    console.log('redirect to dashboard');
                    $state.go('Main.' + role + '.Dashboard', toParams);
                }
            } else if (toState.name == 'Main.Login') {
                if (token && role) {
                    $state.go('Main.' + role + '.Dashboard', toParams);
                    event.preventDefault();
                }
            }
        });
    });
