/// <reference path="../../typings/jquery/jquery.d.ts"/>
/// <reference path="../ink/InkCanvas.ts"/>
/// <reference path="../ink/brush/BrushStroke.ts"/>
/// <reference path="../ink/brush/HighlightBrush.ts"/>
/// <reference path="../ink/brush/SelectionBrush.ts"/>
/// <reference path="../ink/brush/LineBrush.ts"/>
/// <reference path="../util/Rectangle.ts"/>
/// <reference path="../util/DomUtil.ts"/>
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var UnknownSelection = (function (_super) {
    __extends(UnknownSelection, _super);
    function UnknownSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "UnknownSelection");
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        if (fromActiveStroke) {
            inkCanvas.setBrush(new LineBrush());
        }
    }
    UnknownSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new LineBrush());
    };
    UnknownSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    UnknownSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
    };
    UnknownSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    UnknownSelection.prototype.getBoundingRect = function () {
        return this._brushStroke.stroke.getBoundingRect();
    };
    UnknownSelection.prototype.analyzeContent = function () {
    };
    UnknownSelection.prototype.getContent = function () {
        return null;
    };
    return UnknownSelection;
})(AbstractSelection);
