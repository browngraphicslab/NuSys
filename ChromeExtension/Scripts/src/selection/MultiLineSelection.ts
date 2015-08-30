class MultiLineSelection implements ISelection {

    _brushStroke: BrushStroke;
    _inkCanvas: InkCanvas;
    _brush: MultiSelectionBrush;
    _currLineTop: Number;
    _range: Range;
    _nStart: Element;
    _offsetStart: number;
    _nEnd: Element;
    _offsetEnd: number;
    _clientRects: ClientRectList;
    _extraRects: Array<Rectangle>;
    _rect: ClientRect;
    _currWord: Element;
    _rectangle: Rectangle;
    _rectList: Array<Rectangle>;
    _currParent: Element;
    _content: DocumentFragment;
    _temp: Element;
    _prevList: Array<ClientRect>;
    _imgList: Array<ClientRect>;




    constructor(inkCanvas: InkCanvas, fromActiveStroke: boolean = false) {
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        this._rectList = new Array<Rectangle>();
        this._currLineTop = 0;
        console.log("===============constructor============");
        if (fromActiveStroke) {
            this._inkCanvas.setBrush(new HighlightBrush());
            var t = this;
            $.each(inkCanvas._activeStroke.stroke.points, function () {
                t._inkCanvas.draw(this.x, this.y);
            });
        }


    }

    addWordTag(nodes): void {

        console.log(nodes);

        $.each(nodes,(index, value) => {

            if (value.nodeType == Node.TEXT_NODE) {
                $(value).replaceWith("<span>" + $(value).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</span>");
                //$(value).find("span").find("word").each(function (){
                //    console.log(this);
                //});
            }
            else if (value.childNodes.length > 0) {
                this.addWordTag(value.childNodes);
            }
        });
    }

    start(x: number, y: number): void {

        console.log("===================start===============");
        console.log(document.elementFromPoint(x, y));
        //      this.addWordTag(document.elementFromPoint(x, y).childNodes);
        this._currParent = document.elementFromPoint(x, y);
        var rg = document["caretRangeFromPoint"](x, y);
        this._nStart = rg.commonAncestorContainer;
        this._offsetStart = rg.startOffset;
        console.log(this._offsetStart);
        this._prevList = Array<ClientRect>();



    }

    getTextRectangles = (cont: Element, nEnd: Element) => {
        console.log(cont.childNodes);
        $(cont.childNodes).each((index, el) => {
            console.log(el);
            console.log(el.nodeName);

            if (el.nodeName == "#text") {
                var range = document.createRange();
                range.selectNodeContents(el);
                console.log(range);
                console.log(range.getClientRects());
                console.log(range.getBoundingClientRect());
            }
        });
        return new Array<Rectangle>();
    }

    getNodesInRange = (range: Range): Array<Node>=> {
        var start = range.startContainer;
        var end = range.endContainer;
        var commonAncestor = range.commonAncestorContainer;
        var nodes = [];
        var node;

        // walk parent nodes from start to common ancestor
        for (node = start.parentNode; node; node = node.parentNode) {
            nodes.push(node);
            if (node == commonAncestor)
                break;
        }
        nodes.reverse();

        // walk children and siblings from start until end is found
        for (node = start; node; node = this.getNextNode(node)) {
            nodes.push(node);
            if (node == end)
                break;
        }

        return nodes;
    }

    getNextNode = (node: Node): Node => {
        if (node.firstChild)
            return node.firstChild;
        while (node) {
            if (node.nextSibling)
                return node.nextSibling;
            node = node.parentNode;
        }
    }
    isDescendant = (parent: Node, child: Node): Boolean => {
        var node = child.parentNode;
        while (node != null) {
            if (node == parent) {
                return true;
            }
            node = node.parentNode;
        }
        return false;
    }


    getTextNodesBetween = (range): Array<Node>=> {
        var rootNode = range.commonAncestorContainer,
            startNode = range.startContainer, endNode = range.endContainer,
            startOffset = range.startOffset, endOffset = range.endOffset,
            pastStartNode = false, reachedEndNode = false, textNodes = [];

        function getTextNodes(node) {
            var val = node.nodeValue;
            if (node == startNode && node == endNode && node !== rootNode) {
                if (val) textNodes.push(node);
                console.log(node);

                pastStartNode = reachedEndNode = true;
            } else if (node == startNode) {
                if (val) textNodes.push(node);
                pastStartNode = true;
                console.log(node);
            } else if (node == endNode) {
                if (val) textNodes.push(node);
                reachedEndNode = true;
                console.log(node);
            } else if (node.nodeType == 3) {
                if (val && pastStartNode && !reachedEndNode && !/^\s*$/.test(val)) {
                    //    textNodes.push(val);
                    textNodes.push(node);
                    console.log(node);
                }
            }
            //else if (node.nodeName == "IMG") {
            //    //list.push(node);
            //    addEventLis
            //}

            for (var i = 0, len = node.childNodes.length; !reachedEndNode && i < len; ++i) {
                getTextNodes(node.childNodes[i]);
            }
        }
        getTextNodes(rootNode);
        return textNodes;
    }

    update = (x: number, y: number): void => {

        this._inkCanvas.draw(x, y);

        var rg = document["caretRangeFromPoint"](x, y);
        var nEnd = rg.commonAncestorContainer;
        var offsetEnd = rg.startOffset;
        var offsetStart = this._offsetStart;
        this._nEnd = nEnd;
        this._range = document.createRange();
        this._range.setStart(this._nStart, this._offsetStart);
        
        this._range.setEnd(nEnd, offsetEnd);
        var ans = this._range.commonAncestorContainer;
        var nodes = this.getTextNodesBetween(this._range);
        var list = [];
        
        $(nodes).each(function (indx, ele) {
            
            var rg = document.createRange();
            if (indx == 0) {
                if ($(nodes).length == 1) {
                    rg.setStart(ele, offsetStart);
                    rg.setEnd(ele, offsetEnd);
                }
                else {
                        rg.setStart(ele, offsetStart);
                        rg.setEndAfter(ele);
                }
            }
            else if (indx == $(nodes).length - 1) {
                rg.setStartBefore(ele);
                
                rg.setEnd(ele, offsetEnd);
            }
            else {
                rg.selectNode(ele);
            }
            console.log(rg.getClientRects());
            $(rg.getClientRects()).each(function (idx, el) {
                list.push(el);
            });
        });


        this._brushStroke = this._inkCanvas._activeStroke;


        this._brushStroke.brush = new MultiSelectionBrush(list, this._prevList);
        this._brushStroke.brush.drawStroke(null, this._inkCanvas);
        this._prevList = list;
        
        //if (this._prevList == null) {
        //    console.log("prev is null");
        //    this._brushStroke.brush = new MultiSelectionBrush(list, []);
        //}
        //else if (list.length > this._prevList.length) {
        //    ///delete last element of prevlist and add 
        //    console.log("more clientrect selected");
        //    var diff = list.length - this._prevList.length;
        //    this._brushStroke.brush = new MultiSelectionBrush(list.slice(list.length - diff), [this._prevList[this._prevList.length - 1]]);
        //}
        //else if (list.length < this._prevList.length) {
        //    ////remove previous and check last 
        //    console.log("less clientrect selected!!!");
        //}
        //else {
        //    ////check the last rect 
        //    console.log("selection within same rect");
        //    this._brushStroke.brush = new MultiSelectionBrush([list[list.length - 1]], [this._prevList[this._prevList.length - 1]]);
        //}
        
    }


    end(x: number, y: number): void {
        this._inkCanvas.endDrawing(x, y);
        //this._brushStroke = this._inkCanvas._activeStroke;

        this.analyzeContent();
        //  this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    }

    deselect(): void {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    }

    getBoundingRect(): Rectangle {
        return new Rectangle(1, 10, 10, 10);
    }

    analyzeContent(): void {
        var content = this._range.cloneContents();
        console.log(content);
    }
    getContent(): string {
        // console.log("getContent =======================");
        var d = document.createElement('div');
        d.appendChild(this._range.cloneContents());
        console.log(d.innerHTML);
        return d.innerHTML;
        //return this._range.cloneContents();
    }
}
