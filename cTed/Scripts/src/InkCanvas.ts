/// <reference path="Stroke.ts"/>
/// <reference path="StrokeType.ts"/>


class InkCanvas {

    _canvas: HTMLCanvasElement;
    _brush: IBrush;
    _context: CanvasRenderingContext2D;
    _activeStroke: Stroke; 
    _prevBrush: IBrush; 

    constructor(canvas: HTMLCanvasElement) {
        this._canvas = canvas;
        this._context = canvas.getContext("2d");
        this._brush = new HighlightBrush();
        this._activeStroke = new Stroke();
        console.log("new Canvas!!!!!");
    }


    draw(x: number, y: number) {
        this._activeStroke.push(x,y);
        this._brush.draw(x, y, this);
        console.log("draw ................... at " + x + ":" + y);
    }

    removeStroke(): void {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
     //   this.update();
    }

    switchBrush(strokeType) {
        console.log("INKCANVAS brush switched to : " + strokeType);
        switch (strokeType) {
            //////STROKE TYPE CHANGE 
            case StrokeType.Marquee:
                this._brush = new MarqueeBrush();
                break;
            case StrokeType.Line:
                this._brush = new HighlightBrush();
                break;
            default:
                this._brush = new HighlightBrush();
        }
        this._brush.redraw(this._activeStroke, this);
    }

    drawPreviousGestureM(stroke: Stroke) {
        console.log("======================redraw==");
        this._prevBrush = new MarqueeBrush();
        this._prevBrush.drawPrevious(stroke, this);
    }

    clear(): void {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        this._activeStroke = new Stroke();
        this._brush = new HighlightBrush();
    }
}