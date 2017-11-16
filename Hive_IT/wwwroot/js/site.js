$(function () {
    $('#main-content').on('click', '.pager a', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;
    });

});

$(function () {
    $('#main-content').on('click', '.sorting-buttons a', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;
    });
});

$(function () {
    $('#main-content').on('click', '.customer-profile a', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;

    });
});

$(function () {
    $('#main-content').on('click', '.edit-name-cancel', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;

    });
});
