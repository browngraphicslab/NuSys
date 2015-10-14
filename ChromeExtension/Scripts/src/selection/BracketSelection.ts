/// <reference path="../../lib/collections.ts"/>

class BracketSelection extends AbstractSelection{
    
    _brushStroke:BrushStroke;
    _inkCanvas:InkCanvas;
    _clientRects:Array<ClientRect>;
    _range:Range;
    _content:string;

    constructor(inkCanvas: InkCanvas, fromActiveStroke: boolean = false) {
        super("BracketSelection");
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
        this.select();

        this._inkCanvas.removeBrushStroke(this._brushStroke);
        this._inkCanvas.update();
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

    analyzeContent(): void {
        
        var stroke = this._brushStroke.stroke;
        var selectionBB = stroke.getBoundingRect();

       
        selectionBB.w = Main.DOC_WIDTH - selectionBB.x; // TODO: fix this magic number

        var samplingRate = 30;
        var numSamples = 0;
        var totalScore = 0;
        var hitCounter = new collections.Dictionary<Element, number>((elem:Element)=>{return (<HTMLElement>elem).outerHTML.toString()});
        var elList = [];
        var scoreList = [];
        
        for (var x = selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate) {
            for (var y = selectionBB.y; y < selectionBB.y + selectionBB.h; y += samplingRate) {
                var hitElem = document.elementFromPoint(x, y);

                if ($(hitElem).height() > selectionBB.h + 50)
                    continue;

                numSamples++;

                //if (($(hitElem).width() * $(hitElem).height()) / (selectionBB.w * selectionBB.h) < 0.1)
                //    continue;
                var score = 1.0 - Math.sqrt((x - selectionBB.x) / selectionBB.w);
                
                if (elList.indexOf(hitElem) < 0) {
                    elList.push(hitElem);
                    scoreList.push(score);
                }
                else {
                    scoreList[elList.indexOf(hitElem)] += score;
                }
                
                if (!hitCounter.containsKey(hitElem)) {
                    hitCounter.setValue(hitElem, score);
                }
                else {
                    hitCounter.setValue(hitElem, hitCounter.getValue(hitElem) + score);
                }

                totalScore += score;
            }
        }
        
        var maxScore = -10000;
        var bestMatch = null;
        hitCounter.forEach((k, v) => {

            if (v > maxScore) {
                maxScore = v;
                bestMatch = k;
            }
        });
        
        var candidates = [];
        var precision = 4;

        hitCounter.forEach((k, v) => {            
            candidates.push(v);
        });
        
        var std = Statistics.getStandardDeviation(candidates, precision);
        var maxDev = maxScore - 2 * std;

        var finalCandiates = [];
        hitCounter.forEach((k, v) => {
            if (v >= maxDev && v <= maxScore) {
                finalCandiates.push(k);
            }
        });

        var selectedElements = finalCandiates.filter((candidate) => {
            var containedByOtherCandidate = false;
            finalCandiates.forEach((otherCandidate) => {
                if (candidate != otherCandidate && $(otherCandidate).has(candidate)) {
                    containedByOtherCandidate = true;
                }
            });
            return !containedByOtherCandidate;
        });

        this._clientRects = new Array<ClientRect>();
        var result = "";
        selectedElements.forEach((el) => {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            this._clientRects = this._clientRects.concat.apply([], rects);
            var index = $(el.tagName).index(el);
            this.selectedElements.push({ type: "bracket", tagName: el.tagName, index: index });
           result += el.outerHTML;
        });
        console.log(this._clientRects);
        console.log("final candidates");
        console.log(selectedElements);

        this._content = result;
    }

    getContent(): string {
        return this._content;
    }
}