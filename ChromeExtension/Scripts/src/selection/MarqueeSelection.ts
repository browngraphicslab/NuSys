/// <reference path="../ink/brush/MarqueeBrush.ts" />

class MarqueeSelection implements ISelection {

    _brushStroke: BrushStroke;
    _inkCanvas: InkCanvas;
    _startX: number = 0;
    _startY: number = 0;
    _mouseX: number = 0;
    _mouseY: number = 0;
    _marqueeX1: number = 0;
    _marqueeY1: number = 0;
    _marqueeX2: number = 0;
    _marqueeY2: number = 0;
    _parentList: Array<any> = new Array<any>();
    _ct:number = 0;


    constructor(inkCanvas: InkCanvas, fromActiveStroke: boolean = false) {
        this._inkCanvas = inkCanvas;
        
        if (fromActiveStroke) {

            var stroke = inkCanvas._activeStroke.stroke;
            this._startX = stroke.points[0].x;
            this._startY = stroke.points[0].y;
            this._mouseX = stroke.points[stroke.points.length - 1].x;
            this._mouseY = stroke.points[stroke.points.length - 1].y;
            this._ct = 0;
            this._marqueeX1 = this._startX;
            this._marqueeX2 = this._mouseX;
            this._marqueeY1 = this._startY;
            this._marqueeY2 = this._mouseY;
            inkCanvas.setBrush(new MarqueeBrush(this._startX, this._startY));
        }
    }

    start(x: number, y: number): void {
        this._inkCanvas.startDrawing(x, y, new MarqueeBrush(x, y));
        this._parentList = [];
        this._startX = x;
        this._startY = y;
    }

    update(x: number, y: number): void {
        this._mouseX = x;
        this._mouseY = y;

        this._marqueeX1 = this._startX;
        this._marqueeY1 = this._startY;
        this._marqueeX2 = this._mouseX;
        this._marqueeY2 = this._mouseY;

        var canvas = this._inkCanvas._canvas;
        var ctx = this._inkCanvas._context;

        this._inkCanvas.update();
        this._inkCanvas.draw(x, y);

       // this.clearSelection();
    }

    end(x: number, y: number): void {
        var el = document.elementFromPoint(this._startX, this._startY);

        this._parentList.push(el);
       // this._selected = el;

       // this.drawPreviousMarquee();
        if (this._marqueeX1 > this._marqueeX2) {
            var temp = this._marqueeX1;
            this._marqueeX1 = this._marqueeX2;
            this._marqueeX2 = temp;
        }
        if (this._marqueeY1 > this._marqueeY2) {
            var temp = this._marqueeY1;
            this._marqueeY1 = this._marqueeY2;
            this._marqueeY2 = temp;
        }

        //this.getNextElement(el);
        this._inkCanvas.endDrawing(x, y);

        this._brushStroke = this._inkCanvas._activeStroke;
    }

    deselect(): void {

    }

    getBoundingRect(): Rectangle {
        return null;
    }

    analyzeContent(): void {
    }

    getContent(): string {
        return null;
    }

}