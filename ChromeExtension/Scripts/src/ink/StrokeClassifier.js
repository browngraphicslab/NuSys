var GestireClassifier = (function () {
    function GestireClassifier() {
    }
    GestireClassifier.getGestureType = function (stroke) {
        var p0 = stroke.points[0];
        var p1 = stroke.points[stroke.points.length - 1];
        var metrics = stroke.getStrokeMetrics();
        if (Math.abs(p1.x - p0.x) < 5 && Math.abs(p1.y - p0.y) < 5) {
            return GestureType.Null;
        }
        if (Math.abs(p1.y - p0.y) < 20) {
            return GestureType.Horizontal;
        }
        if (Math.abs(p1.x - p0.x) < 20) {
            return GestureType.Vertical;
        }
        if (Math.abs(p1.x - p0.x) > 50 && Math.abs(p1.y - p0.y) > 20) {
            return GestureType.Diagonal;
        }
    };
    return GestireClassifier;
})();
