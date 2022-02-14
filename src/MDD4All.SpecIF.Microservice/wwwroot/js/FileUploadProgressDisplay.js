var FileUploadProgressDisplay = /** @class */ (function () {
    function FileUploadProgressDisplay() {
        $("#startUpload").click(function () {
            console.debug("start upload click");
            $("#loading").removeAttr("hidden");
        });
    }
    return FileUploadProgressDisplay;
}());
