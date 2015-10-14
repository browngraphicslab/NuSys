/// <reference path="../ink/GestureType.ts"/>

class MultiLineSelection extends AbstractSelection {

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
    _content: string;
    _temp: Element;
    _prevList: Array<ClientRect>;
    _imgList: Array<ClientRect>;
    _startX: Number;
    _startY: Number;


    _startElement: Element;
    _endElement:Element;

    constructor(inkCanvas: InkCanvas, fromActiveStroke: boolean = false) {
        super("MultiLineSelection");
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        this._rectList = new Array<Rectangle>();
        this._currLineTop = 0;
    }

    start(x: number, y: number): void {
        console.log("multiline start.");
        this._startElement = document.elementFromPoint(x, y);
    }   

    update (x: number, y: number): void {
    }

    end(x: number, y: number): void {
        
        if (window.getSelection) {
            var sel = window.getSelection();



            if (sel.rangeCount) {
                var range = sel.getRangeAt(0).cloneRange();

                var d = document.createElement('div');
                d.appendChild(range.cloneContents());
                this._content = d.innerHTML;
                             
                var start = range["startContainer"];
                var end = range["endContainer"];

                var startParent = start.parentElement;
                var startParentIndex = $(startParent.tagName).index(startParent);
                var startIndex = Array.prototype.indexOf.call(startParent.childNodes, start);

                var endParent = end.parentElement;
                var endParentIndex = $(endParent.tagName).index(endParent);
                var endIndex = Array.prototype.indexOf.call(endParent.childNodes, end);
                var selectionInfo = {
                    start: { tagName: startParent.tagName, parentIndex: startParentIndex, textIndex: startIndex, offset: range.startOffset },
                    end: { tagName: endParent.tagName, parentIndex: endParentIndex, textIndex: endIndex, offset: range.endOffset }
                };
                this.selectedElements.push(selectionInfo);
 
                this.highlightMultiline(selectionInfo.start, selectionInfo.end);
           
               

                window.getSelection().removeAllRanges();
            }
        }
    }

    highlightMultiline(start: any, end: any): void {


        var startNode, endNode, startParentNode, endParentNode;

        if (start.id != null) {
            startParentNode = $('[data-ctedid="' + start.id + '"]')[0];
            endParentNode = $('[data-ctedid="' + end.id + '"]')[0];
        } else {
            startParentNode = $(start.tagName)[start.parentIndex];
            endParentNode = $(end.tagName)[end.parentIndex];
        }

        startNode = startParentNode.childNodes[start.textIndex];
        endNode = endParentNode.childNodes[end.textIndex];

        console.log("-----------------");
        console.log(startNode);
        console.log(endNode );


        if (startNode != endNode) {

            var newStart = document.createElement("span");
            newStart.innerHTML = "<span>" + startNode.nodeValue.substring(0, start.offset) + "</span><span style='background-color:yellow;'>" + startNode.nodeValue.substring(start.offset, startNode.nodeValue.length) + "</span>";
            startNode.parentNode.replaceChild(newStart, startNode);

            var newEnd = document.createElement("span");
            newEnd.innerHTML = "<span style='background-color:yellow;'>" + endNode.nodeValue.substring(0, end.offset) + "</span>" + "<span>" + endNode.nodeValue.substring(end.offset, endNode.nodeValue.length) + "</span>";
            endNode.parentNode.replaceChild(newEnd, endNode);

            this.highlightSiblingsOf(newStart.nextSibling, newEnd);
            if (newStart.parentNode != newEnd.parentNode) {
                this.highlightSiblingsOf(newEnd.parentNode.firstChild, newEnd);
            }

            var between = this.getElementsBetweenTree(newStart, newEnd);
            $(between).css("background-color", "yellow");
        } else {
            console.log("blaah");
            var newStart = document.createElement("span");
            newStart.innerHTML = "<span>" + startNode.nodeValue.substring(0, start.offset) + "</span><span style='background-color:yellow;'>" + startNode.nodeValue.substring(start.offset, end.offset) + "</span>" + "<span>" + startNode.nodeValue.substring(end.offset, endNode.nodeValue.length) + "</span>";
            startNode.parentNode.replaceChild(newStart, startNode);
        }
    }

    select(): void {
        console.log("select!!");
        this.selectedElements.forEach((el:any) => {
            this.highlightMultiline(el.start, el.end);
        });
    }

    highlightSiblingsOf(next: any, newEnd:any): void {
        while (next != null) {
            var sib = next.nextSibling;

            if (next == newEnd) {
                console.log("breaking");
                break;
            }

            if (next.innerHTML == undefined) {

                var newNode = document.createElement("span");
                newNode.style.backgroundColor = "yellow";
                newNode.innerHTML = next.nodeValue;
                if (next.parentNode != null)
                    next.parentNode.replaceChild(newNode, next);
                else {
                    console.log("parent is null:");
                }

            } else {
                next.style.backgroundColor = "yellow";
            }
            next = sib;
        }
    }
    
    getElementsBetweenTree(start, end):any[] {
        var ancestor = this.getCommonAncestor(start, end);

        var before = [];
        while (start.parentNode !== ancestor) {
            var el = start;
            while (el.nextSibling)
                before.push(el = el.nextSibling);
            start = start.parentNode;
        }

        var after = [];
        while (end.parentNode !== ancestor) {
            var el = end;
            while (el.previousSibling)
                after.push(el = el.previousSibling);
            end = end.parentNode;
        }
        after.reverse();

        while ((start = start.nextSibling) !== end)
            before.push(start);
        return before.concat(after);
    }

    getCommonAncestor(a, b) {
        var parents = $(a).parents()["andSelf"]();
        while (b) {
            var ix = parents.index(b);
            if (ix !== -1)
                return b;
            b = b.parentNode;
        }
        return null;
    }

    getBoundingRect(): Rectangle {
        return new Rectangle(1, 10, 10, 10);
    }

    analyzeContent(): void {
        
    }

    getContent(): string {
        return this._content;
    }
}