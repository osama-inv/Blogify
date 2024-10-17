$(document).ready(function () {
    // Handle click on the post-options element
    $('.post-options').click(function (e) {
        // Prevent event from bubbling up to the document click
        e.stopPropagation();

        // Hide all options-list elements except the current one
        $('.options-list').removeClass('showflex');

        // Show the options-list related to the clicked post-options
        var $optionsList = $(this).closest('.headPost').find('.options-list');
        $optionsList.addClass('showflex');
    });

    // Handle click anywhere on the document
    $(document).click(function () {
        // Hide all options-list elements
        $('.options-list').removeClass('showflex');
    });

    // Prevent document click from hiding options-list when clicking inside it
    $('.options-list').click(function (e) {
        e.stopPropagation();
    });
});
