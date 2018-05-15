$(function () {

    $("#loading-image").hide();

    if ($("#ratings-template").length > 0) {
        getRatings();

        $("#btnFilter").on("click", function () {

            var dateFrom = $("#txtFrom").val();
            var dateTo = $("#txtTo").val();
            var filterParams = { ratingEnabled: $("#chkRatingEnabled").is(":checked"), onlyRatedPages: $("#chkRatedPages").is(":checked"), applyFilter: true };

            if (dateFrom.length > 0) {
                $.extend(true, filterParams, { dateFrom: dateFrom });
            }

            if (dateTo.length > 0) {
                $.extend(true, filterParams, { dateTo: dateTo });
            }

            getRatings(filterParams);
        });
    }

    if ($("#comments-template").length > 0) {

        var contentId = $("#comments-template").attr("data-contentId");
        getPageComments(contentId);
    }


    function getRatings(filterParams) {

        var url = "/api/rating/pagerating/getratings";
        
        if (filterParams != null) {
            url = url + "?" + $.param(filterParams);
        }

        $("#loading-image").show();
        $.ajax({
                type: "GET",
                url: url
            })
            .done(function(data) {

                $.each(data.ratingData, function(i, obj) {

                    $.extend(true, obj, {
                        checked: obj.ratingEnabled ? "checked" : ""
                    });
                });

                var template = $("#ratings-template").html();
                var info = Mustache.to_html(template, data);
                $("#tablePlaceholder").html(info);
                $("#ratingTable")
                    .DataTable({
                        "columnDefs": [
                            {
                                "targets": [7, 8, 9, 10],
                                "visible": false,
                                "searchable": false
                            },
                            {
                                "targets": [5,6],
                                "searchable": false,
                                "orderable": false
                            }
                        ],
                        "order": [[4, "desc"]],

                        dom: "Bfrtip",
                        buttons: [
                            {
                                extend: "excelHtml5",
                                text: "Export",
                                filename: "PageRatingData",
                                exportOptions: {
                                    columns: [10, 1, 7, 8, 9],
                                    format: {
                                        body: function (data)
                                        {
                                            return data.replace(/{nl}/g, "\n");
                                        }
                                    }
                                }
                            }
                        ]
                    });


                $("#ratingTable").on("click", ".ratingSwitch", function (e) { enableRating(this, e); });

            })
            .fail(function(error) {
                console.log(error);
                //show error
            })
            .always(function() {
                $("#loading-image").hide();
            });
    }

    function getPageComments(contentId) {

        $("#loading-image").show();
        $.ajax({
            type: "GET",
            url: "/api/rating/pagerating/getpagecomments" + "?contentId=" + contentId
    })
            .done(function(data) {

                var template = $("#comments-template").html();
                var info = Mustache.to_html(template, data);
                $("#tablePlaceholder").html(info);
                if (data.ratingData.length) {
                    $("#pageTitle").text(data.ratingData[0].pageName);
                }
            })
            .fail(function(error) {
                console.log(error);
                //show error
            })
            .always(function () {
                $("#loading-image").hide();
            });
    }


    function enableRating(element, e) {

        var urlEnableRating = "/api/rating/pagerating/enablerating";
        var contentId = $(element).attr("data-contentId");

        if (confirm("By changing this value for this page it will be published. Do you want to proceed ?") === true) {

            $.ajax({
                type: "POST",
                url: urlEnableRating,
                data: { contentId: contentId, ratingEnabled: element.checked }
            })
                .done(function (data) {

                })
                .fail(function (error) {
                    console.log(error);
                });
        } else {
            e.preventDefault();
            return false;
        }
        return true;
    }
});

