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

    // Show Tree Plan inside sidebar
    $("button#major").click((e) => {
        $(`ul#major-plan-tree:not(ul[data-major='${e.target.innerHTML}'])`).slideUp();
        $(`ul[data-major='${e.target.innerHTML}']`).slideToggle();
    });

    // Add URL parameters or (query strings) for delete plan button
    $("button[data-target='#deletePlan']").on("click", (e) => {
        let url = `${$("#btn-delete-Plan").attr("href")}?fileName=${e.currentTarget.dataset.filename}&major=${e.currentTarget.dataset.magor}&planType=TreePlan`;
        $("#btn-delete-Plan").attr("href", url);
    })

    if ($(".error-alert").hasClass("validation-summary-errors")) {
        $(".error-alert").addClass("alert alert-danger");
    }
    else {
        $(".error-alert").removeClass("alert alert-danger");
    }
});