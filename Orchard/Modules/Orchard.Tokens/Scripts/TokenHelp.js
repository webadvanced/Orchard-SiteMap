jQuery(function ($) {
    // retrieve the popup url
    var url = $("#token-hint-dialog").data("url");

    //define config object
    var dialogOpts = {
        modal: false,
        bgiframe: true,
        autoOpen: false,
        width: 800,
        draggable: false,
        resizeable: true,
        closeOnEscape: true,
        position: ['center', 'top'],
        open: function () {
            //display correct dialog content
            $("#token-hint-dialog").load(url);
        }
    };

    $("#token-hint-dialog").dialog(dialogOpts);

    $('.tokenized')
        .each(function () {
            var icon = $('<span class="tokenized-popup">&nbsp;</span>');
            icon.appendTo($(document.body));
            positionTokenPopup(icon, this);
            this.tokenizedPopup = icon;

        });

    $(window).resize(function () {
        $('.tokenized')
        .each(function () {
            var tokenizedPopup = this['tokenizedPopup'];
            if (!tokenizedPopup) {
                return;
            }

            positionTokenPopup(tokenizedPopup, this);
        });
    });

    function positionTokenPopup(tokenPopup, input) {
        var offset = $(input).offset();
        var left = offset.left + $(input).width() - 10;
        var top = offset.top + 8;
        tokenPopup.offset({ top: top, left: left });
    }

    $('.tokenized-popup').live('click', function () {
        $("#token-hint-dialog").dialog("open");
        return false;
    });

    $(".token-type").live("click", function () {
        $(this).nextUntil(".token-type").toggleClass("token-collapsed");
    });

});
