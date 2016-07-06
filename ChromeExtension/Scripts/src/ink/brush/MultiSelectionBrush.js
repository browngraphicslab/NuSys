var MultiSelectionBrush = (function () {
    function MultiSelectionBrush(rect, toRemove) {
        this._rectlist = rect;
        this._list = new Array();
        this._remList = toRemove;
        console.log("new Brush!!!!");
    }
    MultiSelectionBrush.prototype.init = function (x, y, inkCanvas) {
    };
    MultiSelectionBrush.prototype.draw = function (x, y, inkCanvas) {
    };
    MultiSelectionBrush.prototype.setRectList = function (rectList) {
        this._clientRectList = rectList;
    };
    MultiSelectionBrush.prototype.drawStroke = function (stroke, inkCanvas) {
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
    };
    return MultiSelectionBrush;
})();
