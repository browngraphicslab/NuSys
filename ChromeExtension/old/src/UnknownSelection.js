var UnknownSelection = (function () {

    function UnknownSelection(inkCanvas, fromActiveStroke) {
        UnknownSelection.prototype._inkCanvas = inkCanvas;

        if (fromActiveStroke) {
            var stroke = inkCanvas._activeStroke.stroke;
            inkCanvas.setBrush(new LineBrush());
        }
    }

    UnknownSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new LineBrush());
    };

    UnknownSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };

    UnknownSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke.stroke.getCopy();
    };

    UnknownSelection.prototype.getBoundingRect = function() {
        return this._brushStroke.getBoundingRect();
    };

    UnknownSelection.prototype.getContent = function (stroke) {
        return null;
    };

    return UnknownSelection;
})();





