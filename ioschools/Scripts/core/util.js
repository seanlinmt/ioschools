ioschools.util = {};

ioschools.util.stripTags = function (s) {
    return s.replace(/<&#91;^>&#93;*>/g, "");
};

ioschools.util.querySt = function (ji) {
    var hu = window.location.search.substring(1);
    var gy = hu.split("&");
    for (var i = 0; i < gy.length; i++) {
        var ft = gy[i].split("=");
        if (ft[0] == ji) {
            return ft[1];
        }
    }
    return "";
};

ioschools.util.dateDiff = function(start, end) {
    var timeDiff = Math.abs(end.getTime() - start.getTime());
    var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
    return diffDays;
};

ioschools.util.navigation = {};
ioschools.util.navigation.bind = function () {
    $('.navlink').click(function () {
        $('.navlink').removeClass('current');
        $('.navlink').next().hide();
        $(this).toggleClass('current');
        $(this).next().toggle();
        return false;
    });
};

ioschools.util.setNavigation = function(element, classname) {
    $('#mainnav > li > a').removeClass('selected selected_dark selected_link');
    $(element).addClass(classname);
};

ioschools.util.browserSupported = function () {
    var supported = false;
    var idx = $.browser.version.indexOf(".");
    if (idx == -1) {
        return supported;
    }
    var version = parseInt($.browser.version.substring(0, idx), 10);

    if ($.browser.mozilla) {
        if (version >= 10) {
            supported = true;
        }
    }
    else if ($.browser.msie) {
        if (version >= 9) {
            supported = true;
        }
    }
    else if ($.browser.safari) {
        if (version >= 534) {
            supported = true;
        }
    }
    else if ($.browser.webkit) {
        if (version >= 535) {
            supported = true;
        }
    }

    return supported;
};