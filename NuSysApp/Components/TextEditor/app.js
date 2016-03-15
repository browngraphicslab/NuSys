var myEditor;
var Editor = (function () {
    function Editor(document, element, preview) {
        this.document = document; // entire document
        this.element = element; // div containing text editor
        this.preview = preview;
        this.element.setAttribute("contenteditable", "true"); //makes text editor editable
        this.activateButtons(".btn"); // toolbar push buttons 
        this.activateButtons(".dropdown-menu li a"); // toolbar dropdown menus 
        this.activateLinkCreator(); // creating links
        this.clickableLinks(); // making links clickable in detail view
        this.updateNodeView(); // updating text node as changes are made in text detail editor
    }

    /**
     * Activating toolbar buttons for the text editor.
     * Calls execCommand, which takes in "data-edit" (a command, such as Bold, Underline, etc)
     * and "data-value"if applicable (a font family, a font size, or link href)
     * 
     * @param {} s 
     * @returns {} false: to maintain focus in the text editor
     */
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
            buttons[i].addEventListener("click", function (e) {
                e.preventDefault();
                return false; //prevents text editor box from losing focus
            });
        }
    };

    /**
     * Functionality for Linking button/box
     * Various event ilsteners:
     * mouseenter: highlights text selection (will be lost as focus shifts to link box)
     * mouseleave: unhighlights text selection
     * click: focuses on linkBox
     * change: calls createLink, which calls the execCommand link function
     * 
     * @returns {} 
     */
    Editor.prototype.activateLinkCreator = function () {
        var linkBox = document.getElementById("linkBox");
        var linkBoxOuter = document.getElementById("linkBoxOuter");
        var marked = false;
        var editor = this;
        linkBoxOuter.addEventListener("mouseenter", function (e) {
            editor.markSelection("#c6d9ec");
        }, true);
        linkBoxOuter.addEventListener("mouseleave", function (e) {
            editor.markSelection("transparent");
        });
        linkBox.addEventListener("click", function (e) {
            e.preventDefault();//
            e.stopImmediatePropagation();//
            linkBox.focus();//
        }, true);
        linkBox.addEventListener("focus", function (e) {
            // e.stopImmediatePropagation();//
            input.focus();
            if (!marked) {
                editor.markSelection("#c6d9ec");
                marked = true;
            }
        });
        linkBox.addEventListener("change", function (e) {
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

    /**
     * Creates link after link is entered in link box
     * 
     * @param {} el 
     * @returns {} 
     */
    Editor.prototype.createLink = function (el) {
        var editor = this;
        var element = el;
        console.log("Creating link");
        var newValue = element.value;
        element.value = '';
        editor.restoreSelection();
        if (newValue) {
            editor.element.focus();
            editor.markSelection("white");
            var tag = el.getAttribute("data-edit");
            document.execCommand(tag, false, newValue);
            this.clickableLinks();
        }
    };

    /**
     * Parses entire document to add click event listeners to links
     * window.external.notify sends click event to C#, to open links in detail view
     * 
     * @returns {} 
     */
    Editor.prototype.clickableLinks = function () {
        var links = document.querySelectorAll("#editor a");
        for (var i = 0; i < links.length; i++) {

            $(links[i]).popover({
                html: true,
                placement: 'bottom',
                content: "<div id = 'buttons'><input type = 'button' value = 'NUSYS' id = 'nusys'><input type = 'button' value = 'BROWSER' id = 'browser'></div>"
            });

            links[i].addEventListener("click", function (e) {

                var buttons = document.getElementById("buttons");
                // after countless hours trying every permutation: the line below is the only way to make the popover element unclickable.
                buttons.parentElement.parentElement.setAttribute("contenteditable", "false");

                var link = this.getAttribute("href");
                e.preventDefault();

                var nusys = document.getElementById("nusys");
                nusys.addEventListener("click", function (e) {
                    e.preventDefault();
                    window.external.notify('LaunchMyLink:' + link);
                });

                var browser = document.getElementById("browser");
                browser.addEventListener("click", function (e) {
                    e.preventDefault();
                    window.external.notify('BrowserOpen:' + link);
                });

                return false;
            });
        }

    };

    /**
     * Highlights selection during text link creation
     * 
     * @param {} color 
     * @returns {} 
     */
    Editor.prototype.markSelection = function (color) {
        this.restoreSelection();
        if (document.queryCommandSupported('hiliteColor')) {
            document.execCommand("hiliteColor", false, color);
        }
        this.saveSelection();
    };

    /**
     * Saves current text selection during various functions 
     * 
     * @returns {} 
     */
    Editor.prototype.saveSelection = function () {
        this.selectedRange = this.getCurrentRange();
    };

    /**
     * Gets current text selection from editable text region
     * @returns {} 
     */
    Editor.prototype.getCurrentRange = function () {
        var sel = window.getSelection();
        if (sel.getRangeAt && sel.rangeCount) {
            return sel.getRangeAt(0);
        }
    };

    /**
     * Restores current text selection in editor: important as text editor loses focus/regains focus
     * @returns {} 
     */
    Editor.prototype.restoreSelection = function () {
        var editor = this;
        var selection = window.getSelection();
        if (editor.selectedRange) {
            selection.removeAllRanges();
            selection.addRange(this.selectedRange);
        }
    };

    /**
     * Returns clean HTML to update Text Model
     * 
     * @returns {} 
     */
    Editor.prototype.cleanHtml = function () {
        var html = this.element.innerHTML;
        return html && html.replace(/(<br>|\s|<div><br><\/div>|&nbsp;|)*$/, "");
    };

    /**
     * Inserts Model text when text editor is reopened
     * 
     * @param {} text 
     * @returns {} 
     */
    //Editor.prototype.InsertText = function (text) { // function to update text in the editor
    //    this.element.innerHTML = text;
    //};

    /**
     * Updates text node view in real-time with current text in editor
     * Current events: keyup, change, mousemove
     * Checks for link popover so that it does not appear in nodeview on link click.
     * 
     * @returns {} 
     */
    Editor.prototype.updateNodeView = function () {
        var editor = this;
        this.element.addEventListener("keyup", function () {
            if (!editor.cleanHtml().includes("popover")) {
                window.external.notify(editor.cleanHtml());
            }
        });
        this.element.addEventListener("change", function () {
            if (!editor.cleanHtml().includes("popover")) {
                window.external.notify(editor.cleanHtml());
            }
        });
        this.element.addEventListener("mouseover", function () {
            if (!editor.cleanHtml().includes("popover")) {
                window.external.notify(editor.cleanHtml());
            }
        });
        this.document.addEventListener("click", function () {
            if (!editor.cleanHtml().includes("popover")) {
                window.external.notify(editor.cleanHtml());
            }
        });
    };

    return Editor;
})();

window.onload = function () {
    var el = document.getElementById("editor");
    var prev = document.getElementById("preview");
    myEditor = new Editor(document, el, prev);
};
//# sourceMappingURL=app.js.map