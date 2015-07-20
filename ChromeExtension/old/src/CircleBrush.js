/**
 * Created by phili_000 on 7/4/2015.
 */

var CircleBrush = (function () {

    function CircleBrush() {

    }

    CircleBrush.prototype.init = function (x, y, inkCanvas) {

    };

    CircleBrush.prototype.draw = function (x, y, inkCanvas) {
       // inkCanvas._context.drawImage(this._img, x - inkCanvas._scrollOffset.x - 15, y - inkCanvas._scrollOffset.y - 15, 30, 30);
        inkCanvas._context.fillStyle = "#c82124"; //red
        inkCanvas._context.beginPath();
        inkCanvas._context.arc(x,y,3,0,2*Math.PI);
        inkCanvas._context.closePath();
        inkCanvas._context.fill();
    };

    CircleBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        for(var i=0;i<stroke.points.length;i++) {
            var p = stroke.points[i];
            inkCanvas._context.fillStyle = "#c82124"; //red
            inkCanvas._context.beginPath();
            inkCanvas._context.arc(p.x, p.y,3,0,2*Math.PI);
            inkCanvas._context.closePath();
            inkCanvas._context.fill();
        }
    };

    return CircleBrush;
})();