var MarqueeBrush = (function () {
    function MarqueeBrush(x, y) {
        this._startX = x;
        this._startY = y;
    }
    MarqueeBrush.prototype.init = function (x, y, inkCanvas) {
        inkCanvas._context.lineWidth = 4;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'butt';
    };
    MarqueeBrush.prototype.draw = function (x, y, inkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.beginPath();
        ctx.lineWidth = 5;
        ctx.strokeStyle = "rgb(222,214,0)";
        ctx.setLineDash([6]);
        ctx.rect(this._startX, this._startY, x - this._startX, y - this._startY);
        ctx.stroke();
    };
    MarqueeBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        var firstPoint = stroke.points[0];
        var lastPoint = stroke.points[stroke.points.length - 1];
        this._startX = firstPoint.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX;
        this._startY = firstPoint.y - inkCanvas._scrollOffset.y + stroke.documentOffsetY;
        this.draw(lastPoint.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX, lastPoint.y - inkCanvas._scrollOffset.y + stroke.documentOffsetY, inkCanvas);
    };
    return MarqueeBrush;
})();
