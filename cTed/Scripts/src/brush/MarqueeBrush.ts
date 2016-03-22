
class MarqueeBrush implements IBrush {

    _img: HTMLImageElement;
    _startX: number;
    _startY: number;

    constructor() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }

    draw(x: number, y: number, inkCanvas: InkCanvas) {
        inkCanvas.removeStroke();
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";

        ctx.beginPath();
        ctx.lineWidth = 5;
        ctx.strokeStyle = "rgb(222,214,0)";
        ctx.setLineDash([6]);
        ctx.rect(this._startX, this._startY, x - this._startX, y - this._startY);
        ctx.stroke();
    }



    redraw(stroke: Stroke, inkCanvas: InkCanvas) {
        var firstPoint = stroke.points[0];
        var lastPoint = stroke.points[stroke.points.length - 1];

        this._startX = firstPoint.x;
        this._startY = firstPoint.y;
        this.draw(lastPoint.x, lastPoint.y, inkCanvas);
    }

    focusLine(line: Line, inkCanvas: InkCanvas) {

    }

    focusPoint(p: Point, inkCanvas: InkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";


        ctx.fillStyle = '#0000FF';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.arc(p.x, p.y, 8, 0, Math.PI * 2, false);
        ctx.fill();
    }      

    //draw previous on hover
    drawPrevious(stroke: Stroke, inkCanvas: InkCanvas) {
        console.log("======DRAWPREVMARQUEE");
        console.log(stroke);
        var firstPoint = stroke.points[0];
        var lastPoint = stroke.points[stroke.points.length - 1];

        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";

        ctx.beginPath();
        ctx.lineWidth = 100;
        ctx.strokeStyle = "rgb(0,0,0)";
        ctx.rect(firstPoint.x, firstPoint.y - $(window).scrollTop(), lastPoint.x - firstPoint.x, lastPoint.y - firstPoint.y);
        ctx.stroke();
    }

}
