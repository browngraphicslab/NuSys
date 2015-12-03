var HighlightBrush = (function () {
    function HighlightBrush() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }
    HighlightBrush.prototype.init = function (x, y, inkCanvas) {
    };
    HighlightBrush.prototype.draw = function (x, y, inkCanvas) {
        inkCanvas._context.globalCompositeOperation = "xor";
        inkCanvas._context.globalAlpha = 0.6;
        inkCanvas._context.drawImage(this._img, x - 15, y - 15, 30, 30);
    };
    HighlightBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        for (var i = 0; i < stroke.points.length; i++) {
            var p = stroke.points[i];
            inkCanvas._context.globalCompositeOperation = "xor";
            inkCanvas._context.globalAlpha = 0.6;
            inkCanvas._context.drawImage(this._img, p.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX - 15, p.y + stroke.documentOffsetY - inkCanvas._scrollOffset.y - 15, 30, 30);
        }
    };
    return HighlightBrush;
})();
