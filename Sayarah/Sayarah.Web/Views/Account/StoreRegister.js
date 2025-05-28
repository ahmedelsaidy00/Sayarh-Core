(function ($) {

    if (!$) {
        return;
    }

    $(function () {

        var $registerForm = $('#StoreRegisterForm');
      

        $.validator.addMethod("customUsername", function (value, element) {
            if (value === $registerForm.find('input[name="EmailAddress"]').val()) {
                return true;
            }

            //Username can not be an email address (except the email address entered)
            return !$.validator.methods.email.apply(this, arguments);
        }, abp.localization.localize("RegisterFormUserNameInvalidMessage", "Sayarah"));

        $.validator.addMethod("customPass", function (value, element) {
            if (value === $registerForm.find('input[name="Password"]').val()) {
                return true;
            }
            return false;
        }, abp.localization.localize("Common.Error.PasswordNotMatch", "Sayarah"));

        //$.validator.addMethod("customPhoneNumber", function (value, element) {
        //    if (telInput.intlTelInput("isValidNumber")) {
        //        $('#PhoneNumber').val(telInput.intlTelInput('getFullNumber'));
        //        return true;
        //    }

        //    //
        //    return false;
        //}, abp.localization.localize("Common.Error.InvalidPhoneNumber", "Sayarah"));

        //$.validator.addMethod("customPhoneNumber2", function (value, element) {
        //    if (telInput2.intlTelInput("isValidNumber")) {
        //        $('#CustomerServicePhoneNo').val(telInput2.intlTelInput('getFullNumber'));
        //        return true;
        //    }

        //    //
        //    return false;
        //}, abp.localization.localize("Common.Error.InvalidPhoneNumber", "Sayarah"));


        $.validator.addMethod("regx", function (value, element, regexpr) {
            return regexpr.test(value);
        }, abp.localization.localize("Common.Error.InvalidEmail", "Sayarah"));

        $.validator.addMethod("regx", function (value, element, regexpr) {
            return regexpr.test(value);
        }, abp.localization.localize("Common.Error.InvalidUserName", "Sayarah"));


   
        //$.validator.addMethod("customPhoneNumber", function (value, element) {
        //    if (telInput.intlTelInput("isValidNumber")) {
        //        $('#PhoneNumber').val(telInput.intlTelInput('getFullNumber'));
        //        return true;
        //    }

        //    //
        //    return false;
        //}, abp.localization.localize("Common.Error.InvalidPhoneNumber", "Sayarah"));



        /*
* Translated default messages for the jQuery validation plugin.
* Locale: AR (Arabic; العربية)
*/
        if (abp.localization.currentLanguage.name == 'ar') {
            $.extend($.validator.messages, {
                required: "هذا الحقل إلزامي",
                remote: "يرجى تصحيح هذا الحقل للمتابعة",
                email: "رجاء إدخال عنوان بريد إلكتروني صحيح",
                url: "رجاء إدخال عنوان موقع إلكتروني صحيح",
                date: "رجاء إدخال تاريخ صحيح",
                dateISO: "رجاء إدخال تاريخ صحيح (ISO)",
                number: "رجاء إدخال عدد بطريقة صحيحة",
                digits: "رجاء إدخال أرقام فقط",
                creditcard: "رجاء إدخال رقم بطاقة ائتمان صحيح",
                equalTo: "رجاء إدخال نفس القيمة",
                extension: "رجاء إدخال ملف بامتداد موافق عليه",
                maxlength: $.validator.format("الحد الأقصى لعدد الحروف هو {0}"),
                minlength: $.validator.format("الحد الأدنى لعدد الحروف هو {0}"),
                rangelength: $.validator.format("عدد الحروف يجب أن يكون بين {0} و {1}"),
                range: $.validator.format("رجاء إدخال عدد قيمته بين {0} و {1}"),
                max: $.validator.format("رجاء إدخال عدد أقل من أو يساوي {0}"),
                min: $.validator.format("رجاء إدخال عدد أكبر من أو يساوي {0}")
            });
        }



        $registerForm.validate({
            rules: {
                //UserName: {
                //    required: true,
                //    customUsername: true
                //},
                //Surname: {
                //    required: true,
                //    customSurname: true
                //},
                //PhoneNumber: {
                //    required: true,
                //    customPhoneNumber: true
                //},
                //phone: {
                //    required: true,
                //    customPhoneNumber: true
                //},
                //phone2: {
                //    required: true,
                //    customPhoneNumber2: true
                //},
                EmailAddress: {
                    required: true,
                    //change regexp to suit your needs
                    regx: /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,7})+$/,
                },
                //AcceptTerms: {
                //    required: true
                //},
                ConfirmPassword: {
                    required: true,
                    customPass: true
                },
                Password: {
                    required: true
                }
                //,
                //Terms: {
                //    required: true
                //}
            },

            highlight: function (input) {
                $(input).parents('.form-line').addClass('error');
            },

            unhighlight: function (input) {
                $(input).parents('.form-line').removeClass('error');
            },

            errorPlacement: function (error, element) {
                $(element).parents('.form-group').append(error);
            }
        });
    });

})(jQuery);