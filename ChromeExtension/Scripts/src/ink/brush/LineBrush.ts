class LineBrush implements IBrush {

    constructor() {

    }

    init(x: number, y: number, inkCanvas: InkCanvas): void {
        inkCanvas._context.beginPath();
        inkCanvas._context.globalAlpha = 1;
        inkCanvas._context.globalCompositeOperation = "source-over";
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.moveTo(x, y);
    }

    draw(x: number, y: number, inkCanvas: InkCanvas) {
        inkCanvas._context.globalAlpha = 1;
        inkCanvas._context.globalCompositeOperation = "source-over";
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.lineTo(x, y);
        inkCanvas._context.stroke();
        inkCanvas._context.moveTo(x, y);
    }

    drawStroke(stroke: Stroke, inkCanvas: InkCanvas) {
        var first = stroke.points[0];
        inkCanvas._context.beginPath();
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.moveTo(first.x, first.y);

        for (var i = 1; i < stroke.points.length; i++) {
            var p = stroke.points[i];
            inkCanvas._context.lineTo(p.x, p.y);
            inkCanvas._context.stroke();
            inkCanvas._context.moveTo(p.x, p.y);
        }
    }
}