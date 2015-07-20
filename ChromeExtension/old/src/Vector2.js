// based on from http://evanw.github.io/lightgl.js/docs/vector.html
var Vector2 = (function () {

    function Vector2(x, y) {
        this.x = x || 0;
        this.y = y || 0;
    }

    Vector2.fromPoint = function( p ) {
        return new Vector2(p.x, p.y);
    };

    Vector2.prototype = {
        negative: function () {
            return new Vector(-this.x, -this.y);
        },
        add: function (v) {
            if (v instanceof Vector2) return new Vector2(this.x + v.x, this.y + v.y);
            else return new Vector(this.x + v, this.y + v);
        },
        subtract: function (v) {
            if (v instanceof Vector2) return new Vector2(this.x - v.x, this.y - v.y);
            else return new Vector2(this.x - v, this.y - v);
        },
        multiply: function (v) {
            if (v instanceof Vector2) return new Vector2(this.x * v.x, this.y * v.y);
            else return new Vector2(this.x * v, this.y * v);
        },
        divide: function (v) {
            if (v instanceof Vector2) return new Vector2(this.x / v.x, this.y / v.y);
            else return new Vector2(this.x / v, this.y / v);
        },
        equals: function (v) {
            return this.x == v.x && this.y == v.y;
        },
        dot: function (v) {
            return this.x * v.x + this.y * v.y;
        },
        length: function () {
            return Math.sqrt(this.dot(this));
        },
        getNormalized: function () {
            return this.divide(this.length());
        },
        min: function () {
            return Math.min(this.x, this.y);
        },
        max: function () {
            return Math.max(this.x, this.y);
        },
        distanceTo: function(other) {
            return Math.sqrt((this.x - other.x) * (this.x - other.x) + (this.y - other.y)*(this.y - other.y));
        },
        toArray: function (n) {
            return [this.x, this.y].slice(0, n || 2);
        },
        cross: function (other) {
            return this.x * other.y - this.y * other.x ;
        },
        clone: function () {
            return new Vector2(this.x, this.y);
        },
        angleTo: function(a) {
            return Math.acos(this.dot(a) / (this.length() * a.length()));
        },
        init: function (x, y) {
            this.x = x;
            this.y = y;
            return this;
        }
    };

    return Vector2;
})();