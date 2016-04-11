

class BracketSelection extends AbstractSelection {

    _startX: number = 0;
    _startY: number = 0;
    _endX: number = 0;
    _endY: number = 0;
    _content: string = ""; 
    constructor() {
        super();
        console.log("BRACKET SELECTION");
    } 

    start(x: number, y: number): void {
        this._startX = x;
        this._startY = y;
        console.log("bracket start" + x + ":" + y);
    }

    end(x: number, y: number): void {
        this._endX = x;
        this._endY = y;
        this.samplePoints = [new Point(this._startX, this._startY), new Point(this._endX, this._endY)];
        this.analyzeContent();
        console.log("bracket end" + x + ":" + y);
    } 
    getContent(): string {
        return this._content;
    }

    analyzeContent(): void {
        console.log("bracket starts analyzing content....");
        var stroke = this.stroke;
        var selectionBB = stroke.getBoundingRect();


        selectionBB.w = Main.DOC_WIDTH - selectionBB.x; // TODO: fix this magic number

        var samplingRate = 50;
        var numSamples = 0;
        var totalScore = 0;
        var hitCounter = new collections.Dictionary<Element, number>((elem: Element) => { return (<HTMLElement>elem).outerHTML.toString() });
        var elList = [];
        var scoreList = [];

        for (var x = selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate) {
            for (var y = selectionBB.y; y < selectionBB.y + selectionBB.h; y += samplingRate) {
                var hitElem = document.elementFromPoint(x, y);

                if ($(hitElem).height() > selectionBB.h + selectionBB.h / 2.0) {
                    continue;
                } else {
                    // console.log("hit");
                    // console.log(hitElem); 
                }

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
        console.log(hitCounter);
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

        console.log("initial candidates");
        console.log(candidates);

        var std = Statistics.getStandardDeviation(candidates, precision);
        var maxDev = maxScore - 2 * std;

        var finalCandiates = [];
        hitCounter.forEach((k, v) => {
            if (v >= maxDev && v <= maxScore) {
                finalCandiates.push(k);
            }
        });

        console.log("initial candidates");
        console.log(finalCandiates);

        //finalCandiates = [finalCandiates[0]];

  
        /*
        var selectedElements = finalCandiates.filter((candidate) => {
            
            if ($(candidate).prop('style').float == "left") {
                console.log("found float");
                return true;
            }

            return false;
        });
        
        if (selectedElements.length == 0) {
            selectedElements = finalCandiates.filter((candidate) => {
           
                if ($(candidate).offset().left - selectionBB.x < 100) {
                    return true;
                }

                return false;
            });
        }
        */

        finalCandiates.concat().forEach((c) => {
            var maxDelta = 120;
            var largerParents = [];
            var parents = $(c).parents();
            for (var i = 0; i < parents.length; i++) {
                var parent = $(parents[i]);
                if (parent.width() - $(c).width() < maxDelta && parent.height() - $(c).height() < maxDelta) {
                    var index = finalCandiates.indexOf(c);
                    if (index > 0) {
                        finalCandiates.splice(index, 1);
                        largerParents.push(parent[0]);
                    }
                }
            }
            if (largerParents.length > 0)
                finalCandiates.push(largerParents.pop());
        });

        console.log("initial candidates with parents");
        console.log(finalCandiates);

        var selectedElements = finalCandiates.filter((candidate) => {

            if ($(candidate).offset().left - selectionBB.x < 100) {
                return true;
            }

            return false;
        });


        console.log("selected elements");
        console.log(selectedElements);

    //    this._clientRects = new Array<ClientRect>();
        var result = "";
        selectedElements.forEach((el) => {
     //       console.log(
            $(el).find("img")["andSelf"]().each((i, e: any) => {
                $(e).attr("src", e.src);
                $(e).removeAttr("srcset");
            });
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
     //       this._clientRects = this._clientRects.concat.apply([], rects);
            var index = $(el.tagName).index(el);
            this.selectedElements.push({ type: "bracket", tagName: el.tagName, index: index });
            console.log("selected element: " + el.tagName + "." + index);
            result += el.outerHTML;
            console.log(el);
            $(el).css("background-color", "yellow");

        });


           this._content = result;
    }


}