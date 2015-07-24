/// <reference path="../../typings/jquery/jquery.d.ts"/>
/// <reference path="../ink/InkCanvas.ts"/>
/// <reference path="../ink/brush/BrushStroke.ts"/>
/// <reference path="../ink/brush/HighlightBrush.ts"/>
/// <reference path="../ink/brush/SelectionBrush.ts"/>
/// <reference path="../ink/brush/LineBrush.ts"/>
/// <reference path="../util/Rectangle.ts"/>
/// <reference path="../util/DomUtil.ts"/>


class UnknownSelection implements ISelection {
    
    _brushStroke:BrushStroke;
    _inkCanvas:InkCanvas;

    constructor(inkCanvas: InkCanvas, fromActiveStroke:boolean = false) {
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;

        if (fromActiveStroke) {
            inkCanvas.setBrush(new LineBrush());
        }
    }

    start(x: number, y: number): void {
        this._inkCanvas.startDrawing(x, y, new LineBrush());
    }

    update(x: number, y: number): void {
        this._inkCanvas.draw(x, y);
    }

    end(x: number, y: number): void {
        this._inkCanvas.endDrawing(x, y);
    }

    deselect(): void {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    }

    getBoundingRect(): Rectangle {        
        return this._brushStroke.stroke.getBoundingRect();
    }

    analyzeContent(): void {
        // nothing to analyze.
    }

    getContent(): string {
        return null;
    }
}