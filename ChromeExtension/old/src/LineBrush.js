/**
 * Created by phili_000 on 7/4/2015.
 */

var LineBrush = (function () {

    function LineBrush() {

    }

    LineBrush.prototype.init = function (x, y, inkCanvas) {
        inkCanvas._context.beginPath();
        inkCanvas._context.globalAlpha = 1;
        inkCanvas._context.globalCompositeOperation = "source-over";
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.moveTo(x, y);
    };

    LineBrush.prototype.draw = function (x, y, inkCanvas) {
       // inkCanvas._context.drawImage(this._img, x - inkCanvas._scrollOffset.x - 15, y - inkCanvas._scrollOffset.y - 15, 30, 30);
        inkCanvas._context.globalAlpha = 1;
        inkCanvas._context.globalCompositeOperation = "source-over";
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.lineTo(x, y);
        inkCanvas._context.stroke();
        inkCanvas._context.moveTo(x, y);
    };

    LineBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        var first = stroke.points[0];
        inkCanvas._context.beginPath();
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.moveTo(first.x, first.y);

        for(var i=1;i<stroke.points.length;i++) {
            var p = stroke.points[i];
            inkCanvas._context.lineTo(p.x, p.y);
            inkCanvas._context.stroke();
            inkCanvas._context.moveTo(p.x, p.y);
        }
    };

    return LineBrush;
})();