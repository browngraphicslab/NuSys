var Main = (function () {
    function Main() {
        console.log("Starting NuSys yo");
        this.init();
    }
    Main.prototype.init = function () {
        var body = document.body, html = document.documentElement;
        var dwidth = Math.max(body.scrollWidth, body.offsetWidth, html.clientWidth, html.scrollWidth, html.offsetWidth);
        var dheight = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
        var port = chrome.runtime.connect({ name: "content" });
        var canvas = document.createElement("canvas");
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.position = "fixed";
        canvas.style.top = "0";
    };
    return Main;
})();
