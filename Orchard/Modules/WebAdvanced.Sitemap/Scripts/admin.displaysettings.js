$(document).ready(function () {
    $(".column, .disabled-items").sortable({
        connectWith: ".column, .disabled-items"
    });

    $(".portlet").addClass("ui-widget ui-widget-content ui-helper-clearfix");

    $(".column").disableSelection();

    $(".levels-decrease a").click(function (e) {
        var levelElement = $(this).closest(".display-levels-container").find(".levels-display");
        var levels = parseInt(levelElement.html());
        if (levels > 1) {
            levels = levels - 1;
            levelElement.html(levels);
        }
        e.preventDefault();
    });

    $(".levels-increase a").click(function (e) {
        var levelElement = $(this).closest(".display-levels-container").find(".levels-display");
        var levels = parseInt(levelElement.html());
        if (levels < 9) {
            levels = levels + 1;
            levelElement.html(levels);
        }
        e.preventDefault();
    });

    $("form").submit(function (e) {
        // Set all weights
        $(".column").each(function (colIndex) {
            var column = $(this);
            column.children().each(function (itemIndex) {
                var item = $(this);
                var levels = item.find(".levels-display").html();
                // Set column
                item.find("input[name$=DisplayColumn]").val(colIndex + 1);
                item.find("input[name$=DisplayLevels]").val(levels);
                item.find("input[name$=Weight]").val(itemIndex);
                item.find("input[name$=Active]").val(true);
            });
        });

        $(".disabled-items").each(function (index) {
            var item = $(this);
            var levels = item.find(".levels-display").html();
            item.find("input[name$=Active]").val(false);
            item.find("input[name$=DisplayColumn]").val(1);
            item.find("input[name$=DisplayLevels]").val(levels);
            item.find("input[name$=Weight]").val(index);
        });
    });
});