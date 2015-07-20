/**
 * Created by phili_000 on 7/4/2015.
 */

var LineSelection = (function () {

    function LineSelection(inkCanvas) {
        this._brushStroke = null;
        LineSelection.prototype._inkCanvas = inkCanvas;
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
        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    };

    /*
    LineSelection.prototype.getBoundingRect = function() {
        var bb = this._brushStroke.stroke.getBoundingRect();
        bb.x += this._brushStroke.stroke.documentOffsetX;
        bb.y += this._brushStroke.stroke.documentOffsetY - 5;
        bb.h = 22;
        return bb;
    };
    */

    LineSelection.prototype.deselect = function() {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };

    LineSelection.prototype.getBoundingRect = function(){
        //adds a div (highlighted) above selected elements for finer selection display
        ////

        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for(var i=0;i<this._clientRects.length;i++) {
            var p = this._clientRects[i];
            maxY = p.top + p.height > maxY ? p.top + p.height : maxY;
            maxX = p.left + p.width > maxX ? p.left + p.width : maxX;
            minX = p.left < minX ? p.left : minX;
            minY = p.top < minY ? p.top : minY;
        }

        return new Rectangle(minX, minY + window.pageYOffset, maxX - minX, maxY - minY);

        /*
        var rect = this._clientRects[0];
        var count = 0;
        while (rect){

            var tableRectDiv = document.createElement('div');
            tableRectDiv.style.position = 'absolute';
            tableRectDiv.style.background = 'yellow';
            tableRectDiv.style.opacity = "0.3";
            var scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
            var scrollLeft = document.documentElement.scrollLeft || document.body.scrollLeft;
            tableRectDiv.style.margin = tableRectDiv.style.padding = '0';
            tableRectDiv.style.top = (rect.top + scrollTop) + 'px';
            tableRectDiv.style.left = (rect.left + scrollLeft) + 'px';
            // we want rect.width to be the border width, so content width is 2px less.
            tableRectDiv.style.width = (rect.width) + 'px';
            tableRectDiv.style.height = (rect.height) + 'px';
            document.body.appendChild(tableRectDiv);     
            count++;
            if (!this._clientRects[count]){return;}
            if (rect.left == this._clientRects[count].left && rect.width == this._clientRects[count].width){
                count++;
            }        
            rect = this._clientRects[count];
        }
        */

    };

    LineSelection.prototype.getContent = function (stroke) {
        return this._content;
    };

    LineSelection.prototype.analyzeContent = function () {
        var stroke = this._brushStroke.stroke;
        var pStart = stroke.points[0];
        var pEnd = stroke.points[stroke.points.length-1];

        var nStart = document.elementFromPoint(pStart.x, pStart.y);
        var hi = nStart;
        var nEnd = document.elementFromPoint(pEnd.x, pEnd.y);

        var commonParent = DomUtil.getCommonAncestor(nStart, nEnd);

        var nodes = $(commonParent).contents();

        if (nodes.length > 0) {
            var original_content = $(commonParent).clone();

            $.each(nodes, function() {

                if (this.nodeType == Node.TEXT_NODE) {
                    $(this).replaceWith($(this).text().replace(/([^,\s]*)/g, "<word>$1</word>"));
                }
            });

            nStart = document.elementFromPoint(pStart.x, pStart.y);
            nEnd = document.elementFromPoint(pEnd.x, pEnd.y);
            this._range = new Range();
            this._range.setStart(nStart, 0);
            this._range.setEndAfter(nEnd, 0);
            this._clientRects =  this._range.getClientRects();

            var frag =  this._range.cloneContents();
            var result = "";
            $.each(frag.children, function() {
                result += $(this)[0].outerHTML.replace(/<word>|<\/word>/g, " ");
            });
            result = result.replace(/\s\s+/g, ' ').trim();

            this._content = result;

            $(commonParent).replaceWith(original_content);
        }
    };

    return LineSelection;
})();





