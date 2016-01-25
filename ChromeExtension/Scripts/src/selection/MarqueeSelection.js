/// <reference path="../ink/brush/MarqueeBrush.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var MarqueeSelection = (function (_super) {
    __extends(MarqueeSelection, _super);
    function MarqueeSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "MarqueeSelection");
        this._startX = 0;
        this._startY = 0;
        this._mouseX = 0;
        this._mouseY = 0;
        this._marqueeX1 = 0;
        this._marqueeY1 = 0;
        this._marqueeX2 = 0;
        this._marqueeY2 = 0;
        this._parentList = new Array();
        this._selected = null;
        this._ct = 0;
        this._content = "";
        this._offsetY = 0;
        this._selectedElement = new Array();
        this._inkCanvas = inkCanvas;
        if (fromActiveStroke) {
            var stroke = inkCanvas._activeStroke.stroke;
            this._offsetY = stroke.documentOffsetY;
            this._startX = stroke.points[0].x;
            this._startY = stroke.points[0].y;
            this._mouseX = stroke.points[stroke.points.length - 1].x;
            this._mouseY = stroke.points[stroke.points.length - 1].y;
            this._ct = 0;
            this._marqueeX1 = this._startX;
            this._marqueeX2 = this._mouseX;
            this._marqueeY1 = this._startY + $(window).scrollTop();
            this._marqueeY2 = this._mouseY + $(window).scrollTop();
            inkCanvas.setBrush(new MarqueeBrush(this._startX, this._startY));
        }
    }
    MarqueeSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new MarqueeBrush(x, y));
        this._parentList = [];
        this._offsetY = window.pageYOffset;
        this._startX = x;
        this._startY = y;
        this._mouseX = x;
        this._mouseY = y;
    };
    MarqueeSelection.prototype.update = function (x, y) {
        this._mouseX = x;
        this._mouseY = y;
        this._marqueeX1 = this._startX;
        this._marqueeY1 = this._startY;
        this._marqueeX2 = this._mouseX;
        this._marqueeY2 = this._mouseY;
        this._inkCanvas.update();
        this._inkCanvas.draw(x, y);
    };
    MarqueeSelection.prototype.end = function (x, y) {
        var el = document.elementFromPoint(this._startX, this._startY);
        this._parentList.push(el);
        this._selected = el;
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
        if (el != null) {
            this.getNextElement(el);
        }
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this._inkCanvas.update();
        this.analyzeContent();
        this._inkCanvas.removeBrushStroke(this._brushStroke);
        this._inkCanvas.update();
        console.log(this.selectedElements);
    };
    MarqueeSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    MarqueeSelection.prototype.getNextElement = function (el) {
        if (this._selected != el) {
            return;
        }
        if (this._ct == 50) {
            throw new Error("an exception! please add to edge case list!");
        }
        this._ct++;
        var rect = el.getBoundingClientRect();
        var nextX = this._mouseX - (rect.left + rect.width);
        var nextY = this._mouseY - (rect.top + rect.height);
        var newList = [];
        if (el.nodeName == "HTML") {
            return;
        }
        if (nextX > 0) {
            if (document.body.contains(this._inkCanvas._canvas)) {
                document.body.removeChild(this._inkCanvas._canvas);
            }
            if (!this.isDescendant(el, document.elementFromPoint(this._mouseX - nextX + 1, this._startY))) {
                var element = document.elementFromPoint(this._mouseX - nextX + 1, this._startY);
                for (var i = 0; i < this._parentList.length; i++) {
                    if (this.isDescendant(element, this._parentList[i])) {
                    }
                    else {
                        newList.push(this._parentList[i]);
                    }
                }
                this._selected = element;
                this.drawPreviousMarquee();
                this._startX = this._mouseX - nextX + 1;
                this._parentList = newList;
                this._parentList.push(element);
                this.getNextElement(element);
            }
        }
        if (nextY > 0) {
            if (document.body.contains(this._inkCanvas._canvas)) {
                document.body.removeChild(this._inkCanvas._canvas);
            }
            element = document.elementFromPoint(this._startX, this._mouseY - nextY + 1);
            var contains = false;
            for (var i = 0; i < this._parentList.length; i++) {
                if (this.isDescendant(this._parentList[i], element) || this._parentList[i] == element) {
                    contains = true;
                }
            }
            if (contains) {
                this.drawPreviousMarquee();
                return;
            }
            for (var i = 0; i < this._parentList.length; i++) {
                if (this.isDescendant(element, this._parentList[i])) {
                }
                else {
                    newList.push(this._parentList[i]);
                }
            }
            this._selected = element;
            this._startY = this._mouseY - nextY + 1;
            this._startX = this._marqueeX1;
            this._parentList = newList;
            this._parentList.push(element);
            this.drawPreviousMarquee();
            this.getNextElement(element);
        }
    };
    MarqueeSelection.prototype.isDescendant = function (parent, child) {
        var node = child.parentNode;
        while (node != null) {
            if (node == parent) {
                return true;
            }
            node = node.parentNode;
        }
        return false;
    };
    MarqueeSelection.prototype.drawPreviousMarquee = function () {
        var canvas = this._inkCanvas._canvas;
        var ctx = this._inkCanvas._context;
        document.body.appendChild(canvas);
        this._inkCanvas.update();
        this._inkCanvas.draw(this._marqueeX2, this._marqueeY2);
    };
    MarqueeSelection.prototype.getBoundingRect = function () {
        return new Rectangle(this._marqueeX1, this._offsetY + this._marqueeY1, this._marqueeX2 - this._marqueeX1, this._marqueeY2 - this._marqueeY1);
    };
    MarqueeSelection.prototype.addToHighLights = function (el, txtindx, wordindx) {
        console.log("ADD TO HIGHLIGHTS====================");
        console.info(el);
        console.log(el.attributes);
        var index = $(el.tagName).index(el);
        console.log(index);
        var obj = { type: "marquee", tagName: el.tagName, index: index };
        if (el.tagName == "WORD" || el.tagName == "HILIGHT") {
            console.log("-------------DIFFICULT--------------");
            console.log(el.attributes);
            var par = el.attributes[0]["ownerElement"].parentElement;
            if (el.tagName == "WORD") {
                var startIndex = Array.prototype.indexOf.call(el.parentElement.childNodes, el);
                par = par.parentElement;
                obj["wordIndx"] = wordindx;
                console.log(par);
            }
            var parIndex = $(par.tagName).index(par);
            obj["parIndex"] = parIndex;
            obj["txtnIndx"] = txtindx;
            obj["par"] = par.tagName;
            obj["val"] = el;
            console.log(el.attributes[0]["ownerElement"].parentElement);
        }
        this.selectedElements.push(obj);
    };
    MarqueeSelection.prototype.analyzeContent = function () {
        if (this._parentList.length != 1) {
            for (var i = 1; i < this._parentList.length; i++) {
                var currAn = this.commonAncestor(this._parentList[0], this._parentList[i]);
                this._parentList[0] = currAn;
            }
        }
        var sel = this._parentList[0].cloneNode(true);
        var selX = $(this._parentList[0]).clone(true);
        this.rmChildNodes(sel, this._parentList[0]);
        var htmlString = sel.innerHTML.replace(/"/g, "'");
        if (sel.outerHTML == "") {
            this._content = sel.innerHTML;
        }
        $(sel).find("img")["andSelf"]().each(function (i, e) {
            console.log(e.src);
            $(e).attr("src", e.src);
            $(e).removeAttr("srcset");
        });
        this._content = sel.outerHTML;
    };
    MarqueeSelection.prototype.commonAncestor = function (node1, node2) {
        var parents1 = this.parents(node1);
        var parents2 = this.parents(node2);
        if (parents1[0] != parents2[0]) {
            throw "No common ancestor!";
        }
        for (var i = 0; i < parents1.length; i++) {
            if (parents1[i] != parents2[i]) {
                return parents1[i - 1];
            }
        }
        return parents1[parents1.length - 1];
    };
    MarqueeSelection.prototype.parents = function (node) {
        var nodes = [node];
        while (node != null) {
            node = node.parentNode;
            nodes.unshift(node);
        }
        return nodes;
    };
    MarqueeSelection.prototype.bound = function (myEl, el) {
        if (el.nodeName == "#text") {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) {
                return;
            }
            for (var i = 0; i < rects.length; i++) {
                var ax1 = rects[i].left;
                var ax2 = rects[i].left + rects[0].width;
                var ay1 = rects[i].top;
                var ay2 = rects[i].top + rects[0].height;
                if (!(ax1 >= this._marqueeX1 &&
                    ax2 <= this._marqueeX2 &&
                    ay1 >= this._marqueeY1 &&
                    ay2 <= this._marqueeY2)) {
                    return false;
                }
            }
            return true;
        }
        else if (el.nodeName != "#comment") {
            var rectX = el.getBoundingClientRect();
            var realDim = this.getRealHeightWidth(el.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            if (rectX == null) {
                return false;
            }
            if (rectX["left"] >= this._marqueeX1 &&
                rectX["left"] + realWidth <= this._marqueeX2 &&
                rectX["top"] >= this._marqueeY1 &&
                rectX["top"] + realHeight <= this._marqueeY2) {
                this.setTextStyle(myEl, el);
                return true;
            }
            return false;
        }
    };
    MarqueeSelection.prototype.rmChildNodes = function (el, trueEl) {
        var removed = [];
        var realNList = [];
        var indexList = [];
        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.intersectWith(el.childNodes[i], trueEl.childNodes[i])) {
                removed.push(el.childNodes[i]);
            }
            else {
                realNList.push(trueEl.childNodes[i]);
                indexList.push(i);
            }
        }
        for (var i = 0; i < removed.length; i++) {
            el.removeChild(removed[i]);
        }
        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.bound(el.childNodes[i], realNList[i])) {
                if (el.childNodes[i].nodeName == "#text") {
                    var index = indexList[i];
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<words>" + $(trueEl.childNodes[indexList[i]]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[indexList[i]].childNodes.length; j++) {
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j], trueEl.childNodes[index].childNodes[j])) {
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                                console.log(trueEl.childNodes[index]);
                                this.addToHighLights(trueEl.childNodes[index].childNodes[j], indexList[i], j);
                            }
                            var foundElement = $(trueEl.childNodes[index]).find("img");
                            console.log("FOUND IIIMMAAAAGGGEE!");
                            if (foundElement.length > 0) {
                                console.log("FOUND IMG");
                                console.log(foundElement);
                                var label = $("<span class='wow'>Selected</span>");
                                label.css({ position: "absolute", display: "block", background: "lightgrey", width: "50px", height: "20px", color: "black", "font-size": "12px" });
                                $("body").append(label);
                                label.css("top", $(foundElement).offset().top + "px");
                                label.css("left", $(foundElement).offset().left + "px");
                            }
                            if (!trueEl.childNodes[index].childNodes[j]["innerHTML"]) {
                                if (trueEl.childNodes[index].childNodes[j].nodeName == "WORD") {
                                    result += " ";
                                }
                            }
                            else {
                                result += trueEl.childNodes[index].childNodes[j]["innerHTML"];
                            }
                        }
                    }
                    el.childNodes[i].data = result;
                }
                else {
                    this.rmChildNodes(el.childNodes[i], realNList[i]);
                }
            }
            else {
                console.log("BOUNDEDDDD=====");
                console.log(trueEl.childNodes[indexList[i]]);
                var startIndex = Array.prototype.indexOf.call(trueEl.childNodes, trueEl.childNodes[i]);
                var foundElement = $(trueEl.childNodes[indexList[i]]).find("img");
                console.log("FOUND IIIMMAAAAGGGEE!");
                if (foundElement.length > 0) {
                    console.log("FOUND IMG");
                    console.log(foundElement);
                    var label = $("<span class='wow'>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", ($(foundElement).offset().top - 5) + "px");
                    label.css("left", ($(foundElement).offset().left - 5) + "px");
                }
                if (trueEl.childNodes[indexList[i]].childNodes.length == 0) {
                    console.log("-----------TEXT?-------");
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<hilight>" + $(realNList[i]).text() + "</hilight>");
                }
                trueEl.childNodes[indexList[i]].style.backgroundColor = "yellow";
                console.log(startIndex);
                this.addToHighLights(trueEl.childNodes[indexList[i]], indexList[i], 0);
            }
        }
    };
    MarqueeSelection.prototype.setTextStyle = function (el, trueEl) {
        var elStyle = document.defaultView.getComputedStyle(trueEl["parentElement"]);
        el = el.parentNode;
        el.style.font = elStyle.font;
    };
    MarqueeSelection.prototype.getRealHeightWidth = function (rectsList) {
        var maxH = Number.NEGATIVE_INFINITY;
        var minH = Number.POSITIVE_INFINITY;
        var maxW = Number.NEGATIVE_INFINITY;
        var minW = Number.POSITIVE_INFINITY;
        $(rectsList).each(function (indx, elem) {
            if (elem["top"] + elem["height"] > maxH) {
                maxH = elem["top"] + elem["height"];
            }
            if (elem["top"] < minH) {
                minH = elem["top"];
            }
            if (elem["left"] < minW) {
                minW = elem["left"];
            }
            if (elem["left"] + elem["width"] > maxW) {
                maxW = elem["left"] + elem["width"];
            }
        });
        return [(maxH - minH), (maxW - minW), minW, minH];
    };
    MarqueeSelection.prototype.intersectWith = function (myEl, el) {
        if (!el) {
            return false;
        }
        ;
        var bx1 = this._marqueeX1;
        var bx2 = this._marqueeX2;
        var by1 = this._marqueeY1;
        var by2 = this._marqueeY2;
        if (el.nodeName == "#text") {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) {
                return;
            }
            for (var i = 0; i < rects.length; i++) {
                var ax1 = rects[i].left;
                var ax2 = rects[i].left + rects[i].width;
                var ay1 = rects[i].top;
                var ay2 = rects[i].top + rects[i].height;
                if (ax1 < bx2 && ax2 > bx1 && ay1 < by2 && ay2 > by1) {
                    return true;
                }
            }
            return false;
        }
        else if (el.nodeName != "#comment") {
            var rangeY = document.createRange();
            rangeY.selectNodeContents(el);
            var realDim = this.getRealHeightWidth(rangeY.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            var minX = realDim[2];
            var minY = realDim[3];
            ax1 = el.getBoundingClientRect()["left"];
            ax2 = el.getBoundingClientRect()["left"] + realWidth;
            ay1 = el.getBoundingClientRect()["top"];
            ay2 = el.getBoundingClientRect()["top"] + realHeight;
        }
        if (ax1 < bx2 && bx1 < ax2 && ay1 < by2) {
            return by1 < ay2;
        }
        else {
            return false;
        }
    };
    MarqueeSelection.prototype.getContent = function () {
        return this._content;
    };
    return MarqueeSelection;
})(AbstractSelection);
