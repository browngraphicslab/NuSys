class LassoSelection extends AbstractSelection {


    _parentList: Array<any> = new Array<any>();
    _strokeHash = {};
    _minX = 10000;
    _minY = 10000;
    _maxX = 0;
    _maxY = 0;
    _content: string = "";


    start(x: number, y: number): void {
    }

    end(x: number, y: number): void {
        this.analyzeContent();
    }
    changeMinMax(point) {
        var x = point.x;
        var y = point.y;
        if (x < this._minX)
            this._minX = x;
        if (x > this._maxX)
            this._maxX = x;
        if (y > this._maxY)
            this._maxY = y;
        if (y < this._maxY)
            this._minY = y;
    }
    analyzeContent(): void {
        //create a hashtable of y/3 --> x values
        //create a list of elements overlaying strokes
        var len = this.stroke.points.length;
        var element;
        for (var i = 0; i < len; i++) {
            var point = this.stroke.points[i];
            this.changeMinMax(point);
            if (element != document.elementFromPoint(point.x, point.y)) {
                element = document.elementFromPoint(point.x, point.y);
                this._parentList.push(element);
            }
            var key = Math.floor(this.stroke.points[i].y / 5);
            if (this._strokeHash.hasOwnProperty(key.toString())) {
                this._strokeHash[key].push(this.stroke.points[i].x);
            } else {
                this._strokeHash[key] = [this.stroke.points[i].x];
            }
        }

        this.findCommonParent();
        console.info(this._parentList[0]);
        console.log(this._strokeHash);
        console.log("=====rmChildforLasso");
        var parent = this._parentList[0].cloneNode(true);
   //     this.rmChildNodes(parent, this._parentList[0]);   
        this._content = parent.innerHTML;
    }

    isBound(el): boolean {
        if (el.nodeName == "#text") {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) { return; }

            for (var i = 0; i < rects.length; i++) {
                var x1 = rects[i].left;
                var ax2 = rects[i].left + rects[0].width;
                var y1 = rects[i].top;
                var ay2 = rects[i].top + rects[0].height;

                if (!this.isRectBound(new Rectangle(x1, y1, rects[0].width, rects[0].height))) {
                    return false
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
            return this.isRectBound(new Rectangle(rectX["left"], rectX["top"], realWidth, realHeight));
        }
    }

    intersectWith(el): boolean {
        if (!el)
            return false;

        if (el.nodeName == "#text") {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) { return; }
            for (var i = 0; i < rects.length; i++) {
                var rect = rects[i];
                if (this.isRectIntersect(new Rectangle(rect.left, rect.top, rect.width, rect.height))) {
                    return true;
                }
            }
            return false
        } else if (el.NodeName != "#comment") {
            var rangeY = document.createRange();
            rangeY.selectNodeContents(el);
            var realDim = this.getRealHeightWidth(rangeY.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            var minX = realDim[2];
            var minY = realDim[3];
            /////works weird for Wikipedia. 
            console.log(el);
            console.log(minX + "  " + minY + "  " + realHeight + "  " + realWidth);
            console.log(this._minX + " " + this._minY + " " + this._maxX + " " + this._maxY);
            if (minX < this._minX && minX + realWidth > this._maxX && minY < this._minY && minY + realHeight > this._maxY)
                return true;
            var xValues = this._strokeHash[Math.floor(minY / 5)];
            if (xValues) {
                for (var i = 0; i < xValues.length; i++) {
                    if (xValues[i] > minX && xValues[i] < minX + realWidth) {
                        return true;
                    }
                }
            }
            xValues = this._strokeHash[Math.floor( (minY + realHeight) / 5)];
            if (xValues) {
                for (var i = 0; i < xValues.length; i++) {
                    if (xValues[i] > minX && xValues[i] < minX + realWidth) {
                        return true;
                    }
                }
            }
            return this.isRectIntersect(new Rectangle(minX, minY, realWidth, realHeight));
        }
        console.log("in intersect:"+el);
        return false;
    }
    //move to util
    getRealHeightWidth(rectsList): Array<number> {
        //finds the real Heights and Widths of DOM elements by iterating through their clientRectList.
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
    }
    isRectIntersect(rect: Rectangle): boolean {

        return (this.isPointBound(rect.x, rect.y) || this.isPointBound(rect.x + rect.w, rect.y)
            || this.isPointBound(rect.x, rect.y + rect.h) || this.isPointBound(rect.x + rect.w, rect.y+rect.h));
    }
    isRectBound(rect: Rectangle): boolean {
        return (this.isPointBound(rect.x, rect.y) && this.isPointBound(rect.x + rect.w, rect.y)
            && this.isPointBound(rect.x, rect.y + rect.h) && this.isPointBound(rect.x + rect.w, rect.y + rect.h));
    }

    isPointBound(x, y: number): boolean {
        if (x > this._maxX || x < this._minX || y > this._maxY || y < this._minY)
            return false;
        var res = false;
        var yKey = Math.floor(y / 5);
        var xValues = this._strokeHash[yKey];
        var delta = 5;
        while (!xValues) {
            xValues = this._strokeHash[yKey + delta];
            delta *= -1;
            if (delta > 0)
                delta += 5;
        }
        xValues.sort();
        
        console.log(xValues);
        for (var i = 0; i < xValues.length; i++) {
            res = !res;
            if (x < xValues[i])
                return res;
        }

        console.log("=============isPointBound======================================");
        return true;
    }

    rmChildNodes(el, trueEl): void {
        var removed = [];
        var realNList = [];
        var indexList = [];
        //iterate through childNodes and add to list(removed).
        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.intersectWith(trueEl.childNodes[i])) {
                removed.push(el.childNodes[i]);
            }
            else {
                realNList.push(trueEl.childNodes[i]);
                indexList.push(i);
            }
        }

        //remove not intersecting elements; 
        for (var i = 0; i < removed.length; i++) {
            el.removeChild(removed[i]);
        }
        console.log("remove......");
        console.log(this._strokeHash);
        console.log(el.childNodes);
        
        return;
        for (var i = 0; i < el.childNodes.length; i++) {
            console.log(this.isBound(realNList[i]));
            if (!this.isBound(realNList[i])) {
                if (el.childNodes[i].nodeName == "#text") {
                    var index = indexList[i];
                    $(trueEl.childNodes[index]).replaceWith("<words>" + $(trueEl.childNodes[index]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[index].childNodes.length; j++) {
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j])) {
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                                console.log(trueEl.childNodes[index]);
                            //    this.addToHighLights(trueEl.childNodes[index].childNodes[j], indexList[i], j);
                            }
                            if (!trueEl.childNodes[index].childNodes[j]["innerHTML"]) {
                                if (trueEl.childNodes[index].childNodes[j].nodeName == "WORD") {
                                    result += " ";
                                }
                            }
                            else { result += trueEl.childNodes[index].childNodes[j]["innerHTML"]; }
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
                if (foundElement.length > 0) {
                    var label = $("<span class='wow'>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", ($(foundElement).offset().top - 5) + "px");
                    label.css("left", ($(foundElement).offset().left - 5) + "px");
                }

                if (trueEl.childNodes[indexList[i]].childNodes.length == 0) {
                    console.log("-----------TEXT?-------");
                    console.log($(trueEl.childNodes[indexList[i]]));
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<hilight>" + $(realNList[i]).text() + "</hilight>");
                }
                $(realNList[i]).css("background-color", "yellow");
                trueEl.childNodes[indexList[i]].style.backgroundColor = "yellow";
                console.log(startIndex);
            //    this.addToHighLights(trueEl.childNodes[indexList[i]], indexList[i], -1);
                 
                //if (trueEl.childNodes[index].childNodes[j]) {
                //    trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                //}

                // realNList[i].style.backgroundColor = "yellow";
            }


        }

    }

    findCommonParent(): void {
        if (this._parentList.length != 1) {                     //finds the common parent of all elements in _parentList
            for (var i = 1; i < this._parentList.length; i++) {
                var currAn = this.commonParent(this._parentList[0], this._parentList[i]);
                this._parentList[0] = currAn;
            }
        }
    }

    commonParent(node1: Element, node2: Element) {
        //finds common ancestor between two nodes. 
        var parents1 = this.parents(node1)
        var parents2 = this.parents(node2)

        if (parents1[0] != parents2[0]) {
            throw "No common ancestor!"
        }
        for (var i = 0; i < parents1.length; i++) {
            if (parents1[i] != parents2[i]) {
                return parents1[i - 1];
            }
        }
        return parents1[parents1.length - 1];
    }

    parents(node: Node): Node[] {
        var nodes = [node]
        while (node != null) {
            node = node.parentNode;
            nodes.unshift(node);
        }
        return nodes;
    }


    getContent(): string {

        return this._content;
    }

}