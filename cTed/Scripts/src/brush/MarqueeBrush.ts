
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
}
