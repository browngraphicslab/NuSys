class StrokeClassifier {

    static getStrokeType(stroke) {

        var p0 = stroke.points[0];
        var p1 = stroke.points[stroke.points.length - 1];
        var metrics = stroke.getStrokeMetrics();

        if (metrics.error > 20) {
            return StrokeType.Scribble;
        }
        if (Math.abs(p1.y - p0.y) < 20) {
            return StrokeType.Line;
        }
        if (Math.abs(p1.x - p0.x) < 20) {
            return StrokeType.Bracket;
        }
        if (Math.abs(p1.x - p0.x) > 50 && Math.abs(p1.y - p0.y) > 20) {
            return StrokeType.Marquee;
        }

    }
}