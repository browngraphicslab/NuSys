/// <reference path="../../lib/collections.ts"/>

class BracketSelection implements ISelection {
    
    _brushStroke:BrushStroke;
    _inkCanvas:InkCanvas;
    _clientRects:Array<ClientRect>;
    _range:Range;
    _content:string;

    constructor(inkCanvas:InkCanvas, fromActiveStroke:boolean = false) {
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;

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
        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
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
        console.log(this._clientRects.length);
        for (var i = 0; i < this._clientRects.length; i++) {
            var p = this._clientRects[i];
            maxY = p.top + p.height > maxY ? p.top + p.height : maxY;
            maxX = p.left + p.width > maxX ? p.left + p.width : maxX;
            minX = p.left < minX ? p.left : minX;
            minY = p.top < minY ? p.top : minY;
        }

        return new Rectangle(minX, minY + window.pageYOffset, maxX - minX, maxY - minY);
    }

    analyzeContent(): void {
        
        console.log("analyzing content");

        var stroke = this._brushStroke.stroke;
        var selectionBB = stroke.getBoundingRect();
        selectionBB.w = Main.DOC_WIDTH - selectionBB.x; // TODO: fix this magic number

        var samplingRate = 20;
        var numSamples = 0;
        var totalScore = 0;
        //var hitCounter = new Map<Element, number>();
        var hitCounter = new collections.Dictionary<Element, number>();
        for (var x = selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate) {
            for (var y = selectionBB.y; y < selectionBB.y + selectionBB.h; y += samplingRate) {
                var hitElem = document.elementFromPoint(x, y);

                numSamples++;

                if (($(hitElem).width() * $(hitElem).height()) / (selectionBB.w * selectionBB.h) < 0.1)
                    continue;

                var score = (1.0 - x / (selectionBB.x + selectionBB.w)) / (selectionBB.w * selectionBB.h);

                if (hitCounter.getValue(hitElem) == undefined)
                    hitCounter.setValue(hitElem, score);
                else
                    hitCounter.setValue(hitElem, hitCounter.getValue(hitElem) + score);

                totalScore += score;

            }
        }
 

        var candidates = [];
        var precision = 4;
        console.log("numCandidates: " + candidates.length)
        hitCounter.forEach((k, v) => {            
            candidates.push(v / totalScore);
        });    
        
        console.log(candidates)  

        var std = Statistics.getStandardDeviation(candidates, precision);

        var result = "";
        this._clientRects = new Array<ClientRect>();

        var count = 0;
        var result = "";

        hitCounter.forEach((k, v) => {
            console.log(k)
            if (Statistics.isWithinStd(candidates[count++], 1, std)) {

                result += k["outerHTML"];
                var range = document.createRange();
                range.selectNodeContents(k);

                var rects = range.getClientRects();
                this._clientRects = this._clientRects.concat.apply([], rects);
                console.log(rects.length);
            }
        });

        console.log(result);

        this._content = result;
    }

    getContent(): string {
        return this._content;
    }
}