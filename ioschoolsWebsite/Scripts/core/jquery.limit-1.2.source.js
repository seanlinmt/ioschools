/* limits chars
*/
(function ($) {
    $.fn.limit = function (limit, element) {
        if (limit == undefined || limit == null || isNaN(limit) || (parseFloat(limit) != parseInt(limit))) {
                return this; // so that this will work with inline edit plugin
            }
        return this.each(function () {
            var interval, f;
            var self = $(this);

            $(this).focus(function () {
                interval = window.setInterval(substring, 100);
            });

            $(this).blur(function () {
                clearInterval(interval);
                substring();
            });

            function substring() {
                var val = $(self).val();
                var length = val.length;
                if (length > limit) {
                    $(self).val($(self).val().substring(0, limit));
                }
                if ($(element).html() != limit - length) {
                    $(element).html((limit - length <= 0) ? '0' : limit - length);
                }
            }
            substring();
        });
    };
})(jQuery);