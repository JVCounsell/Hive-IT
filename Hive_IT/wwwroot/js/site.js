$(document).ready(function () {    
    //when a pager link is clicked (AKA a Next or Previous page button) get
    //the designated href and the load it into main-content (content within cshtmls)
    $('#main-content').on('click', '.pager a', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;
    });

    //similar but for sorting buttons
    $('#main-content').on('click', '.sorting-buttons a', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;
    });

    //for all the little add, edit, etc buttons on customer-profile except back to list: prevents url from changing in address bar    
    $('#main-content').on('click', '.customer-profile a', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;

    });

    //if cancel is clicked return to profile, prevents url from changing in address bar
    $('#main-content').on('click', '.edit-name-cancel', function () {

        var url = $(this).attr('href');

        $('#main-content').load(url);

        return false;

    });
       
    // when manufacturer-select changes, call anonymous function
    $('#manufacturer-select').on('change', function () {
        AlterSelect();
    });   

    //function that changes values of select list for services when checkbox altered
    $('#service-checkbox').on('click', function () {        
        if ($('#service-checkbox').prop('checked')) {
            //empty options, then fill it with option designated for unspecific service, finally disable select
            $('#manufacturer-select').empty();
            $('#manufacturer-select').append($("<option></option>")
                .attr("value", 'Not Manufacturer Specific').text('Not Manufacturer Specific'));
            $('#manufacturer-select').prop('disabled', true);

            $('#model-select').empty();
            $('#model-select').append($("<option></option>")
                .attr("value", 'Not Model Specific').text('Not Model Specific'));
            $('#model-select').prop('disabled', true);
        } else {
            $.ajax({
                url: '/Services/ReturnManufacturersAndModels',

                success: function (data) {

                    var manuTarget = $('#manufacturer-select');
                    var modTarget = $('#model-select');

                    //empty select lists
                    manuTarget.empty();
                    modTarget.empty();

                    //conversion to proper JSON object
                    var jsonObjects = JSON.parse(data);

                    //create a options in the select items with values as was populated from the action
                    //and grab it from the Json object
                    $.each(jsonObjects.Manufacturers, function (index, value) {
                        manuTarget.append($("<option></option>")
                            .attr("value", value).text(value));
                        manuTarget.prop('disabled', false);
                    });

                    $.each(jsonObjects.Models, function (index, value) {
                        modTarget.append($("<option></option>")
                            .attr("value", value).text(value));
                        modTarget.prop('disabled', false);
                    });
                }
            });
        }
    });

    //function that calls a controller via ajax and changes a select based off of first
    function AlterSelect() {
        $.ajax({
            url: '/WorkOrder/ReturnLinkedModels',
            data: { manufacturer: $('#manufacturer-select').val() },

            success: function (data) {

                //set the model select as a target for ease of use
                var target = $('#model-select');
                //remove the previous options
                target.empty();

                //conversion of Json string into Json
                var objects = JSON.parse(data);

                //for each element in the options.models do whatever in function passing in key -> index(0, 1, ..) and the actual values
                $.each(objects.Models, function (index, value) {
                    //add a new option with the value attribute set to the value and display text to value
                    target.append($("<option></option>")
                        .attr("value", value).text(value));
                });
            }
        });
    };

    $('#status-select').on('change', function () {
        $.ajax({
            type: "POST",
            url: '/WorkOrder/Details',
            data: {
                __RequestVerificationToken: $('#status-form input[name=__RequestVerificationToken]').val(),
                status: $('#status-select').val(),
                order: $('#order-number').val()
            },
            success: function (data) {
                //returns either true or false, if true reload the page, if false(couldn't find number) link to the list
                if (data == true)
                    location.reload();
                else
                    $('#main-content').load('/WorkOrder/List');
            }
        });
    });

    var orderStatus = $('#status-select').val();
    switch (orderStatus){
        case "Created":
            $('#status-select').attr('class', 'status-created');
            break;
        case "Paid":
            $('#status-select').attr('class', 'status-paid');
            break;
        case "Complete":
            $('#status-select').attr('class', 'status-complete');
            break;
        default:
            $('#status-select').attr('class', 'status-created');
            break;
    }  

    //for when the page loads change the color based on status value
    var orderStatus = $('#device-status-select').val();
    switch (orderStatus) {
        case "Diagnosed":
            $('#device-status-select').attr('class', 'form-control').addClass('device-status-diagnosed');
            break;
        case "Being Repaired":
            $('#device-status-select').attr('class', 'form-control').addClass('device-status-beingfix');
            break;
        case "Repaired":
            $('#device-status-select').attr('class', 'form-control').addClass('device-status-repaired');
            break;
        case "Not Fixable":
            $('#device-status-select').attr('class', 'form-control').addClass('device-status-notfix');
            break;
        case "Picked Up":
            $('#device-status-select').attr('class', 'form-control').addClass('device-status-picked');
            break;
        default:
            $('#device-status-select').attr('class', 'form-control');
            break;
    }    
    //change colors on status change
    $('#device-status-select').on('change', function () {
        var orderStatus = $('#device-status-select').val();
        switch (orderStatus) {
            case "Diagnosed":
                $('#device-status-select').attr('class', 'form-control').addClass('device-status-diagnosed');
                break;
            case "Being Repaired":
                $('#device-status-select').attr('class', 'form-control').addClass('device-status-beingfix');
                break;
            case "Repaired":
                $('#device-status-select').attr('class', 'form-control').addClass('device-status-repaired');
                break;
            case "Not Fixable":
                $('#device-status-select').attr('class', 'form-control').addClass('device-status-notfix');
                break;
            case "Picked Up":
                $('#device-status-select').attr('class', 'form-control').addClass('device-status-picked');
                break;
            default:
                $('#device-status-select').attr('class', 'form-control');
                break;
        }    
    });

});