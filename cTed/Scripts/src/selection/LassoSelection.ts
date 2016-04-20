class LassoSelection extends AbstractSelection {


    _parentList: Array<any> = new Array<any>();
    _minX = 10000;
    _minY = 10000;
    _maxX = 0;
    _maxY = 0;
    _content: string = "";
    _sampleLines: Array<Line>;

    start(x: number, y: number): void {
        this.yscroll = 0;
    }

    end(x: number, y: number): void {
        var points = this.stroke.sampleStroke().points;
        for (var i = 0; i < points.length; i++) {
            points[i] = new Point(points[i].x, points[i].y - $(document).scrollTop() + this.yscroll);
        }
        this.samplePoints = points;
        this._sampleLines = this.sampleLines(this.samplePoints);
        this.analyzeContent();
    }

    analyzeContent(): void { 

        this.makeInitialParentList();

        this.findCommonParent();

        var parent = this._parentList[0].cloneNode(true);
        if (parent.nodeName == "html") {
            console.log("=========================PARENT WRONG ================");
            return;
        }
        this.rmChildNodes(parent, this._parentList[0]);
        this._content = parent.innerHTML;
    }

    makeInitialParentList(): void {
        var element;
        this.stroke.points.forEach(p => {
            if (element != document.elementFromPoint(p.x, p.y)) {
                element = document.elementFromPoint(p.x, p.y);
                this._parentList.push(element);
            }
        });
    }

    sampleLines(points: Array<Point>): Array<Line> {
        var sampleStroke = points;
        var lines = [];
        for (var i = 1; i < sampleStroke.length; i++) {
            lines.push(new Line(sampleStroke[i - 1], sampleStroke[i]));
        }
        lines.push(new Line(sampleStroke[sampleStroke.length - 1], sampleStroke[0]));
        return lines;
    }
    


    isLineIntersect(l1, l2: Line): boolean {
        if (l1.p1.x == l1.p2.x) {
            //vertical line 
            if (l1.p1.x <= Math.max(l2.p1.x, l2.p2.x) && l1.p1.x >= Math.min(l2.p1.x, l2.p2.x)) {
                var y = (l2.C - l2.A * l1.p1.x) / l2.B;
                return y <= Math.max(l1.p1.y, l1.p2.y) && y >= Math.min(l1.p1.y, l1.p2.y);
            }
        } else {
            if (l1.p1.y <= Math.max(l2.p1.y, l2.p2.y) && l1.p1.y >= Math.min(l2.p1.y, l2.p2.y)){
                var x = (l2.C - l2.B * l1.p1.y) / l2.A;
                return x <= Math.max(l1.p1.x, l1.p2.x) && x >= Math.min(l1.p1.x, l1.p2.x);
            }
        }
        return false;

    }


    intersectWith(el: Element): number {
        if (!el)
            return 0;
        if (this.isTextElement(el)) {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) { return 0; }
            for (var i = 0; i < rects.length; i++) {
                var rect = rects[i];
                if (this.isRectIntersect(new Rectangle(rect.left, rect.top, rect.width, rect.height))) {
                    return 1;
                }
            }
            if (this.isPointBound(new Point(rects[0].left, rects[0].top))) {
                return 2;
            }
            return 0;
        } else if (!this.isCommentElement(el)) {
            var rangeY = document.createRange();
            rangeY.selectNodeContents(el);
            var realDim = this.getRealHeightWidth(rangeY.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            var minX = realDim[2];
            var minY = realDim[3];
            /////works weird for Wikipedia. 
            if (minX > 100000 || minY > 100000) {
                console.log("WTF!!!!>>????");
                console.log(el);
                return 0;
            }
            if (this.isRectIntersect(new Rectangle(minX, minY, realWidth, realHeight))) {
                return 1;
            }
            if (this.isPointBound(new Point(minX, minY))) {
                return 2;
            }
            return 0;
        }
    }

    isPointBound(p: Point): boolean {
   //     console.log("======isPointBound ");
    //    console.log(p);
        var xPoints = [];
        for (var i = 0; i < this._sampleLines.length; i++){
            var l = this._sampleLines[i];
            if (p.y <= Math.max(l.p1.y, l.p2.y) && p.y >= Math.min(l.p1.y, l.p2.y)) {
                var x = (l.C - l.B * p.y) / l.A;
                xPoints.push(x);
            }
        }
    //    console.log(xPoints);
        if (xPoints.length == 0)
            return false;
        xPoints.sort(function (a, b) { return a - b; });
        var res = false;
      //  console.log(xPoints);
        for (var i = 0; i < xPoints.length; i++) {
            if (p.x < xPoints[i])
                return res;
            res = !res;
        }
        return false;
    }

    isCommentElement(el: Element): boolean {
        return el.nodeName == "#comment";
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
        var lines = rect.getLines();
        for (var i = 0; i < lines.length; i++) {
            for (var j = 0; j < this._sampleLines.length; j++) {
                if (this.isLineIntersect(lines[i], this._sampleLines[j])) {
                    return true;
                }
            }
        }
        return rect.hasPoint(this.stroke.points[0]);
    }


    isTextElement(el: Element): boolean {
        return (el.nodeName == "#text");
    }

    rmChildNodes(el, trueEl): void {
  //      console.log("removeChildNodes for... ");
  //      console.log(trueEl);
        var removed = [];
        var realNList = [];
        var resList = [];
        var indexList = [];
        //iterate through childNodes and add to list(removed).
        for (var i = 0; i < el.childNodes.length; i++) {
            var res = this.intersectWith(trueEl.childNodes[i]);
    //        console.log(trueEl.childNodes[i]);
    //        console.log(res);
            if (res == 0) {
                removed.push(el.childNodes[i]);
            }
            else {
                realNList.push(trueEl.childNodes[i]);
                resList.push(res);
                indexList.push(i);
            }
        }
        //remove not intersecting elements; 
        for (var i = 0; i < removed.length; i++) {
            el.removeChild(removed[i]);
        }
        for (var i = 0; i < el.childNodes.length; i++) {

            if (resList[i] != 2) {
                if (this.isTextElement(el.childNodes[i])) {
                    var index = indexList[i];
                    $(trueEl.childNodes[index]).replaceWith("<words>" + $(trueEl.childNodes[index]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[index].childNodes.length; j++) {
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j]) > 0) {
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                                this.addToHighLights(trueEl.childNodes[index].childNodes[j], indexList[i], j);
                                trueEl.childNodes[index].childNodes[j].className += " " + this.id.toString();

                            }
                            if (!trueEl.childNodes[index].childNodes[j]["innerHTML"]) {
                                if (trueEl.childNodes[index].childNodes[j].nodeName == "WORD") {
                                    trueEl.childNodes[index].childNodes[j]["innerHTML"] = " ";
                                }
                            }
                            else { result += trueEl.childNodes[index].childNodes[j]["innerHTML"]; }
                        }
                    }
                    el.childNodes[i].data = result;
                } else {
                    this.rmChildNodes(el.childNodes[i], realNList[i]);
                }
            } else {
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
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<hilight>" + $(realNList[i]).text() + "</hilight>");
                }
                $(realNList[i]).css("background-color", "yellow");
                trueEl.childNodes[indexList[i]].style.backgroundColor = "yellow";
                trueEl.childNodes[indexList[i]].className += " " + this.id.toString();
                this.addToHighLights(trueEl.childNodes[indexList[i]], indexList[i], -1);
            }
        }

    }
    addToHighLights(el: Element, txtindx: Number, wordindx): void {

        var index = $(el.tagName).index(el);
        var obj = { type: "lasso", tagName: el.tagName, index: index };
        if (el.tagName == "WORD" || el.tagName == "HILIGHT") {

            var par = el.attributes[0]["ownerElement"].parentElement;
            if (el.tagName == "WORD") {

                var startIndex = Array.prototype.indexOf.call(el.parentElement.childNodes, el);
                par = par.parentElement;
                obj["wordIndx"] = wordindx;
            }
            var parIndex = $(par.tagName).index(par);
            obj["par"] = par.tagName;
            obj["parIndex"] = parIndex;
            obj["txtnIndx"] = txtindx;
            obj["val"] = el;
        }
        this.selectedElements.push(obj);
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

    changeMinMax(point) {
        var x = point.x;
        var y = point.y;
        if (x < this._minX)
            this._minX = x;
        if (x > this._maxX)
            this._maxX = x;
        if (y > this._maxY)
            this._maxY = y;
        if (y < this._minY)
            this._minY = y;
    }

}