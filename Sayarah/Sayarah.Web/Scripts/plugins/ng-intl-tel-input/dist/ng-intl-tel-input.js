angular.module('ngIntlTelInput', []); angular.module('ngIntlTelInput')
  .provider('ngIntlTelInput', function () {
    var me = this;
    var props = {};
    var setFn = function (obj) {
      if (typeof obj === 'object') {
        for (var key in obj) {
          props[key] = obj[key];
        }
      }
    };
    me.set = setFn;

    me.$get = ['$log', function ($log) {
      return Object.create(me, {
        init: {
          value: function (elm) {
            if (!window.intlTelInputUtils) {
              $log.warn('intlTelInputUtils is not defined. Formatting and validation will not work.');
            }
            elm.intlTelInput(props);
          }
        },
      });
    }];
  });
angular.module('ngIntlTelInput')
  .directive('ngIntlTelInput', ['$timeout', 'ngIntlTelInput', '$log', '$window', '$parse',
    function ($timeout, ngIntlTelInput, $log, $window, $parse) {
      return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attr, ctrl) {
          // Warning for bad directive usage.
          if ((!!attr.type && (attr.type !== 'text' && attr.type !== 'tel')) || elm[0].tagName !== 'INPUT') {
            $log.warn('ng-intl-tel-input can only be applied to a *text* or *tel* input');
            return;
          }
          // Override separateDialCode.
          if (attr.hiddenInput) {
            ngIntlTelInput.set({ hiddenInput: attr.hiddenInput });
          }
          // Override separateDialCode.
          if (attr.separateDialCode) {
            ngIntlTelInput.set({ separateDialCode: attr.separateDialCode });
          }
          // Override default country.
          if (attr.initialCountry) {
            ngIntlTelInput.set({ initialCountry: attr.initialCountry });
          }
          // Initialize.
          ngIntlTelInput.init(elm);
          // Set Selected Country Data.
          function setSelectedCountryData(model) {
            var getter = $parse(model);
            var setter = getter.assign;
            setter(scope, elm.intlTelInput('getSelectedCountryData'));
          }
          // Handle Country Changes.
          function handleCountryChange() {
            setSelectedCountryData(attr.selectedCountry);
          }
          // Country Change cleanup.
          function cleanUp() {
            angular.element($window).off('countrychange', handleCountryChange);
          }
          // Selected Country Data.
          if (attr.selectedCountry) {
            setSelectedCountryData(attr.selectedCountry);
            angular.element($window).on('countrychange', handleCountryChange);
            scope.$on('$destroy', cleanUp);
          }
          // Validation.
          ctrl.$validators.ngIntlTelInput = function (value) {
            // if phone number is deleted / empty do not run phone number validation
            if (value || elm[0].value.length > 0) {
              return elm.intlTelInput('isValidNumber');
            } else {
              return true;
            }
          };
          // Set model value to valid, formatted version.
          ctrl.$parsers.push(function (value) {
            //return elm.intlTelInput('getNumber');
            return elm.intlTelInput('getFullNumber')
          });
          // Set input value to model value and trigger evaluation.
          ctrl.$formatters.push(function (value) {
            $timeout(function () {
              if (value) {
                if (value.charAt(0) !== '+') {
                  value = '+' + value;
                }

                elm.intlTelInput('setNumber', value);
                elm.trigger('change');
              }
              console.log(value);
              return value;
            }, 0);
          });
        }
      };
    }]);
