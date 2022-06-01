$(function () {

    'use strict';

    $('.js-menu-toggle').click(function () {

        var $this = $(this);

        if ($('aside').hasClass('show-sidebar')) {
            $('aside').removeClass('show-sidebar');
            $this.removeClass('active');
        } else {
            $('aside').addClass('show-sidebar');
        }
    });

    // click outisde offcanvas
    $(document).mouseup(function (e) {
        var container = $(".sidebar");
        if (!container.is(e.target) && container.has(e.target).length === 0) {
            if ($('aside').hasClass('show-sidebar')) {
                $(`ul#major-plan-tree`).slideUp();
                $('aside').removeClass('show-sidebar');
            }
        }
    });

    $("button#major").click((e) => {
        $(`ul#major-plan-tree:not(ul[data-major='${e.target.innerHTML}'])`).slideUp();
        $(`ul[data-major='${e.target.innerHTML}']`).slideToggle();
    });

    if ($(".error-alert").hasClass("validation-summary-errors")) {
        $(".error-alert").addClass("alert alert-danger");
    }
    else {
        $(".error-alert").removeClass("alert alert-danger");
    }
});