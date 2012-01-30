$(document).ready(function () {
    // Add a new route form to the screen
    var url = window.location.toString().replace("Indexing", "");
    $("#addroute").click(function (e) {
        $.ajax({
            url: url + "GetCustomRouteForm",
            success: function (data) {
                var o = $(data);
                o.hide();
                $("#custom-routes").append(o);
                o.fadeIn(600).css("display", "table-row");
            }
        });
        e.preventDefault();
    });

    $("#custom-routes").delegate(".delete-button button", "click", function (e) {
        $(this).closest("tr").fadeOut(600, function () {
            $(this).remove();
        });
        e.preventDefault();
    });

    $("#save").click(function (e) {
        // Fix all the custom route names.
        $("#custom-routes tr").each(function (i, e) {
            $(e).find("input, select").each(function(fi, fe) {
                $(fe).attr("name", "CustomRoutes[" + i + "]." + $(fe).attr("name").replace("route.", ""));
            });
        });
    });
});