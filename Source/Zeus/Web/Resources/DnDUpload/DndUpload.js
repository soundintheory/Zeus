// ************************ Drag and drop ***************** //
/**
 * To use, create a form with id of <elementId> (change to desired id)
 * */
class DragAndDropUpload {
	constructor(elementId, identifier, fileId, uploadAction, existingFilename, typeFilters) {
		this.elementId = elementId;
		this.identifier = identifier;
		this.fileId = fileId;
		this.existingFilename = existingFilename;

		this.target = document.getElementById(elementId);

		this.target.appendChild(this.CreateElement(`<label id="${this.elementId}_wrapper" for="${this.elementId}_fileInput"></div>`));
		this.targetInner = document.getElementById(`${this.elementId}_wrapper`);

		this.uploadProgress = [];
		this.progressBar = "";
		this.uploadDestination = uploadAction;

		this.fileTypes = typeFilters ? typeFilters.map((filter) => `\\${filter.substring(1, filter.length)}`) : [];
		this.fileTypeRegex = new RegExp(`(${this.fileTypes.join("|")})`, "i");

		this.Init = this.Init.bind(this);
		this.AddHighlight = this.AddHighlight.bind(this);
		this.RemoveHighlight = this.RemoveHighlight.bind(this);
		this.HandleDrop = this.HandleDrop.bind(this);
		this.InitProgress = this.InitProgress.bind(this);
		this.UpdateProgress = this.UpdateProgress.bind(this);
		this.HandleFiles = this.HandleFiles.bind(this);
		this.PreviewFile = this.PreviewFile.bind(this);
		this.UploadFile = this.UploadFile.bind(this);
		this.UploadSuccessful = this.UploadSuccessful.bind(this);
		this.AddFileToGallery = this.AddFileToGallery.bind(this);
		this.AddExistingFileToGallery = this.AddExistingFileToGallery.bind(this);
		this.AddUploadedFileToGallery = this.AddUploadedFileToGallery.bind(this);
		this.HandleInputChange = this.HandleInputChange.bind(this);
		this.DeleteFile = this.DeleteFile.bind(this);
		this.Init();
	}

	CreateElement(elementString) {
		var template = document.createElement("template");
		elementString = elementString.trim();
		template.innerHTML = elementString;
		return template.content.firstChild;
	}

	Init() {
		this.target.appendChild(this.CreateElement(`<input type="file" id="${this.elementId}_fileInput" class="dnd-input"/>`));
		//this.targetInner.appendChild(this.CreateElement(`<i class="text-align: center">Drag files here to upload</i>`));
		this.target.parentElement.insertBefore(this.CreateElement(`<div id="${this.elementId}_gallery" class="dnd-gallery"/></div>`), this.target);
		this.targetInner.appendChild(this.CreateElement(`<div id="${this.elementId}_progress-bar-wrapper" class="progress-bar-wrapper"><div id="${this.elementId}_progress-bar" class="progress-bar progress-bar-striped progress-bar-animated progress-bar-danger" style="width: 0%;"></div></div>`));

		['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
			this.target.addEventListener(eventName, this.PreventDefaults, false);
			document.body.addEventListener(eventName, this.PreventDefaults, false);
		});
		['dragenter', 'dragover'].forEach(eventName => {
			this.target.addEventListener(eventName, this.AddHighlight, false);
		});
		['dragleave', 'drop'].forEach(eventName => {
			this.target.addEventListener(eventName, this.RemoveHighlight, false);
		});

		this.target.addEventListener("drop", this.HandleDrop, false);
		this.progressBar = document.getElementById(`${this.elementId}_progress-bar`);

		this.input = document.getElementById(`${this.elementId}_fileInput`);
		this.input.addEventListener("change", this.HandleInputChange, false);

		if (this.existingFilename) {
			this.AddExistingFileToGallery(this.existingFilename);
		}
	}
	PreventDefaults(e) {
		e.preventDefault();
		e.stopPropagation();
	}
	AddHighlight(e) {
		this.target.classList.add('dnd-highlight')
	}
	RemoveHighlight(e) {
		this.target.classList.remove("dnd-highlight");
	}
	HandleDrop(e) {
		var dt = e.dataTransfer;
		var files = dt.files;

		if (files.length > 1) {
			alert('Please upload only one image');
			return;
		}

		this.HandleFiles(files);
	}
	HandleInputChange(e) {
		e.preventDefault();
		let files = e.target.files;
		if (files && files.length > 0) {
			this.HandleFiles(files);
		}
	}

	InitProgress(numberOfFilesToUpload) {
		this.progressBar.value = 0;
		this.uploadProgress = [];

		this.progressBar.style.display = "block";

		for (let i = numberOfFilesToUpload; i > 0; i--) {
			this.uploadProgress.push(0);
		}
	}
	UpdateProgress(fileNumber, percent) {
		this.uploadProgress[fileNumber] = percent;
		let total = this.uploadProgress.reduce((tot, curr) => tot + curr, 0) / this.uploadProgress.length;
		console.debug('update', fileNumber, percent, total);
		this.progressBar.style.width = `${total}%`;
		if (total === 100) {
			this.progressBar.style.display = "none";
		}
	}
	HandleFiles(files) {
		files = [...files];

		// check size constraints

		for (let file of files) {
			if (file.size > 4000000) {
				alert("Please choose a smaller file! Max size is 4MB.");
				return;
			}
			if (!this.fileTypeRegex.test(file.name)) {
				alert(`Please choose a file of type: ${this.fileTypes.join(" ,")}`);
				return;
			}
		}

		this.InitProgress(files.length);
		files.forEach(this.UploadFile);
		files.forEach(this.PreviewFile);
	}

	PreviewFile(file) {
		let reader = new FileReader();
		reader.readAsDataURL(file);
		reader.onloadend = () => {
			this.AddUploadedFileToGallery(file, reader);
		};
	}

	AddUploadedFileToGallery(file, reader) {
		let img = document.createElement('img');
		img.src = reader.result;
		img.id = `${this.elementId}_image_${file.name.trim()}`;

		let uploadedImg = this.AddFileToGallery(img);
		uploadedImg.classList.add("dnd-uploading");
	}
	AddExistingFileToGallery(filename) {
		let img = document.createElement('img');
		img.src = `/Images/${filename}`;
		img.id = `${this.elementId}_image_${filename.trim()}`;

		this.AddFileToGallery(img);

		let preview = document.getElementById(`preview_${img.id}`);
		preview.classList.add("dnd-existing");
	}

	AddFileToGallery(img) {
		let gallery = document.getElementById(`${this.elementId}_gallery`);
		if (gallery.children.length > 0) {
			gallery.removeChild(gallery.firstElementChild);
		}

		let previewAnchor = document.createElement("a");
		previewAnchor.onclick = this.DeleteFile;
		previewAnchor.href = "#";

		let previewElement = document.createElement("div");
		previewElement.classList.add("img-preview");
		previewElement.appendChild(img);
		previewElement.id = `preview_${img.id}`;
		previewAnchor.appendChild(previewElement);
		return gallery.appendChild(previewAnchor);
	}

	DeleteFile(e) {
		e.preventDefault();
		document.getElementById(this.fileId).value = "-1";
		let gallery = document.getElementById(`${this.elementId}_gallery`);
		if (gallery.children.length > 0) {
			gallery.removeChild(gallery.firstElementChild);
		}
	}

	UploadSuccessful(event, response, i, name, imgId) {
		let img = document.getElementById(`preview_${imgId}`);
		img.classList.remove("dnd-uploading");

		document.getElementById(this.fileId).value = name;
	}

	UploadFile(file, i) {
		var xhr = new XMLHttpRequest();
		var formData = new FormData();
		xhr.open('POST', this.uploadDestination, true);
		xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

		// Update progress (can be used to show progress indicator)
		xhr.upload.addEventListener("progress", (e) => {
			this.UpdateProgress(i, (e.loaded * 100.0 / e.total) || 100);
		});

		xhr.addEventListener('readystatechange', (e) => {
			if (xhr.readyState === 4 && xhr.status === 200) {
				this.UpdateProgress(i, 100);
				this.UploadSuccessful(e, xhr.responseText, i, file.name.trim(), `${this.elementId}_image_${file.name.trim()}`);
			}
			else if (xhr.readyState === 4 && xhr.status !== 200) {
				alert("There was an error while uploading an image.");
			}
		});

		formData.append('image', file);
		formData.append('identifier', this.identifier);
		xhr.send(formData);
	}
}
