/// <reference path="../../lib/collections.ts"/>
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var BracketSelection = (function (_super) {
    __extends(BracketSelection, _super);
    function BracketSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "BracketSelection");
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        if (fromActiveStroke) {
            this._inkCanvas.setBrush(new HighlightBrush());
            var t = this;
            $.each(inkCanvas._activeStroke.stroke.points, function () {
                t._inkCanvas.draw(this.x, this.y);
            });
        }
    }
    BracketSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new HighlightBrush());
    };
    BracketSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    BracketSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();
        this.select();
        this._inkCanvas.removeBrushStroke(this._brushStroke);
        this._inkCanvas.update();
    };
    BracketSelection.prototype.getBoundingRect = function () {
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
    BracketSelection.prototype.analyzeContent = function () {
        var _this = this;
        var stroke = this._brushStroke.stroke;
        var selectionBB = stroke.getBoundingRect();
        selectionBB.w = Main.DOC_WIDTH - selectionBB.x;
        var samplingRate = 50;
        var numSamples = 0;
        var totalScore = 0;
        var hitCounter = new collections.Dictionary(function (elem) { return elem.outerHTML.toString(); });
        var elList = [];
        var scoreList = [];
        for (var x = selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate) {
            for (var y = selectionBB.y; y < selectionBB.y + selectionBB.h; y += samplingRate) {
                var hitElem = document.elementFromPoint(x, y);
                if ($(hitElem).height() > selectionBB.h + selectionBB.h / 2.0) {
                    continue;
                }
                else {
                }
                numSamples++;
                var score = 1.0 - Math.sqrt((x - selectionBB.x) / selectionBB.w);
                if (elList.indexOf(hitElem) < 0) {
                    elList.push(hitElem);
                    scoreList.push(score);
                }
                else {
                    scoreList[elList.indexOf(hitElem)] += score;
                }
                if (!hitCounter.containsKey(hitElem)) {
                    hitCounter.setValue(hitElem, score);
                }
                else {
                    hitCounter.setValue(hitElem, hitCounter.getValue(hitElem) + score);
                }
                totalScore += score;
            }
        }
        var maxScore = -10000;
        var bestMatch = null;
        console.log(hitCounter);
        hitCounter.forEach(function (k, v) {
            if (v > maxScore) {
                maxScore = v;
                bestMatch = k;
            }
        });
        var candidates = [];
        var precision = 4;
        hitCounter.forEach(function (k, v) {
            candidates.push(v);
        });
        console.log("initial candidates");
        console.log(candidates);
        var std = Statistics.getStandardDeviation(candidates, precision);
        var maxDev = maxScore - 2 * std;
        var finalCandiates = [];
        hitCounter.forEach(function (k, v) {
            if (v >= maxDev && v <= maxScore) {
                finalCandiates.push(k);
            }
        });
        console.log("initial candidates");
        console.log(finalCandiates);
        finalCandiates.concat().forEach(function (c) {
            var maxDelta = 120;
            var largerParents = [];
            var parents = $(c).parents();
            for (var i = 0; i < parents.length; i++) {
                var parent = $(parents[i]);
                if (parent.width() - $(c).width() < maxDelta && parent.height() - $(c).height() < maxDelta) {
                    var index = finalCandiates.indexOf(c);
                    if (index > 0) {
                        finalCandiates.splice(index, 1);
                        largerParents.push(parent[0]);
                    }
                }
            }
            if (largerParents.length > 0)
                finalCandiates.push(largerParents.pop());
        });
        console.log("initial candidates with parents");
        console.log(finalCandiates);
        var selectedElements = finalCandiates.filter(function (candidate) {
            if ($(candidate).offset().left - selectionBB.x < 100) {
                return true;
            }
            return false;
        });
        console.log("selected elements");
        console.log(selectedElements);
        this._clientRects = new Array();
        var result = "";
        selectedElements.forEach(function (el) {
            $(el).find("img")["andSelf"]().each(function (i, e) {
                $(e).attr("src", e.src);
                $(e).removeAttr("srcset");
            });
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            _this._clientRects = _this._clientRects.concat.apply([], rects);
            var index = $(el.tagName).index(el);
            _this.selectedElements.push({ type: "bracket", tagName: el.tagName, index: index });
            result += el.outerHTML;
        });
        this._content = result;
    };
    BracketSelection.prototype.getContent = function () {
        return this._content;
    };
    return BracketSelection;
})(AbstractSelection);
