class FileUploadProgressDisplay {

	constructor() {

		$("#startUpload").click(() => {
			console.debug("start upload click");
			$("#loading").removeAttr("hidden");

		});


	}
}