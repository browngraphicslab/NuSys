/**
 * Created by phili_000 on 7/4/2015.
 */

var MarqueeSelection = (function () {

    function MarqueeSelection(inkCanvas, fromActiveStroke) {
        this._inkCanvas = inkCanvas;
        this._brushStroke = null;
        this._startX = 0;
        this._startY = 0;
        this._mouseX = 0;
        this._mouseY = 0;
        this._marqueeX1 = 0;
        this._marqueeX2 = 0;
        this._marqueeY1 = 0;
        this._marqueeY2 = 0;
        this._parentList = [];

        if (fromActiveStroke) {

            var stroke = inkCanvas._activeStroke.stroke;
            this._startX = stroke.points[0].x;
            this._startY = stroke.points[0].y;
            this._mouseX = stroke.points[stroke.points.length-1].x;
            this._mouseY = stroke.points[stroke.points.length-1].y;
            this._ct = 0;
            this._marqueeX1 = this._startX;
            this._marqueeX2 = this._mouseX;
            this._marqueeY1 = this._startY;
            this._marqueeY2 = this._mouseY;
            inkCanvas.setBrush(new MarqueeBrush(this._startX, this._startY));
        }
    }

    MarqueeSelection.prototype.getBoundingRect = function() {
        return new Rectangle(this._marqueeX1, this._marqueeY1, this._marqueeX2 - this._marqueeX1, this._marqueeY2 - this._marqueeY1);
    };

    MarqueeSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new MarqueeBrush(x, y));
        this._parentList = [];
        this._startX = x;
        this._startY = y;
    };

    MarqueeSelection.prototype.update = function (x, y) {
        this._mouseX = parseInt(x);
        this._mouseY = parseInt(y);

        this._marqueeX1 = this._startX;
        this._marqueeY1 = this._startY;
        this._marqueeX2 = this._mouseX;
        this._marqueeY2 = this._mouseY;

        var canvas = this._inkCanvas._canvas;
        var ctx = this._inkCanvas._context;

        this._inkCanvas.update();
        this._inkCanvas.draw(x,y);

        this.clearSelection();
    };

    MarqueeSelection.prototype.end = function (x, y) {

        var el = document.elementFromPoint(this._startX, this._startY);

        this._parentList.push(el);
        this._selected = el;

        this.drawPreviousMarquee();
        if (this._marqueeX1 > this._marqueeX2){
            var temp = this._marqueeX1;
            this._marqueeX1 = this._marqueeX2;
            this._marqueeX2 = temp;
        }
        if (this._marqueeY1 > this._marqueeY2){
            var temp = this._marqueeY1;
            this._marqueeY1 = this._marqueeY2;
            this._marqueeY2 = temp;
        }
        
        this.getNextElement(el);
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;

        this._brushStroke.brush = new SelectionBrush();
        this._inkCanvas.update();
    };

    MarqueeSelection.prototype.deselect = function() {
        if (!this._inkCanvas.removeBrushStroke(this._brushStroke))
            console.log(this);
    };

    MarqueeSelection.prototype.drawPreviousMarquee = function() {
        var canvas = this._inkCanvas._canvas;
        var ctx = this._inkCanvas._context;
        document.body.appendChild(canvas);
        this._inkCanvas.update();
        this._inkCanvas.draw(this._marqueeX2, this._marqueeY2);
    };

    MarqueeSelection.prototype.clearSelection = function() {
        if(document.selection && document.selection.empty) {
            document.selection.empty();
        } else if(window.getSelection) {
            var sel = window.getSelection();
            sel.removeAllRanges();
        }
    };

    MarqueeSelection.prototype.isDescendant = function(parent, child) {
        var node = child.parentNode;
        while (node != null) {
            if (node == parent) {
                return true;
            }
            node = node.parentNode;
        }
        return false;
    };

    MarqueeSelection.prototype.getNextElement = function(el){

        if (this._selected!=el){return;}
        if (this._ct==50){
            throw new Error("an exception! please add to edge case list!")
        }
        this._ct++;
        var rect = el.getBoundingClientRect();
        var nextX = this._mouseX- (rect.left + rect.width);
        var nextY = this._mouseY- (rect.top + rect.height);
        var newList = [];

        if (nextX>0){

            document.body.removeChild(this._inkCanvas._canvas);
            if (!this.isDescendant(el, document.elementFromPoint(this._mouseX-nextX+1, this._startY))){

                element = document.elementFromPoint(this._mouseX-nextX+1, this._startY);
                for (var i=0; i<this._parentList.length; i++){

                    if (this.isDescendant(element,this._parentList[i])){
                    }
                    else{
                        newList.push(this._parentList[i]);
                    }
                }
                this._selected = element;
                this.drawPreviousMarquee();
                this._startX = this._mouseX - nextX +1;
                this._parentList = newList;
                this._parentList.push(element);
                this.getNextElement(element);
            }
        }
        if(nextY>0){
            if (body.contains(this._inkCanvas._canvas)){
                document.body.removeChild(this._inkCanvas._canvas);
            }
            element = document.elementFromPoint(this._startX, this._mouseY-nextY + 1);
            var contains = false;
            for (var i=0; i<this._parentList.length; i++){
                if (this.isDescendant(this._parentList[i],element) || this._parentList[i]==element){
                    contains = true;
                }
            }
            if (contains){
                this.drawPreviousMarquee();
                return;
            }

            for (var i=0; i<this._parentList.length; i++){
                if (this.isDescendant(element, this._parentList[i])){
                }
                else{
                    newList.push(this._parentList[i]);
                }
            }

            this._selected = element;
            this._startY = this._mouseY-nextY+1;
            this._startX = this._marqueeX1;
            this._parentList = newList;
            this._parentList.push(element);
            this.drawPreviousMarquee();
            this.getNextElement(element);
        }
    };


    MarqueeSelection.prototype.rmChildNodes = function(el, trueEl) {

        var removed = [];
        var realNList = [];
        var indexList = [];

        //iterate through childNodes and add to list(removed).
        for (var i=0; i<el.childNodes.length; i++){
            if (!this.intersectWith(el.childNodes[i],trueEl.childNodes[i])){
                removed.push(el.childNodes[i]);
            }
            else{
                realNList.push(trueEl.childNodes[i]);
                indexList.push(i);
            }
        }

        //remove not intersecting elements; 
        for (var i=0; i<removed.length; i++){
            el.removeChild(removed[i]);
        }

        for (var i=0; i<el.childNodes.length; i++){
            if (!this.bound(el.childNodes[i],realNList[i])){
                if (el.childNodes[i].nodeName == "#text"){
                    var index = indexList[i];

                $(trueEl.childNodes[indexList[i]]).replaceWith("<span>"+$(trueEl.childNodes[indexList[i]]).text().replace(/([^\s]*)/g, "<word>$1</word>")+"</span>");


                 var result = "";
                 for (var j=0; j<trueEl.childNodes[indexList[i]].childNodes.length; j++){
                    if (this.intersectWith(trueEl.childNodes[index].childNodes[j],trueEl.childNodes[index].childNodes[j])){
                       if (!trueEl.childNodes[index].childNodes[j].innerHTML){
                            if (trueEl.childNodes[index].childNodes[j].nodeName == "WORD"){
                                result += " ";
                            }
                       }
                       else{ result += trueEl.childNodes[index].childNodes[j].innerHTML;}
                    }   
                 }
                el.childNodes[i].data = result;
                }
                else{
                this.rmChildNodes(el.childNodes[i], realNList[i]);
                }
            }
        }
    };


    MarqueeSelection.prototype.getContent = function(){

        if (this._parentList.length!=1){
            for (var i=1; i<this._parentList.length; i++){
                var currAn = this.commonAncestor(this._parentList[0], this._parentList[i]);
                this._parentList[0] = currAn;
            }
        }

        var sel = this._parentList[0].cloneNode(true);
        this.rmChildNodes(sel, this._parentList[0]);
        return sel.innerHTML;
    };



    

    MarqueeSelection.prototype.commonAncestor = function(node1, node2) {
        var parents1 = this.parents(node1)
        var parents2 = this.parents(node2)

        if (parents1[0] != parents2[0]) throw "No common ancestor!"

        for (var i = 0; i < parents1.length; i++) {
            if (parents1[i] != parents2[i]) return parents1[i - 1]
        }
    };

    MarqueeSelection.prototype.parents = function(node) {
        var nodes = [node]
        for (; node; node = node.parentNode) {
            nodes.unshift(node)
        }
        return nodes
    };

    MarqueeSelection.prototype.intersectWith = function(myEl,el) {

        if (!el){
            return false};

        var bx1 = this._marqueeX1;
        var bx2 = this._marqueeX2;
        var by1 = this._marqueeY1;
        var by2 = this._marqueeY2;
        if (el.nodeName == "#text"){  
            this.setTextStyle(myEl, el);
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length==0){return;}
            for (var i=0; i<rects.length; i++){
                ax1 = rects[i].left;
                ax2 = rects[i].left + rects[i].width;
                ay1 = rects[i].top;
                ay2 = rects[i].top + rects[i].height;
                if (ax1<bx2 && ax2>bx1 && ay1<by2 && ay2>by1){
                    return true;
                }
            }
            return false
            
        }
        else if (el.nodeName != "#comment"){
            var rects = el.getBoundingClientRect();
            ax1 = rects.left;
            ax2 = rects.left + rects.width;
            ay1 = rects.top;
            ay2 = rects.top + rects.height;
        }

        /*
         //console.log(ax1+","+ay1+","+ax2+","+ay2);
         //console.log(bx1+","+bx2+","+by1+","+by2);*/

        if (ax1<bx2 && ax2>bx1 && ay1<by2 && ay2>by1){
            return true;
        }
        else{return false;}
    };

    MarqueeSelection.prototype.setStyle = function(el, trueEl) {
        trueStyle = document.defaultView.getComputedStyle(trueEl.parentNode);
        el.setAttribute("style", trueStyle);
    };

    MarqueeSelection.prototype.setTextStyle = function(el, trueEl) {
        var elStyle = document.defaultView.getComputedStyle(trueEl.parentElement);
        el = el.parentNode;
        el.style.font = elStyle.font;
    }

    MarqueeSelection.prototype.bound = function(myEl,el) {
        if (el.nodeName == "#text"){
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length==0){return;}

            for (var i=0; i<rects.length; i++){
            ax1 = rects[i].left;
            ax2 = rects[i].left + rects[0].width;
            ay1 = rects[i].top;
            ay2 = rects[i].top + rects[0].height;
            
            if(!(ax1>=this._marqueeX1 &&
                ax2<=this._marqueeX2 &&
                ay1>=this._marqueeY1 &&
                ay2<=this._marqueeY2)){ return false}
            }
            return true;
        }
        else if(el.nodeName!="#comment"){
            var rects = el.getBoundingClientRect();
            if (rects.left>=this._marqueeX1 &&
                rects.left+rects.width<=this._marqueeX2 &&
                rects.top>=this._marqueeY1 &&
                rects.top+rects.height<=this._marqueeY2){
                //console.log("TRUE");
                this.setTextStyle(myEl, el);
                return true;
            }
            //console.log("FALSE");
            return false;
        }
    };

    return MarqueeSelection;
})();





