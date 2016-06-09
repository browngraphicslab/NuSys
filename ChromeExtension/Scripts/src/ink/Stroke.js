var Stroke = (function () {
    function Stroke() {
        this.documentOffsetX = 0;
        this.documentOffsetY = 0;
        this.points = new Array();
    }
    Stroke.fromPoints = function (points) {
        var stroke = new Stroke();
        stroke.points = points.slice(0);
        return stroke;
    };
    Stroke.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this.points.length; i++) {
            var p = this.points[i];
            maxY = p.y > maxY ? p.y : maxY;
            maxX = p.x > maxX ? p.x : maxX;
            minX = p.x < minX ? p.x : minX;
            minY = p.y < minY ? p.y : minY;
        }
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    };
    Stroke.prototype.breakUp = function () {
        var segments = new Array();
        for (var i = 1; i < this.points.length; i++) {
            var p0 = this.points[i - 1];
            var p1 = this.points[i];
            segments.push(Line.fromPoint(p0, p1));
        }
        return segments;
    };
    Stroke.prototype.getResampled = function (samples) {
        var c = this.getCopy();
        c.resample(samples);
        return c;
    };
    Stroke.prototype.getEntropy = function () {
        var angles = [];
        for (var i = 1; i < this.points.length; i++) {
            var v0 = new Vector2(this.points[i - 1].x, this.points[i - 1].y);
            var v1 = new Vector2(this.points[i].x, this.points[i].y);
            angles.push(v0.angleTo(v1));
        }
    };
    Stroke.prototype.getStrokeMetrics = function () {
        var startPoint = Vector2.fromPoint(this.points[0]);
        var endPoint = Vector2.fromPoint(this.points[this.points.length - 1]);
        var l = endPoint.subtract(startPoint);
        var ln = l.getNormalized();
        var error = 0;
        var errors = [];
        for (var i = 0; i < this.points.length; i++) {
            var a = Vector2.fromPoint(this.points[i]).subtract(startPoint);
            var b = ln.multiply(a.dot(ln));
            var c = a.subtract(b);
            error += Math.abs(c.length());
            errors.push(Math.abs(c.length()));
        }
        function median(values) {
            values.sort(function (a, b) { return a - b; });
            var half = Math.floor(values.length / 2);
            if (values.length % 2)
                return values[half];
            else
                return (values[half - 1] + values[half]) / 2.0;
        }
        var m = median(errors);
        error /= this.points.length;
        return { length: this.points.length, error: m };
    };
    Stroke.prototype.resample = function (numSamples) {
        var oldSamples = this.points;
        var scale = numSamples / oldSamples.length;
        var newSamples = new Array(numSamples);
        var radius = scale > 1 ? 1 : 1 / (2 * scale);
        var startX = oldSamples[0].x;
        var deltaX = oldSamples[oldSamples.length - 1].x - startX;
        for (var i = 0; i < numSamples; ++i) {
            var center = i / scale + (1.0 - scale) / (2.0 * scale);
            var left = Math.ceil(center - radius);
            var right = Math.floor(center + radius);
            var sum = 0;
            var sumWeights = 0;
            for (var k = left; k <= right; k++) {
                var weight = this.g(k - center, scale);
                var index = Math.max(0, Math.min(oldSamples.length - 1, k));
                sum += weight * oldSamples[index].y;
                sumWeights += weight;
            }
            sum /= sumWeights;
            newSamples[i] = { x: startX + i / numSamples * deltaX, y: sum };
        }
        this.points = newSamples.slice(0);
    };
    Stroke.prototype.g = function (x, a) {
        var radius;
        if (a < 1)
            radius = 1.0 / a;
        else
            radius = 1.0;
        if ((x < -radius) || (x > radius))
            return 0;
        else
            return (1 - Math.abs(x) / radius) / radius;
    };
    Stroke.prototype.getCopy = function () {
        var s = new Stroke();
        s.points = this.points.slice(0);
        s.documentOffsetX = this.documentOffsetX;
        s.documentOffsetY = this.documentOffsetY;
        return s;
    };
    return Stroke;
})();
