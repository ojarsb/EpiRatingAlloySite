define([
        "dojo/_base/declare",
        "dijit/_Widget",
        "dijit/_TemplatedMixin",
        "dojo/text!./templates/RatingProperty.html"
    ],
    function(declare, _Widget, _TemplatedMixin, template) {
        return declare("ratingModule.RatingProperty",
        [_Widget, _TemplatedMixin],
        {
            templateString: template,
            postCreate: function() {

                var contextService = epi.dependency.resolve("epi.shell.ContextService");
                var currentContext = contextService.currentContext;
                var res = currentContext.id.split("_");
                var currentContentId = res[0];
                this._getRatingData(currentContentId);
            },
            _getRatingData: function (contentId) {

                var that = this;

                $.ajax({ url: "/api/rating/getpagecomments" + "?contentId=" + contentId, type: "GET" })
                    .done(function(data) {
                        if (data.ratingData.length > 0) {

                            var ratingDataObj = data.ratingData[0];
                            var ratingDataHtml = "<tr>";

                            ratingDataHtml += "<td>" + ratingDataObj.ratingCount + "</td>";
                            ratingDataHtml += "<td>" + ratingDataObj.rating + "</td>";
                            ratingDataHtml += "<td>" + ratingDataObj.positiveRatingCount + "</td>";
                            ratingDataHtml += "<td>" + ratingDataObj.negativeRatingCount + "</td>";
                            ratingDataHtml += "<td>" + ratingDataObj.lastCommentDateFormatted + "</td>";
                            that.ratingsTable.innerHTML = ratingDataHtml + "</tr>";

                            var commentDataHtml = "";

                            if (ratingDataObj.comments.length > 0) {
                                $.each(ratingDataObj.comments,
                                   function(i, obj) {

                                       commentDataHtml += "<tr><td>" +
                                           obj.commentDateFormated +
                                           "</td><td>" +
                                           obj.commentText +
                                           "</td></tr>";
                                   });
                                that.pageComments.innerHTML = commentDataHtml;   
                            } else {
                                $("#commentsTbl").hide();
                            }
                        }
                    })
                    .fail(function(error) {
                        console.log(error);
                    });
            }
        });
    });