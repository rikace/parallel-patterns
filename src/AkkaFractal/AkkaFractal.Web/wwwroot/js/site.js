// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var init = function() {
    var container = $("#container");
    container.empty();

    var canvas = document.createElement("canvas");
    canvas.setAttribute("id", "tile-canvas");
    canvas.width = 4000;
    canvas.height = 4000;
    container.append(canvas);
};

$(function() {
    init();

    var source = new EventSource('/fractal-tiles');
    source.onmessage = function (event) {
        var json = JSON.parse(event.data);

        var image = new Image();
        image.onload = function() {
            var ctx = document.getElementById("tile-canvas").getContext("2d");
            ctx.drawImage(image, json.X, json.Y);
        };
        image.setAttribute('src', 'data:image/png;base64,'+json.ImageBase64);
    };

    $("#btnStart").click(function(event){
        event.preventDefault();
        $.get( "/run");
    });
    $("#btnReset").click(function(event){
        event.preventDefault();
        init();
    });
});
