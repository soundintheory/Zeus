(function() {

    // Images come cropped to these max sizes, so we use these to calculate the scale we're working at based on the source dimensions
    const maxImageWidth = 800;
    const maxImageHeight = 600;

    window.ImageEditor = { };

    window.ImageEditor.initDropzone = function(dz) {
        dz.on('addedfile', (file) => {
            file.imageEditor = file.imageEditor || new ImageEditor(dz, file);
        });
    }

    class ImageEditor {

        constructor(dz, file) {
            this.dz = dz;
            this.file = file;

            if (this.file.existing) {
                this.scale = Math.min(1, maxImageWidth / file.sourceWidth, maxImageHeight / file.sourceHeight);
            } else {
                this.scale = 1;
                this.file.crops = {};
            }

            if (this.dz.options.cropDataFieldId) {
                this.cropDataField = document.getElementById(this.dz.options.cropDataFieldId);
            }

            this.configureDropzone();
        }

        configureDropzone() {
            this.file._editLink = Dropzone.createElement(
                `<a class="dz-edit-link href="javascript:undefined;" title="Crop image" data-dz-edit></a>`
            );
            this.file.previewElement.appendChild(this.file._editLink);
            this.file._editLink.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.open();
            });
        }

        open() {
            this.modal = document.createElement('dialog');
            this.modal.closedBy = 'none';
            const modalContent = this.getModalContent();

            this.modal.id = 'edit-image-modal';
            this.modal.innerHTML = modalContent;
            document.body.appendChild(this.modal);

            this.modal.addEventListener('close', (e) => this.onClose());

            this.modal.addEventListener('click', (e) => {
                let trigger = e.target.closest('[data-action]');
                if (trigger && this[trigger.dataset.action]) {

                    this[trigger.dataset.action](trigger);
                }
            }, true);

            this.modal.showModal();

            this.resizeObserver = new ResizeObserver((entries) => {
                clearTimeout(this.resizeTimeout);
                this.resizeTimeout = setTimeout(() => {
                    this.reloadCropper();
                }, 50);
            });
            this.resizeObserver.observe(this.modal.querySelector('.canvas'));

            this.cropSelect = this.modal.querySelector('.crop-select');
            let initialCropId = this.dz.options.crops[0].id;

            if (this.cropSelect) {
                this.cropSelect.addEventListener('input', () => this.selectCrop(this.cropSelect.value));
            }

            this.zoomInput = this.modal.querySelector('.crop-zoom');

            if (this.zoomInput) {
                this.zoomInput.addEventListener('input', () => this.setZoom(parseInt(this.zoomInput.value) / 100));
            }

            this.selectCrop(initialCropId);
        }

        selectCrop(cropId) {

            if (cropId === this.currentCropId) {
                return;
            }

            this.currentCropId = cropId;

            if (this.cropper) {
                this.cropper.destroy();
                this.cropper = null;
            }

            this.initCropper();
        }

        getCurrentCropSettings() {
            if (!this.currentCropId) {
                return null;
            }
            return this.dz.options.crops.find(x => x.id === this.currentCropId);
        }

        getCurrentCropData() {
            if (!this.currentCropId || !this.file.crops[this.currentCropId]) {
                return null;
            }

            let data = this.file.crops[this.currentCropId];

            return {
                x: Math.round((data.x || 0) * this.scale),
                y: Math.round((data.y || 0) * this.scale),
                width: Math.round((data.w || 0) * this.scale),
                height: Math.round((data.h || 0) * this.scale),
                zoom: data.s || 1
            }
        }

        initCropper() {

            if (!this.modal || this.cropper) {
                return;
            }

            this.zoom = 1;
            this.zoomInput.value = 100;
            this.isReady = false;
            this.img = this.modal.querySelector('.canvas img');
            let cropSettings = this.getCurrentCropSettings();
            let cropData = this.getCurrentCropData();
            let hasCropData = this.hasCropData(cropData);

            const opts = {
                background: false,
                guides: false,
                autoCrop: false,
                scalable: false,
                rotatable: false,
                zoomOnWheel: false,
                toggleDragModeOnDblclick: false,
                responsive: false,
                restore: false,
                ready: () => {
                    if (cropData && cropData.zoom && cropData.zoom < 1) {
                        this.setZoom(cropData.zoom);
                    }
                    if (hasCropData) {
                        this.cropper.crop();
                        this.cropper.setData(cropData);
                    }
                    this.isReady = true;
                },
                crop: (event) => {
                    if (this.isReady) {
                        this.setCurrentCropDataDebounced(Object.assign(this.cropper.getData(), { zoom: this.zoom }));
                    }
                }
            };

            if (cropSettings) {

                if (cropSettings.aspectRatio) {
                    opts.aspectRatio = cropSettings.aspectRatio;

                    if (!hasCropData) {
                        opts.autoCrop = true;
                        opts.autoCropArea = 1;
                    }
                }

                if (cropSettings.minWidth) {
                    opts.minCropBoxWidth = cropSettings.minWidth;
                }

                if (cropSettings.minHeight) {
                    opts.minCropBoxHeight = cropSettings.minHeight;
                }
            }

            this.cropper = new Cropper(this.img, opts);
        }

        hasCropData(crop) {
            return crop && crop.width && crop.height;
        }

        setZoom(value) {
            value = Math.max(.05, Math.min(1, value || 0));
            this.zoomInput.value = Math.round(value * 100);
            if (value != this.zoom && this.cropper) {
                var imageData = this.cropper.getImageData();
                var containerData = this.cropper.getContainerData();
                var maxZoom = Math.min(containerData.width / imageData.naturalWidth, containerData.height / imageData.naturalHeight);
                this.zoom = value;
                this.cropper.zoomTo(maxZoom * value);
            }
        }

        reloadCropper() {
            if (this.cropper) {
                this.cropper.destroy();
                this.cropper = null;
            }
            this.initCropper();
        }

        setCurrentCropDataDebounced(data) {
            clearTimeout(this.setCropDataTimeout);
            this.setCropDataTimeout = setTimeout(() => this.setCurrentCropData(data), 30);
        }

        setCurrentCropData(data) {
            clearTimeout(this.setCropDataTimeout);
            if (!data || !this.currentCropId) {
                return;
            }
            let fileCrop = this.file.crops[this.currentCropId];

            if (!fileCrop) {
                fileCrop = this.file.crops[this.currentCropId] = {};
            }

            fileCrop.x = Math.round((data.x || 0) / this.scale);
            fileCrop.y = Math.round((data.y || 0) / this.scale);
            fileCrop.w = Math.round((data.width || 0) / this.scale);
            fileCrop.h = Math.round((data.height || 0) / this.scale);
            fileCrop.s = data.zoom || 1;

            if (this.cropDataField) {
                this.cropDataField.value = JSON.stringify(this.file.crops);
            }
        }

        onClose() {
            if (this.cropper) {
                this.cropper.destroy();
                this.cropper = null;
            }
            if (this.modal) {
                this.modal.remove();
                this.modal = null;
            }
        }

        close() {
            this.modal.close();
        }

        save() {
            this.close();
        }

        reset() {
            this.setCurrentCropData({ x: 0, y: 0, width: 0, height: 0, zoom: 1 });
            this.reloadCropper();
        }

        getModalContent() {
            var output = '';
            var cropSelect = '';

            if (this.dz.options.crops.length > 1) {
                let cropOptions = this.dz.options.crops.map((crop) => `<option value="${crop.id}">${!!crop.title ? crop.title : crop.id}</option>`).join('');
                cropSelect = `<div class="control"><label for="crop-select">Select Crop:</label> <select id="crop-select" class="crop-select">${cropOptions}</select></div>`;
            }

            let zoomInput = `<div class="control zoom-control"><label for="crop-zoom">Zoom:</label> <input type="range" id="crop-zoom" class="crop-zoom" min="5" max="100" value="90" step="1" /></div>`;
            let imageSrc = this.file.existing ? this.file.fullSizeUrl : this.file.dataURL;

            return `
                <div class="canvas">
                    <img src="${imageSrc}" alt="">
                </div>
                <div class="controls">
                    ${cropSelect}
                    ${zoomInput}
                    <button class="btn btn-info" data-action="reset"><i>&#x21BA;</i> Reset</button>
                    <button class="btn btn-success" data-action="save"><i>&#x2714;</i> Done</button>
                </div>
                <div class="top">
                    <h1 class="title">${this.file.name}</h1>
                </div>
            `;
        }
    }

})();