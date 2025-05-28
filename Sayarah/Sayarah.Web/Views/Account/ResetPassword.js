(function ($) {

    if (!$) {
        return;
    }

    $(function () {

        var $registerForm = $('#ForgotPasswordForm');

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
                EmailAddress: {
                    required: true
               
                },
                Password: {
                    required: true,
                    minlength: 6
                },
                ConfirmPassword: {
                    required: true,
                    equalTo: "#Password"
                }
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