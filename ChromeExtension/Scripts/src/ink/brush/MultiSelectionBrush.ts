class MultiSelectionBrush implements IBrush {

    _rect: Rectangle;
    _list: Array<ClientRect>;
    _remList: Array<ClientRect>;
    _rectlist: Array<ClientRect>;
    _clientRectList: ClientRectList;
    _diff: Number;

    constructor(rect: Array<ClientRect>, toRemove: Array<ClientRect>) {
        this._rectlist = rect;
        this._list = new Array<ClientRect>();
        this._remList = toRemove;
        console.log("new Brush!!!!");

    }

    init(x: number, y: number, inkCanvas: InkCanvas): void {
        // do nothing
    }

    draw(x: number, y: number, inkCanvas: InkCanvas) {
        // do nothing.
    }

    setRectList(rectList: ClientRectList): void {

        this._clientRectList = rectList;

    }

    drawStroke(stroke: Stroke, inkCanvas: InkCanvas) {

        console.log("draw Stroke =========================================");
        var ctx = inkCanvas._context;

        ctx.globalCompositeOperation = "source-over";
        ctx.globalAlpha = 0.6;
        ctx.beginPath();
        ctx.fillStyle = "rgb(255,255,204)";

        console.log(this._rectlist);
        for (var i = 0; i < this._remList.length; i++) {
            var el = this._remList[i];
            ctx.clearRect(el.left, el.top, el.width, el.height);

        }
        for (var i = 0; i < this._rectlist.length; i++) {
            var el = this._rectlist[i];
            ctx.fillRect(el.left, el.top, el.width, el.height);
        }

        ctx.fill();
    }
}
        //var diff = this._rectlist.length - this._remList.length;
        
        //if (diff > 0) {
        //    console.log("add!!!!");
        //    var rec = this._remList[this._remList.length - 1];
        //    if (rec != null) {
        //        ctx.clearRect(rec.left, rec.top, rec.width, rec.height);
        //        var newRec = this._rectlist[this._remList.length - 1];
        //        ctx.fillRect(newRec.left, newRec.top, newRec.width, newRec.height);
        //    }
        //    console.log("===========!!!!" + diff);
        //    $(this._rectlist).each((indx, elem) => {
        //        console.log("====================" + indx);
        //        console.log(this._remList.length - 1);
        //        if (indx >= this._remList.length - 1) {
        //            console.log(elem);
        //            var x = elem.clientLeft;
        //            var y = elem.clientTop;
        //            var w = elem.clientWidth;
        //            var h = elem.clientHeight;

        //            ctx.fillRect(x, y, w, h);
        //            console.log("DRAWN");
        //        }
        //    });
        //}
        //else if (diff < 0) {
        //    console.log("must remove");
        //}

        //else {
        //    console.log("remove last & add last");
        //    var rec = this._remList[this._remList.length - 1];
        //    ctx.clearRect(rec.left, rec.top, rec.width, rec.height);
        //    var newrec = this._rectlist[this._rectlist.length - 1];
        //    ctx.fillRect(newrec.left, newrec.top, newrec.width, newrec.height);
        //}
        //for (var i = 0; i < this._rectlist.length; i++) {
        //        var startX = this._rectlist[i].left;
        //        var startY = this._rectlist[i].top;
        //        var w = this._rectlist[i].width;
        //        var h = this._rectlist[i].height;
        //        var rect = new Rectangle(startX, startY, w, h);
        //        console.log(rect);
        //        var count = 0;
        //        for (var j = 0; j < this._remList.length; j++) {
        //            if (this._remList[j].x == rect.x && this._remList[j].y == rect.y && this._remList[j].w == rect.w && this._remList[j].h == rect.h) {
        //                console.log("==========REMOVEREMOVEREMOVE====================");
        //                count++;
        //            }
        //        }
        //        if (count==0) {
        //            ctx.fillRect(startX, startY, w, h);
        //        }
        //        else {
        //            console.log("=====================RECTLISTREMOVED============================");
        //            console.log(rect);
        //        }
        //}
        //for (var i = 0; i < this._remList.length; i++) {
        //    var startX = this._remList[i].left;
        //    var startY = this._remList[i].top;
        //    var w = this._remList[i].width;
        //    var h = this._remList[i].height;
        //    ctx.clearRect(startX, startY, w, h);
        

      //  ctx.fill();
       // this._list = new Array<ClientRect>();