var StrokeClassifier = (function () {
    function StrokeClassifier() {}


    StrokeClassifier.GetStrokeType = function (stroke) {

        var p0 = stroke.points[0];
        var p1 = stroke.points[stroke.points.length - 1];

        var metrics = stroke.getStrokeMetrics();

       // console.log(metrics.error)

        if ( metrics.error > 20) {
            return StrokeType.SCRIBBLE;
        }
        if (Math.abs(p1.y - p0.y) < 20) {
            return StrokeType.LINE;
        }
        if (Math.abs(p1.x - p0.x) < 20) {
            return StrokeType.BRACKET;
        }
        if (Math.abs(p1.x - p0.x) > 50 && Math.abs(p1.y - p0.y) > 20) {
            return StrokeType.MARQUEE;
        }

    };
    return StrokeClassifier;

})();

var StrokeType= (function () {
    function StrokeType() {}

    StrokeType.LINE = "line";
    StrokeType.BRACKET = "bracket";
    StrokeType.MARQUEE = "marquee";
    StrokeType.SCRIBBLE = "scribble";

    return StrokeType;
})();