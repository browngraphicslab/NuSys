var DEMO = (function ($) {
    'use strict';

    _.templateSettings.variable = "rc";
    // Grab the HTML out of our template tag and pre-compile it.



    // Define our render data (to be put into the "rc" variable).


    


    var $grid = $('#grid'),
        $filterOptions = $('.filter-options'),
        $sizer = $grid.find('.shuffle__sizer'),

    init = function () {
        

        // None of these need to be executed synchronously
        setTimeout(function () {
            listen();
            setupFilters();
            setupSorting();
            setupSearching();
        }, 100);

        // You can subscribe to custom events.
        // shrink, shrunk, filter, filtered, sorted, load, done
        $grid.on('loading.shuffle done.shuffle shrink.shuffle shrunk.shuffle filter.shuffle filtered.shuffle sorted.shuffle layout.shuffle', function (evt, shuffle) {
            // Make sure the browser has a console
            if (window.console && window.console.log && typeof window.console.log === 'function') {
                console.log('Shuffle:', evt.type);
            }
        });

        // instantiate the plugin
        $grid.shuffle({
            itemSelector: '.picture-item',
            sizer: $sizer
        });

        // Destroy it! o_O
        // $grid.shuffle('destroy');
    },

    // Set up button clicks
    setupFilters = function () {
        var $btns = $filterOptions.children();
        $btns.on('click', function () {
            var $this = $(this),
                isActive = $this.hasClass('active'),
                group = isActive ? 'all' : $this.data('group');

            // Hide current label, show current label in title
            if (!isActive) {
                $('.filter-options .active').removeClass('active');
            }

            $this.toggleClass('active');

            // Filter elements
            $grid.shuffle('shuffle', group);
        });

        $btns = null;
    },

    setupSorting = function () {
        // Sorting options
        $('.sort-options').on('change', function () {
            var sort = this.value,
                opts = {};

            // We're given the element wrapped in jQuery
            if (sort === 'date-created') {
                opts = {
                    reverse: true,
                    by: function ($el) {
                        return $el.data('date-created');
                    }
                };
            } else if (sort === 'title') {
                opts = {
                    by: function ($el) {
                        return $el.data('title').toLowerCase();
                    }
                };
            }

            // Filter elements
            $grid.shuffle('sort', opts);
        });
    },

    setupSearching = function () {
        // Advanced filtering
        $('.js-shuffle-search').on('keyup change', function () {
            var val = this.value.toLowerCase();
            $grid.shuffle('shuffle', function ($el, shuffle) {

                // Only search elements in the current group
                if (shuffle.group !== 'all' && $.inArray(shuffle.group, $el.data('groups')) === -1) {
                    return false;
                }

                var text = $.trim($el.find('.picture-item__title').text()).toLowerCase();
                var text2 = $.trim($el.find('.html_content').text()).toLowerCase();
                return text.indexOf(val) !== -1 || text2.indexOf(val) !== -1;
            });
        });
    },

    // Re layout shuffle when images load. This is only needed
    // below 768 pixels because the .picture-item height is auto and therefore
    // the height of the picture-item is dependent on the image
    // I recommend using imagesloaded to determine when an image is loaded
    // but that doesn't support IE7
    listen = function () {
        var debouncedLayout = $.throttle(300, function () {
            $grid.shuffle('update');
        });

        // Get all images inside shuffle
        $grid.find('img').each(function () {
            var proxyImage;

            // Image already loaded
            if (this.complete && this.naturalWidth !== undefined) {
                return;
            }

            // If none of the checks above matched, simulate loading on detached element.
            proxyImage = new Image();
            $(proxyImage).on('load', function () {
                $(this).off('load');
                debouncedLayout();
            });

            proxyImage.src = this.src;
        });

        // Because this method doesn't seem to be perfect.
        setTimeout(function () {
            debouncedLayout();
        }, 500);
    };

    return {
        init: init
    };
}(jQuery));



$(document).ready(function () {

    var template = _.template(
    $("script.template").html()
);

    var templateData = {
        listItems: []

    };

    chrome.storage.local.get(function (cTedStorage) {

        var allTags = {};

        cTedStorage.selections.forEach(function (val, index) {

            var item = {
                url: val.url,
                displayUrl: val.url.split("//")[1],
                groups: ["group1", "group2"],
                timestamp: val.id,
                title: "title",
                tagArray: val.tags.split(","),
                tags: null,
                html: val._content
            };

            var tagString = "[";
            val.tags.split(",").forEach(function (tag) {
                allTags[tag] = true;
                tagString += '"' + tag + '",';
            });
            tagString = tagString.substring(0, tagString.length - 1) + ']';
            item.tags = tagString;



            var tpl = $(template(item));
            $(tpl).find(".contentframe")[0].srcdoc = ""
                + '<html><head>' +
                '<link href="https://fonts.googleapis.com/css?family=Open+Sans:400,300" rel="stylesheet" type="text/css">'
                + '<style> '
                + 'body { font-family: "Open Sans", sans-serif; font-size:75%;} ' +
                "::-webkit-scrollbar { width: 7px; } ::-webkit-scrollbar-track { -webkit-box-shadow: inset 0 0 3px rgba(0,0,0,0.3); -webkit-border-radius: 10px; border-radius: 10px; } ::-webkit-scrollbar-thumb { -webkit-border-radius: 10px; border-radius: 10px; background: rgba(48,48,48,0.4); -webkit-box-shadow: inset 0 0 6px rgba(0,0,0,0.5); } ::-webkit-scrollbar-thumb:window-inactive { background: rgba(255,0,0,0.4); } "
                + '</style></head><body>'
                + val._content + "</body></html>";

            console.log($(tpl).find(".contentframe"));

            $("#grid").append(tpl);
        });

        for (var t in allTags) {
            if (allTags.hasOwnProperty(t)) {
                $(".filter-options").append(
                    $('<button class="btn btn--warning" data-group="' + t + '">' + t +  '</button>')
                );
            }
        }

        
   
        DEMO.init();

    });


    // Render the underscore template and inject it after the H1
    // in our current DOM.

});
