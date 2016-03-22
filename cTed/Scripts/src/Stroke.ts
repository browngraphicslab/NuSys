/// <reference path = "util/Point.ts"/>
class Stroke {

    public points: Array<any>;
    
    constructor() {
        this.points = new Array<any>();
    }

    push(x: number, y: number):void {
        this.points.push({ x:x, y: y });
    }

    getBoundingRect(): Rectangle {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this.points.length; i++) {
            var p: any = this.points[i];
            maxY = p.y > maxY ? p.y : maxY;
            maxX = p.x > maxX ? p.x : maxX;
            minX = p.x < minX ? p.x : minX;
            minY = p.y < minY ? p.y : minY;
        }

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    nearestPointArea(p: Point): Point {
        console.log("nearestpont for " + p.x + "and " + p.y);
        var xval = Math.floor(p.x / 3);
        var yval = Math.floor(p.y / 3);
        if (Math.abs(p.x / 3 - xval) > 0.5)
            xval++;
        if (Math.abs(p.y / 3 - yval) > 0.5)
            yval++;
        console.log("resulting : " + xval +  " and " + yval);
        return new Point(xval, yval);
    }
    degree(p1, p2: Point): number {
        return Math.atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Math.PI;
    }

    

    getStrokeMetrics() {
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
    }



    sampleStroke(): Stroke {
        var len = this.points.length;
        var ypre;
        var xpre;
        var predg = 0;
        var prept = this.points[0];
        var strokeHash = {};
        var sampledStrokes = [];
        sampledStrokes.push(prept);
        for (var i = 1; i < len; i++) {
        //    var pt = this.nearestPointArea(this.points[i]);
            var pt = this.points[i];
            if (Math.abs(predg - this.degree(pt, prept)) < 10 && i < len-1) {
                continue;
            }
            predg = this.degree(pt, prept);
            sampledStrokes.push(this.points[i]);
        }
        var res = new Stroke();
        res.points = sampledStrokes;
        return res;
    }

}