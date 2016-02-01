class MarqueeSelection extends AbstractSelection {

    _startX: number = 0;
    _startY: number = 0;
    _endX: number = 0;
    _endY: number = 0;
    _nextX: number = 0;
    _nextY: number = 0;
    _content: string = "";
    _parentList: Array<any> = new Array<any>();
    constructor() {
        super();
        console.log("MARQUEE SELECTION");
    }

    start(x: number, y: number): void {
        this._startX = x;
        this._startY = y;
        console.log("marquee start" + x +":"+ y);
    }

    end(x: number, y: number): void {
        this._endX = x;
        this._endY = y;

        if (this._startX > this._endX) {
            var temp = this._startX;
            this._startX = this._endX;
            this._endX = temp;
        }
        if (this._startY > this._endY) {
            var temp = this._startY;
            this._startY = this._endY;
            this._endY = temp;
        }

        this.analyzeContent();
        console.log(this._startX + "START" + this._startY);
        console.log("marquee end" + x +":"+ y);
    }
    getContent(): string {
        return this._content;
    }

    analyzeContent(): void {
        this.findParentList();
        console.log("marquee start analyzing content....");
        this.findCommonParent();
        var parent = this._parentList[0].cloneNode(true);
        this.rmChildNodes(parent, this._parentList[0]);   
        console.log(this._parentList[0]);
        this._content = parent.innerHTML;
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

        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.bound(el.childNodes[i], realNList[i])) {
                if (el.childNodes[i].nodeName == "#text") {
                    var index = indexList[i];
                    $(trueEl.childNodes[index]).replaceWith("<words>" + $(trueEl.childNodes[index]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[index].childNodes.length; j++) {
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j])) {
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                                console.log(trueEl.childNodes[index]);
                                this.addToHighLights(trueEl.childNodes[index].childNodes[j], indexList[i], j);
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
                this.addToHighLights(trueEl.childNodes[indexList[i]], indexList[i], -1);
                 
                //if (trueEl.childNodes[index].childNodes[j]) {
                //    trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                //}

                // realNList[i].style.backgroundColor = "yellow";
            }


        }
    
    }

    addToHighLights(el: Element, txtindx: Number, wordindx): void {
        console.log("ADD TO HIGHLIGHTS====================");
        console.info(el.tagName);
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
            obj["par"] = par.tagName;
            obj["parIndex"] = parIndex;
            obj["txtnIndx"] = txtindx;
            obj["val"] = el;

            console.log(el.attributes[0]["ownerElement"].parentElement);
            console.log(obj);
        } 
        this.selectedElements.push(obj);
        console.log(this.selectedElements);
        
    }
    intersectWith(el): boolean {

        //checks if element is intersecting with selection range 
        if (!el) {
            return false
        };
        var bx1 = this._startX;
        var bx2 = this._endX;
        var by1 = this._startY;
        var by2 = this._endY;
        if (el.nodeName == "#text") {
            // this.setTextStyle(myEl, el);                        
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) { return; }
            for (var i = 0; i < rects.length; i++) {
                var ax1 = rects[i].left;
                var ax2 = rects[i].left + rects[i].width;
                var ay1 = rects[i].top;
                var ay2 = rects[i].top + rects[i].height;

                if (ax1 < bx2 && ax2 > bx1 && ay1 < by2 && ay2 > by1) {
                    return true;
                }
            }
            return false

        }
        else if (el.nodeName != "#comment") {
            var rangeY = document.createRange();
            rangeY.selectNodeContents(el);
            var realDim = this.getRealHeightWidth(rangeY.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            var minX = realDim[2];
            var minY = realDim[3];
            /////works weird for Wikipedia. 
            ax1 = el.getBoundingClientRect()["left"];
            ax2 = el.getBoundingClientRect()["left"] + realWidth;
            ay1 = el.getBoundingClientRect()["top"];
            ay2 = el.getBoundingClientRect()["top"] + realHeight;
        }

        if (ax1 < bx2 && bx1 < ax2 && ay1 < by2) {
            return by1 < ay2;
        }
        else {
            return false
        }
    }

    bound(myEl, el): boolean {
        if (el.nodeName == "#text") {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) { return; }

            for (var i = 0; i < rects.length; i++) {
                var ax1 = rects[i].left;
                var ax2 = rects[i].left + rects[0].width;
                var ay1 = rects[i].top;
                var ay2 = rects[i].top + rects[0].height;

                if (!(ax1 >= this._startX &&
                    ax2 <= this._endX &&
                    ay1 >= this._startY &&
                    ay2 <= this._endY)) { return false }
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
            if (rectX["left"] >= this._startX &&
                rectX["left"] + realWidth <= this._endX &&
                rectX["top"] >= this._startY &&
                rectX["top"] + realHeight <= this._endY) {
          //      this.setTextStyle(myEl, el);

                return true;
            }
            return false;
        }
    }

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

    findParentList(): void {
        this._parentList = [];
        var el = document.elementFromPoint(this._startX, this._startY);
        this._nextX = this._endX;
        this._nextY = this._startY;
  //      this._parentList.push(el);
        if (el != null)
            this.findNextElement(el);
        console.info(this._parentList);
        
    }

    findNextElement(el: Element): void {
        var rect = el.getBoundingClientRect();
        var nextX = this._endX - (rect.left + rect.width);
        var nextY = this._endY - (rect.top + rect.height);
        if (this._parentList.indexOf(el) > -1)
            return;
        this._parentList.push(el);
        console.info(el);
        if (el.nodeName == "HTML")
            return;
        if (nextX > 0) {
            console.log("more on the X AXIS");
            console.info(document.elementFromPoint(this._endX - nextX + 1, this._nextY));
            this.findNextElement(document.elementFromPoint(this._endX - nextX + 1, this._nextY));
        }
        if (nextY > 0) {
            console.log("more on the Y AXIS");
            console.info(document.elementFromPoint(rect.left, this._endY - nextY + 1));
            this.findNextElement(document.elementFromPoint(rect.left, this._endY - nextY + 1));
        }
    }


}