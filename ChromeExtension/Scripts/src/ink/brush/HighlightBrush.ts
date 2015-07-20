class HighlightBrush implements IBrush {

    _img:HTMLImageElement;

    constructor() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }

    init(x: number, y: number, inkCanvas: InkCanvas): void {
        // do nothing
    }

    draw(x: number, y: number, inkCanvas: InkCanvas) {
        inkCanvas._context.globalCompositeOperation = "xor";
        inkCanvas._context.globalAlpha = 0.6;
        inkCanvas._context.drawImage(this._img, x - 15, y - 15, 30, 30);
    }

    drawStroke(stroke: Stroke, inkCanvas: InkCanvas) {
        for (var i = 0; i < stroke.points.length; i++) {
            var p = stroke.points[i];
            inkCanvas._context.globalCompositeOperation = "xor";
            inkCanvas._context.globalAlpha = 0.6;
            inkCanvas._context.drawImage(this._img, p.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX - 15, p.y + stroke.documentOffsetY - inkCanvas._scrollOffset.y - 15, 30, 30);
        }
    }
}