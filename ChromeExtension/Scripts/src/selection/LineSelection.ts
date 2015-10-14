/// <reference path="../../typings/jquery/jquery.d.ts"/>
/// <reference path="../ink/InkCanvas.ts"/>
/// <reference path="../ink/brush/BrushStroke.ts"/>
/// <reference path="../ink/brush/HighlightBrush.ts"/>
/// <reference path="../ink/brush/SelectionBrush.ts"/>
/// <reference path="../util/Rectangle.ts"/>
/// <reference path="../util/DomUtil.ts"/>
/// <reference path="AbstractSelection.ts"/>

class LineSelection extends AbstractSelection {
    
    _brushStroke:BrushStroke;
    _inkCanvas:InkCanvas;
    _clientRects:ClientRectList;
    _range:Range;
    _content:string;

    constructor(inkCanvas: InkCanvas, fromActiveStroke: boolean = false) {
        super("LineSelection");
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

    start(x: number, y: number): void {
        this._inkCanvas.startDrawing(x, y, new HighlightBrush());
    }

    update(x: number, y: number): void {
        this._inkCanvas.draw(x, y);
    }

    end(x: number, y: number): void {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();

        this.select();

        this._inkCanvas.removeBrushStroke(this._brushStroke);
        this._inkCanvas.update();
    }

    deselect(): void {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    }

    getBoundingRect(): Rectangle {
        
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


    }


    addWordTag(nodes): void {

        $.each(nodes, (index, value) =>{

            if (value.nodeType == Node.TEXT_NODE) {
                $(value).replaceWith($(value).text().replace(/([^,\s]*)/g, "<word>$1</word>"));
            }
            else if (value.childNodes.length>0){
                this.addWordTag(value.childNodes);
            }
        });
    }

    analyzeContent(): void {

        console.log("analyzing content.");

        var stroke = this._brushStroke.stroke;
        var pStart = stroke.points[0];
        var pEnd = stroke.points[stroke.points.length - 1];
        var nStart = document.elementFromPoint(pStart.x, pStart.y);
        var nEnd = document.elementFromPoint(pEnd.x, pEnd.y);

        var commonParent = DomUtil.getCommonAncestor(nStart, nEnd);
        console.log(commonParent);
        var nodes = $(commonParent).contents();

        if (nodes.length > 0) {
            
            var original_content = $(commonParent).clone();

            $.each(nodes, function () {

                if (this.nodeType == Node.TEXT_NODE) {
                    $(this).replaceWith($(this).text().replace(/([^,\s]*)/g, "<word>$1</word>"));
                }
            });

            nStart = document.elementFromPoint(pStart.x, pStart.y);
            nEnd = document.elementFromPoint(pEnd.x, pEnd.y);
            this._range = new Range();
            this._range.setStart(nStart, 0);
            this._range.setEndAfter(nEnd);
            this._clientRects = this._range.getClientRects();

            $(nStart).nextUntil($(nEnd)).css("background-color", "yellow");
            $(nStart).css("background-color", "yellow");
            $(nEnd).css("background-color", "yellow");

            var frag = this._range.cloneContents();
            var result = "";
            $.each(frag["children"], function () {
               
                result += $(this)[0].outerHTML.replace(/<word>|<\/word>/g, " ");
            });
            result = result.replace(/\s\s+/g, ' ').trim();

            this._content = result;

            //$(commonParent).replaceWith(original_content);
            console.log("done analyzing line selection.");
        }
    }
    
    getContent(): string {
        return this._content;
    }
}