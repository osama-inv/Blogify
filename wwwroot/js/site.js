$(document).ready(function () {

    $('.react').on('click', function () {
        var postId = $(this).data('id');  // Get the post ID from the data attribute
        var value = $(this).data('value'); // Get the value from the data attribute
        var $currentElement = $(this);

        $.ajax({
            url: '/Home/Reactions',  // URL of the controller method
            type: 'POST',  // Use POST method
            data: {
                id: postId,  // Send post ID
                value: value  // Send other data (e.g., some variable)
            },
            success: function (response) {

                $currentElement.siblings('.react').removeClass('ActedReaction');
    
                if ($currentElement.hasClass('ActedReaction')) {
                    $currentElement.removeClass('ActedReaction');
                } else {
                    $currentElement.addClass('ActedReaction');  
                }
            },
            success: function (response) {
                if (response.success) {
                    // Update the likes and dislikes counts
                    $currentElement.closest('.reaction').find('[data-value="like"]').attr('data-Clikes', response.numofLikes).text(response.numofLikes + ' likes');
                    $currentElement.closest('.reaction').find('[data-value="dislike"]').attr('data-Cdislikes', response.numofDislikes).text(response.numofDislikes + ' dislikes');

                    // Remove ActedReaction from siblings
                    $currentElement.siblings('.react').removeClass('ActedReaction');

                    // Toggle ActedReaction class based on user's reaction
                    if ($currentElement.hasClass('ActedReaction')) {
                        $currentElement.removeClass('ActedReaction');
                    } else {
                        $currentElement.addClass('ActedReaction');
                    }
                }
            },
            error: function (xhr, status, error) {
                // Handle errors
                console.log('Error:', error);
            }
        });
    });
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

$(document).ready(function () {
    // Define the observer options
    var options = {
        root: null, // relative to the viewport
        rootMargin: '0px',
        threshold: 1 // Trigger when 50% of the element is in the viewport
    };

    // Callback function to handle intersections (when the element is in view)
    function handleIntersection(entries, observer) {
        entries.forEach(entry => {
            if (entry.isIntersecting) { // Check if the element is in view
                
                var $element = $(entry.target);

                // Check if this element was already viewed (to avoid multiple triggers)
                if (!$element.hasClass('viewed')) {
                    $element.addClass('viewed'); // Mark this element as viewed

                    var postId = $element.find('.react').data('id');

                    $.ajax({
                        url: '/LogSeenBlog',
                        type: 'POST',
                        data: { Blogid: postId },
                        success: function (response) {
                            //
                        },
                        error: function (xhr, status, error) {
                            //
                        }
                    });
                }
            }
        });
    }

    // Create an IntersectionObserver instance
    var observer = new IntersectionObserver(handleIntersection, options);

    // Observe each .bigAll div
    $('.BlogPost').each(function () {
        observer.observe(this);
    });
});