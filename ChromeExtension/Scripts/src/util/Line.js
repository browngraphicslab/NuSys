var Line = (function () {
    function Line() {
    }
    Line.fromPoint = function (start, end) {
        var line = new Line();
        line.start = new Vector2(start.x, start.y);
        line.end = new Vector2(end.x, end.y);
        return line;
    };
    Line.fromVector = function (start, end) {
        var line = new Line();
        line.start = start.clone();
        line.end = end.clone();
        return line;
    };
    Line.prototype.intersectsLine = function (other) {
        var s1_x = this.end.x - this.start.x;
        var s1_y = this.end.y - this.start.y;
        var s2_x = other.end.x - other.start.x;
        var s2_y = other.end.y - other.start.y;
        var s, t;
        s = (-s1_y * (this.start.x - other.start.x) + s1_x * (this.start.y - other.start.y)) / (-s2_x * s1_y + s1_x * s2_y);
        t = (s2_x * (this.start.y - other.start.y) - s2_y * (this.start.x - other.start.x)) / (-s2_x * s1_y + s1_x * s2_y);
        if (s >= 0 && s <= 1 && t >= 0 && t <= 1) {
            return true;
        }
        return false;
    };
    return Line;
})();
