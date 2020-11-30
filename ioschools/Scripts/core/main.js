// log any client-side errors
window.onerror = function (em, url, ln) {
    if (ln == 0) {
        return false;
    }
    var msg = JSON.stringify(em) + ", " + url + ", " + ln;

    var getBrowserString = function () {
        var browser = [];
        $.each(jQuery.browser, function (i, val) {
            browser.push(i + " : " + val);
        });
        return browser.toString();
    };

    var ip = $('#ipaddr').val();

    msg = ip + " " + window.location.toString() + " (" + getBrowserString() + ") :" + escape(msg);

    $.post('/error/log/', { message: msg, loaded: ioschools.pageloaded });
    //$.jGrowl("An error has occurred. Try pressing Ctrl and F5 simultaneously to clear the error.", { sticky: true });
    return false;
};

window.onload = function () {
    ioschools.pageloaded = true;
    if (self != top) {
        top.location = self.location;
    }
};

var lodge = lodge || {};
ioschools.preverr = '';
ioschools.pageloaded = false;

$(document).ready(function () {


    $('.jGrowl-notification').live('click', function () {
        $(this).trigger('jGrowl.close');
    });

    // pretty loader
    $.prettyLoader();

    tradelr.ajax.init();

    $.watermark.options = {
        className: 'watermark'
    };

    // prototypes
    Array.max = function (array) {
        return Math.max.apply(Math, array);
    };

    Array.min = function (array) {
        return Math.min.apply(Math, array);
    };

    // binary search of a sorted array
    Array.prototype.binarySearch = function (find, comparator) {
        var low = 0, high = this.length - 1, i, comparison;
        while (low <= high) {
            i = Math.floor((low + high) / 2);
            comparison = comparator(this[i], find);
            if (comparison < 0) { low = i + 1; continue; };
            if (comparison > 0) { high = i - 1; continue; };
            return i;
        }
        return -1;
    };

    // normal search of any array
    Array.prototype.search = function (find, comparator) {
        for (var i = 0; i < this.length; i++) {
            var comparison = comparator(this[i], find);
            if (comparison == 0) {
                return i;
            }
        }
        return -1;
    };

    // remove item from array
    Array.prototype.remove = function (from, to) {
        var rest = this.slice((to || from) + 1 || this.length);
        this.length = from < 0 ? this.length + from : from;
        return this.push.apply(this, rest);
    };

    // fix array indexOF in some browsers
    if (!Array.prototype.indexOf) {
        Array.prototype.indexOf = function (obj, start) {
            for (var i = (start || 0), j = this.length; i < j; i++) {
                if (this[i] === obj) {
                    return i;
                }
            }
            return -1;
        };
    }

    // check browser
    if (!ioschools.util.browserSupported()) {
        $('#browserNotSupportedWarning').slideDown();
    }

});

$.fn.extend({
    focusTextarea: function () {
        if (this.length == 0) {
            return;
        }
        var el = this[0];
        if (el.createTextRange) {
            //IE  
            var FieldRange = el.createTextRange();
            FieldRange.moveStart('character', el.value.length);
            FieldRange.collapse();
            FieldRange.select();
        }
        else {
            //Firefox and Opera  
            el.focus();
            var length = el.value.length;
            el.setSelectionRange(length, length);
        }
    },
    removeLoading: function () {
        $('.loader', this).remove();
    },
    showLoading: function (msg) {
        if (msg == null) {
            return this.each(function () {
                $(this).html("<img class='loader' align='absmiddle' src='/Content/img/loading.gif' />");
            });
        }
        else {
            return $(this).html("<img class='loader' align='absmiddle' src='/Content/img/loading.gif' /> " + msg);
        }
    },
    showLoadingBlock: function (clear) {
        return this.each(function () {
            if ($('.loader', this).length == 0) {
                if (clear != undefined && clear) {
                    $(this).html('');
                }
                $(this).append("<div class='loader ac p10 clear'><img src='/Content/img/loading_circle.gif' /></div>");
            }
        });
    },
    trackUnsavedChanges: function (resetButtonID) {
        isDirty = false;
        $('input, textarea, select', this).change(function () {
            isDirty = true;
        });
        window.onbeforeunload = function () {
            if (isDirty) {
                return 'You have unsaved changes.';
            }
        };
        $(resetButtonID).click(function () {
            isDirty = false;
        });
    }
});

jQuery.fn.quickEach = (function () {

    var jq = jQuery([1]);

    return function (c) {

        var i = -1, el, len = this.length;

        try {

            while (
                 ++i < len &&
                 (el = jq[0] = this[i]) &&
                 c.call(jq, i, el) !== false
            );

        } catch (e) {

            delete jq[0];
            throw e;

        }

        delete jq[0];

        return this;

    };

} ());

function GetAvailableHeight(targetid) {
    var total = $(window).height();
    var offset = $(targetid).offset().top;
    var others = 0;
    for (var i = 1; i < arguments.length; i++) {
        others += $(arguments[i]).outerHeight();
    }
    //console.log([total, offset, others].join(' '));
    return total - offset - others;
};

function loadMainContent(url, hash, callback) {
    $.get(url, function (result) {
        if (result.success != undefined && !result.success) {
            $.jGrowl(result.message);
            return false;
        }

        $('#content_result').html(result);
        if (hash != undefined && hash != null) {
            parent.location.hash = hash;
        }

        if (callback != undefined && callback != null) {
            callback();
        }
    });
};

function dialogBox_close() {
    $('#ajax_dialog').dialog('destroy');
    $('#ajax_dialog').remove();
    $('body').css('overflow-y', 'scroll');
    return false;
};


function dialogBox_open(url, title, w) {
    if (w == undefined || w == '' || w == null) {
        w = 400;
    }

    $.get(url, null, function (result) {
        // should never be a json result unless we are not logged in
        if (result.success != undefined && !result.success) {
            $.jGrowl(result.message);
            return false;
        }
        $('body').append("<div id='ajax_dialog'><div id='ajax_content'></div></div>");
        $('#ajax_content').html(result);
        $('#ajax_dialog').dialog({
            autoOpen: false,
            closeOnEscape: true,
            draggable: false,
            modal: true,
            resizable: false,
            overlay: { background: "white" },
            width: w,
            title: title,
            zIndex: 1000
        });
        $('#ajax_dialog').dialog("open");
    });
};

function init_autogrow(context, height) {
    var minval = '50px';
    if (height != null) {
        minval = height + 'px';
    }

    $("textarea", context).autogrow({ minHeight: minval });
};

function init_inputSelectors() {
    $('input[type="text"],input[type="password"],textarea').bind('focus.input', null, function () {
        $(this).addClass("curFocus");
    });
    $('input[type="text"],input[type="password"],textarea').bind('blur.input', null, function () {
        $(this).removeClass("curFocus");
    });
};

function isEnterKey(e) {
    var key;
    if (window.event) {
        key = window.event.keyCode;     //IE
    }
    else {
        key = e.which;     //firefox
    }
    if (key == 13)
        return true;

    return false;
};

