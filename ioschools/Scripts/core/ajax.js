var tradelr = tradelr || {};
tradelr.ajax = {};
tradelr.ajax.init = function () {
    jQuery.ajaxSettings.traditional = true;

    $.fn.extend({
        getx: function (url, data, callback, type) {
            if (jQuery.isFunction(data)) {
                type = type || callback;
                callback = data;
                data = undefined;
            }

            return jQuery.ajax({
                type: 'get',
                url: url,
                data: data,
                success: callback,
                dataType: type,
                context: this.length == 0 ? undefined : this
            });
        },
        post: function (url, data, callback, type) {
            if (jQuery.isFunction(data)) {
                type = type || callback;
                callback = data;
                data = undefined;
            }

            return jQuery.ajax({
                type: 'post',
                url: url,
                data: data,
                success: callback,
                dataType: type,
                context: this.length == 0 ? undefined : this
            });
        }
    });

    $(document).ajaxSend(function (event, request, settings) {
        if (settings.context != undefined &&
            settings.context.length &&
            settings.context[0].nodeType) {
            $(settings.context).attr("disabled", true);
        }
    });

    $(document).ajaxError(function (event, request, settings) {
        if (settings.context != undefined &&
            settings.context.length &&
                settings.context[0].nodeType) {
            $(settings.context).attr("disabled", false);
        }

        switch (request.status) {
            case 200:
                break;
            case 401:
                break;
            case 403:
                break;
            case 500:
                if (request.responseText.indexOf('A potentially dangerous Request.Form') != -1) {
                    $.jGrowl('Please check your input. Please use plain text (no HTML tags).');
                } else {
                    $.jGrowl('An error has occurred and has been noted. Try again or wait for 24 hours for us to resolve the problem.');
                }
                break;
            default:
                if (DEBUG) {
                    throw [request.status, ":", request.responseText].toString();
                }
                break;
        }

        $.prettyLoader.hide();
    });


    $(document).ajaxComplete(function (event, request, settings) {
        if (settings.context != undefined &&
            settings.context.length &&
            settings.context[0].nodeType) {
            $(settings.context).attr("disabled", false);
        }

        switch (request.status) {
            case 200:
                break;
            case 401:
                $.jGrowl("Your session has expired. <a href='/login?redirect=" + encodeURIComponent(window.location.pathname) + "'>Click here to sign in</a>", { sticky: true});
                break;
            case 403:
                $.jGrowl("You don't have permission to do that");
                break;
            case 500:
                break;
            default:
                if (DEBUG) {
                    throw [request.status, ":", request.responseText].toString();
                }
                break;
        }
    });
};