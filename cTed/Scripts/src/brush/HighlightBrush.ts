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

    redraw(stroke: Stroke, inkCanvas: InkCanvas) {

    }

    drawPrevious(stroke: Stroke, inkCanvas: InkCanvas): void { }

}
