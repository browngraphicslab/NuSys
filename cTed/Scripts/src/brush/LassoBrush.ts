
class LassoBrush implements IBrush {

    _img: HTMLImageElement;
    _startX: number;
    _startY: number;

    constructor() {

    }

    draw(x: number, y: number, inkCanvas: InkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";

       
        ctx.fillStyle = '#ff0000';
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.arc(x, y, 5, 0, Math.PI * 2, false);
        ctx.fill();
        console.log("--X HASH KEY: " + Math.floor(x / 3));
    }

    redraw(stroke: Stroke, inkCanvas: InkCanvas) {
        inkCanvas.removeStroke();
        for (var i = 0; i < stroke.points.length; i++) {
            this.draw(stroke.points[i].x, stroke.points[i].y, inkCanvas);
        }
    }

    //draw previous on hover
    drawPrevious(stroke: Stroke, inkCanvas: InkCanvas) {
        console.log("======DRAWPREV");
        var firstPoint = stroke.points[0];
        var lastPoint = stroke.points[stroke.points.length - 1];

        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";

        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.strokeStyle = "rgb(255,70,70)";
        ctx.setLineDash([5]);
        ctx.rect(firstPoint.x, firstPoint.y - $(window).scrollTop(), lastPoint.x - firstPoint.x, lastPoint.y - firstPoint.y);
        ctx.stroke();
    }

}
