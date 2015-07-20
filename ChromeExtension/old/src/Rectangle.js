/**
 * Created by phili_000 on 7/14/2015.
 */

var Rectangle = (function () {

    function Rectangle(x,y,w,h) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }

    Rectangle.prototype.intersectsRectangle = function(r2) {
        return !(r2.x > this.x + this.w ||
        r2.x + r2.w < this.x ||
        r2.y > this.y + this.h ||
        r2.y + r2.h < this.y);
    };

    return Rectangle;
})();