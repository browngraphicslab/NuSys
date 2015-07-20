/**
 * Created by phili_000 on 7/4/2015.
 */

var SelectionBrush = (function () {

    function SelectionBrush(rect) {
        this._rect = rect;
    }

    SelectionBrush.prototype.init = function (x, y, inkCanvas) {
    };

    SelectionBrush.prototype.draw = function (x, y, inkCanvas) {
        console.log("not implemented")
    };

    SelectionBrush.prototype.drawStroke = function (stroke, inkCanvas) {

        if (this._rect != null) {
            console.log(this._rect);
            stroke = new Stroke();
            stroke.points.push({x: this._rect.x, y: this._rect.y});
            stroke.points.push({x: this._rect.x + this._rect.w, y: this._rect.y + this._rect.h});
        }

        var startX = stroke.points[0].x;
        var startY = stroke.points[0].y;
        var w = stroke.points[stroke.points.length-1].x -startX;
        var h = stroke.points[stroke.points.length-1].y - startY;

        startX = startX - inkCanvas._scrollOffset.x + stroke.documentOffsetX;
        startY = startY - inkCanvas._scrollOffset.y + stroke.documentOffsetY;

        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.globalAlpha = 0.6;
        ctx.beginPath();
        ctx.fillStyle = "rgb(222,214,0)";
        ctx.fillRect(startX, startY, w , h);
        ctx.fill();
    };

    return SelectionBrush;
})();