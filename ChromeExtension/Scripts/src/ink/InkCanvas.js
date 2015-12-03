/// <reference path="brush/BrushStroke.ts"/>
var InkCanvas = (function () {
    function InkCanvas(canvas) {
        this._canvas = canvas;
        this._context = canvas.getContext("2d");
        this._isDrawing = false;
        this._brushStrokes = [];
        this._brush = null;
        this._scrollOffset = { x: 0, y: 0 };
    }
    InkCanvas.prototype.drawStroke = function (stroke, brush) {
        if (brush)
            this._brush = brush;
        this._scrollOffset = { x: 0, y: 0 };
        this._isDrawing = true;
        this._activeStroke = new BrushStroke(this._brush, new Stroke());
        this._activeStroke.stroke.documentOffsetY = window.pageYOffset;
        var first = stroke.points[0];
        var last = stroke.points[stroke.points.length - 1];
        this.startDrawing(first.x, first.y, brush);
        for (var i = 1; i < stroke.points.length - 2; i++) {
            this.draw(stroke.points[i].x, stroke.points[i].y);
        }
        this.endDrawing(last.x, last.y);
    };
    InkCanvas.prototype.startDrawing = function (x, y, brush) {
        if (brush)
            this._brush = brush;
        this._brush.init(x, y, this);
        this._scrollOffset = { x: 0, y: 0 };
        this._isDrawing = true;
        this._activeStroke = new BrushStroke(this._brush, new Stroke());
        this._activeStroke.stroke.documentOffsetY = window.pageYOffset;
        this.draw(x, y);
    };
    InkCanvas.prototype.draw = function (x, y) {
        if (this._isDrawing == false)
            return;
        this._activeStroke.stroke.points.push({ x: x, y: y });
        this._brush.draw(x, y, this);
    };
    InkCanvas.prototype.endDrawing = function (x, y) {
        this.draw(x, y);
        this._isDrawing = false;
        this._brushStrokes.push(this._activeStroke);
    };
    InkCanvas.prototype.addBrushStroke = function (brushStroke) {
        if (this._brushStrokes.indexOf(brushStroke) == -1)
            this._brushStrokes.push(brushStroke);
    };
    InkCanvas.prototype.removeBrushStroke = function (brushStroke) {
        var index = this._brushStrokes.indexOf(brushStroke);
        if (index > -1) {
            this._brushStrokes.splice(index, 1);
            return true;
        }
        return false;
        console.log("couldn't remove element");
    };
    InkCanvas.prototype.update = function () {
        this._scrollOffset = { x: window.pageXOffset, y: window.pageYOffset };
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        for (var i = 0; i < this._brushStrokes.length; i++) {
            this._brushStrokes[i]["brush"].drawStroke(this._brushStrokes[i]["stroke"], this);
        }
    };
    InkCanvas.prototype.setBrush = function (brush) {
        this._brush = brush;
        if (this._isDrawing) {
            this._activeStroke.brush = brush;
            var p = this._activeStroke.stroke.points[0];
            this._brush.init(p.x, p.y, this);
        }
    };
    InkCanvas.prototype.redrawActiveStroke = function () {
        this.update();
        this._activeStroke.brush.drawStroke(this._activeStroke.stroke, this);
    };
    InkCanvas.prototype.removeStroke = function () {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        this.update();
    };
    InkCanvas.prototype.hide = function () {
        console.log("hide canvas");
    };
    InkCanvas.prototype.reveal = function () {
        console.log("reveal canvas");
    };
    return InkCanvas;
})();
