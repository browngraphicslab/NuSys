var Editor = (function () {
    function Editor(document, element, preview) {
        this.preview = preview;
        this.document = document;
        this.element = element;
        this.element.setAttribute("contenteditable", "true");
        this.activateButtons(".btn");
        this.activateButtons(".dropdown-menu li a");
        this.activateLinkCreator();
        this.clickableLinks();
       // this.updateText();
        this.updateNodeView();

    }
    Editor.prototype.activateButtons = function (s) {
        var buttons = document.querySelectorAll(s);
        var editor = this;
        for (var i = 0; i < buttons.length; i++) {
            buttons[i].addEventListener("mousedown", function (e) {
                e.preventDefault();
                editor.saveSelection();
                var tag = this.getAttribute("data-edit");
                document.execCommand(tag, false, this.getAttribute("data-value"));
                editor.restoreSelection();
                return false;

            });
            buttons[i].addEventListener("click", function(e) {
                e.preventDefault();
                return false;
            });
        }
    };
    Editor.prototype.activateLinkCreator = function () {
        var linkBox = document.getElementById("linkBox");
        var linkBoxOuter = document.getElementById("linkBoxOuter");
        var marked = false;
        var editor = this;
        //linkBoxOuter.addEventListener("mouseenter", function (e) {
        //    editor.markSelection("#c6d9ec");
        //}, true);
        //linkBoxOuter.addEventListener("mouseleave", function (e) {
        //    editor.markSelection("transparent");
        //});
        linkBox.addEventListener("click", function (e) {
            e.preventDefault();//
            e.stopImmediatePropagation();//
            linkBox.focus();//
            console.log("CLICK IN BOX");
        }, true);
        linkBox.addEventListener("focus", function (e) {
            console.log("FOCUS");
           // e.stopImmediatePropagation();//
            input.focus();
            if (!marked) {
                editor.markSelection("#c6d9ec");
                marked = true;
            }
        });
        linkBox.addEventListener("change", function (e) {
            console.log("Change");
           // editor.focus();
            editor.createLink(this);//
        });
        linkBox.addEventListener("blur", function (e) {
           if (marked) {
                editor.markSelection("transparent");
                marked = false;
           }
        });
    };
    Editor.prototype.createLink = function (el) {
        var editor = this;
        var element = el;
        console.log("Creating link");
        var newValue = element.value;
        element.value = '';
        editor.restoreSelection();
        if (newValue) {
            editor.element.focus();
            // linkBox.focus();
            editor.markSelection("white");
            var tag = el.getAttribute("data-edit");
            document.execCommand(tag, false, newValue);
            this.clickableLinks();
        }
    };
    Editor.prototype.clickableLinks = function () {
        var links = document.querySelectorAll("#editor a");
        for (var i = 0; i < links.length; i++) {
            links[i].addEventListener("click", function (e) {
                var currlink = this.getAttribute("href");
                window.external.notify('LaunchMyLink:' + currlink);
                return false;
                //window.open(l, null, "height=500,width=500,status=no,toolbar=no,menubar=no,location=no");
            });
        }
    };
    Editor.prototype.markSelection = function (color) {
        this.restoreSelection();
        if (document.queryCommandSupported('hiliteColor')) {
            document.execCommand("hiliteColor", false, color);
        }
        this.saveSelection();
    };
    Editor.prototype.saveSelection = function () {
        this.selectedRange = this.getCurrentRange();
    };
    Editor.prototype.getCurrentRange = function () {
        var sel = window.getSelection();
        if (sel.getRangeAt && sel.rangeCount) {
            return sel.getRangeAt(0);
        }
    };
    Editor.prototype.restoreSelection = function () {
        var editor = this;
        var selection = window.getSelection();
        console.log("trying to restore");
        if (editor.selectedRange) {
            console.log("restoring");
            selection.removeAllRanges();
            selection.addRange(this.selectedRange);
        }
    };
    Editor.prototype.cleanHtml = function () {
        var html = this.element.innerHTML;
        return html && html.replace(/(<br>|\s|<div><br><\/div>|&nbsp;)*$/, "");
    };
    //Editor.prototype.updateText = function () {
    //    var editor = this;
    //    this.element.addEventListener("keyup", function () {
    //        var prev = document.getElementById("editorPreview");
    //        prev.innerHTML = editor.cleanHtml();
    //    });
    //};


    Editor.prototype.InsertText = function (text) { // function to update text in the editor
        this.element.innerHTML = text;
    };

    Editor.prototype.updateNodeView = function () {
        //this.preview.innerHTML = "<p> HI there!</p>";

        //this.addEventListener("mousemove", function(e) {
            //this.preview.innerHTML = this.element.innerHTML;

            // window.external.notify(this.cleanHtml());
       // });
        var editor = this;
        //this.element.addEventListener("keyup", function () {
        //    editor.preview.innerHTML = editor.cleanHtml();
        //});

        //this.preview.innerHTML = "hi";
        
        this.element.addEventListener("keyup", function() {
            editor.preview.innerHTML = editor.cleanHtml();
            window.external.notify(editor.cleanHtml());
        });

       // this.preview.innerHTML = "ho";

        this.element.addEventListener("change", function() {
            editor.preview.innerHTML = editor.cleanHtml();
            window.external.notify(editor.cleanHtml());
        });
       // this.preview.innerHTML = "hey";

    };

    return Editor;
})();

window.onload = function () {
    var el = document.getElementById('editor');
    var prev = document.getElementById('editorPreview');
    new Editor(document, el, prev);
};
//# sourceMappingURL=app.js.map