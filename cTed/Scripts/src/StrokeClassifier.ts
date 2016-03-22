class StrokeClassifier {

    static getStrokeType(stroke): StrokeType {

        var p0 = stroke.points[0];
        var p1 = stroke.points[stroke.points.length - 1];
        var metrics = stroke.getStrokeMetrics();
  //      console.log("====================================="+metrics.error);
        if (Math.abs(p1.x - p0.x) < 5 && Math.abs(p1.y - p0.y) < 5) {
            return StrokeType.Null;
        }
        if (metrics.error > 50) {
            return StrokeType.Lasso;
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