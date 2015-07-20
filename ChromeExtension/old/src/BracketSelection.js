/**
 * Created by phili_000 on 7/4/2015.
 */

var BracketSelection = (function () {

    function BracketSelection(inkCanvas, fromActiveStroke) {
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;

        if (fromActiveStroke) {
            this._inkCanvas.setBrush(new HighlightBrush());
            var t = this;
            $.each(inkCanvas._activeStroke.stroke.points, function(){
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
        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    };

    BracketSelection.prototype.deselect = function() {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };

    BracketSelection.prototype.getBoundingRect = function() {
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
    };

    /*
    BracketSelection.prototype.getBoundingRect = function() {

        var x = 1000000;
        var y = 1000000;
        var w = -1000000;
        var h = -1000000;

        var elems = this.getBracketedElements(this._brushStroke.stroke);
        $.each( elems, function() {
            var offset = $(this).offset();
            var wi = $(this).width();
            var he = $(this).height();
            x = offset.left < x ? offset.left : x;
            y = offset.top < y ? offset.top : y;
            w = wi > w ? wi : w;
            h = he > h ? he : h;
        });

        return new Rectangle(x,y,w,h);
    };
    */


    BracketSelection.prototype.getFineSelect = function(rects){
        //adds a div (highlighted) above selected elements for finer selection display

        var rect = rects[0];
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
            if (!rects[count]){return;}

            if (rect.left == rects[count].left && rect.width == rects[count].width){
                count++;
            }        
            rect = rects[count];
        }
    };

    BracketSelection.prototype.getContent = function () {
        return this._content;
    };

    BracketSelection.prototype.analyzeContent = function () {
        var stroke = this._brushStroke.stroke;
        var selectionBB = stroke.getBoundingRect();
        selectionBB.w = dwidth - selectionBB.x;

        var samplingRate = 20;
        var numSamples = 0;
        var totalScore = 0;
        var hitCounter = new Map();
        for(var x=selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate ){
            for(var y=selectionBB.y; y< selectionBB.y + selectionBB.h; y+= samplingRate ) {
                var hitElem = document.elementFromPoint(x, y);

                numSamples++;

                if (($(hitElem).width() * $(hitElem).height()) / (selectionBB.w * selectionBB.h) < 0.1)
                    continue;

                var score = (1.0 - x / (selectionBB.x + selectionBB.w)) / (selectionBB.w * selectionBB.h);

                if (hitCounter.get(hitElem) == undefined)
                    hitCounter.put(hitElem, score);
                else
                    hitCounter.put(hitElem, hitCounter.get(hitElem) + score);

                totalScore += score;

            }
        }


        var candidates = [];
        var precision = 4;

        for(var i = 0; i++ < hitCounter.size; hitCounter.next()) {
            candidates.push(hitCounter.value() / totalScore)
        }

        var std = getStandardDeviation( candidates, precision );

        var result = "";
        this._clientRects = [];

        for(var i = 0; i < hitCounter.size; i++) {

            if (isWithinStd(candidates[i],1, std)) {
                result += hitCounter.key().outerHTML;

                    ////Bracket Selection HightLighting/////////////////
                    console.log(hitCounter.key());
                    var range = document.createRange();
                    range.selectNodeContents(hitCounter.key());

                    var rects = range.getClientRects();
                    this._clientRects = this._clientRects.concat.apply([], rects);
                    //this.getFineSelect(rects);

                    //this._inkCanvas.removeLineSelectionStroke();
                    //this._inkCanvas.removeStroke();
                    //////////////////////////////////////////

            }

            hitCounter.next();
        }

        console.log(result);

        this._content =  result;
    };

    return BracketSelection;
})();





