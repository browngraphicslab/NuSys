class AbstractSelection implements ISelection {

    id: number;
    url: string;
    className: string;
    tags:string;
    public selectedElements:Array<any> = new Array<any>();

    constructor(className) {
        this.className = className;
    }

    start(x: number, y: number): void {}

    update(x: number, y: number): void {}

    end(x: number, y: number): void {}

    select(): void {
        console.log("select");
        this.selectedElements.forEach((selectedElement) => {
            var foundElement = $(selectedElement.tagName)[selectedElement.index];

            if (foundElement.tagName.toLowerCase() == "img") {
                var label = $("<span>Selected</span>");
                label.css({ position: "absolute", display: "block", background: "lightgrey", width: "50px", height: "20px", color: "black", "font-size": "12px" });
                $("body").append(label);
                label.css("top", $(foundElement).offset().top);
                label.css("left", $(foundElement).offset().left);
            } else {
                $(foundElement).css("background-color", "yellow");
            }

      
        });
    }

    deselect(): void {
        console.log("deselect");
        this.selectedElements.forEach((selectedElement) => {
            var foundElement = $(selectedElement.tagName)[selectedElement.index];
            $(foundElement).css("background-color", "");
        });
    }

    getBoundingRect(): Rectangle { return null; }

    analyzeContent(): void { }

    getContent(): string { return null; }

}