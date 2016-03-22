class HighlightBrush implements IBrush{

    _img: HTMLImageElement;

    constructor() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }

    draw(x: number, y: number, inkCanvas: InkCanvas) {
        inkCanvas._context.globalCompositeOperation = "xor";
        inkCanvas._context.globalAlpha = 0.6;
        inkCanvas._context.drawImage(this._img, x - 15, y - 15, 30, 30);
    }
    focusLine(line: Line, inkCanvas: InkCanvas) {

    }
    redraw(stroke: Stroke, inkCanvas: InkCanvas) {

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

    drawPrevious(stroke: Stroke, inkCanvas: InkCanvas): void { }

}
