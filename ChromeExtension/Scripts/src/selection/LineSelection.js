/// <reference path="../../typings/jquery/jquery.d.ts"/>
/// <reference path="../ink/InkCanvas.ts"/>
/// <reference path="../ink/brush/BrushStroke.ts"/>
/// <reference path="../ink/brush/HighlightBrush.ts"/>
/// <reference path="../ink/brush/SelectionBrush.ts"/>
/// <reference path="../util/Rectangle.ts"/>
/// <reference path="../util/DomUtil.ts"/>
/// <reference path="AbstractSelection.ts"/>
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var LineSelection = (function (_super) {
    __extends(LineSelection, _super);
    function LineSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "LineSelection");
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        console.log("making line selection");
        if (fromActiveStroke) {
            this._inkCanvas.setBrush(new HighlightBrush());
            var t = this;
            $.each(inkCanvas._activeStroke.stroke.points, function () {
                t._inkCanvas.draw(this.x, this.y);
            });
        }
    }
    LineSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new HighlightBrush());
    };
    LineSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    LineSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();
        this.select();
        this._inkCanvas.removeBrushStroke(this._brushStroke);
        this._inkCanvas.update();
    };
    LineSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    LineSelection.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this._clientRects.length; i++) {
            var p = this._clientRects[i];
            maxY = p.top + p.height > maxY ? p.top + p.height : maxY;
            maxX = p.left + p.width > maxX ? p.left + p.width : maxX;
            minX = p.left < minX ? p.left : minX;
            minY = p.top < minY ? p.top : minY;
        }
        return new Rectangle(minX, minY + window.pageYOffset, maxX - minX, maxY - minY);
    };
    LineSelection.prototype.addWordTag = function (nodes) {
        var _this = this;
        $.each(nodes, function (index, value) {
            if (value.nodeType == Node.TEXT_NODE) {
                $(value).replaceWith($(value).text().replace(/([^,\s]*)/g, "<word>$1</word>"));
            }
            else if (value.childNodes.length > 0) {
                _this.addWordTag(value.childNodes);
            }
        });
    };
    LineSelection.prototype.analyzeContent = function () {
        console.log("analyzing content.");
        var stroke = this._brushStroke.stroke;
        var pStart = stroke.points[0];
        var pEnd = stroke.points[stroke.points.length - 1];
        var startRange = document["caretRangeFromPoint"](pStart.x, pStart.y);
        var endRange = document["caretRangeFromPoint"](pEnd.x, pEnd.y);
        console.log("----------------");
        console.log(startRange);
        console.log(endRange);
        console.log(startRange.startContainer.nodeValue);
        console.log(startRange.startContainer.nodeValue.substring(0, startRange.startOffset));
        console.log(startRange.startContainer.nodeValue.substring(startRange.startOffset, endRange.endOffset));
        console.log(startRange.startContainer.nodeValue.substring(endRange.endOffset, endRange.endContainer.nodeValue.length));
        this._content = startRange.startContainer.nodeValue.substring(startRange.startOffset, endRange.endOffset);
        var newStart = document.createElement("span");
        newStart.innerHTML = "<span>" + startRange.startContainer.nodeValue.substring(0, startRange.startOffset) + "</span>" + "<span style='background: yellow;'>" + startRange.startContainer.nodeValue.substring(startRange.startOffset, endRange.endOffset) + "</span>" + "<span>" + startRange.startContainer.nodeValue.substring(endRange.endOffset, endRange.endContainer.nodeValue.length) + "</span>";
        startRange.startContainer.parentNode.replaceChild(newStart, startRange.startContainer);
        console.log("done analyzing line selection.");
    };
    LineSelection.prototype.getContent = function () {
        return this._content;
    };
    return LineSelection;
})(AbstractSelection);
