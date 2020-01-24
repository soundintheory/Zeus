body
{
}
.dnd-upload {
	/*border: 2px dashed #ccc;
	border-radius: 20px;*/
	width: 100%;
	min-height: 200px;
	/*padding: 20px;*/
}

	.dnd-upload:hover {
		border: none;
	}

	.dnd-upload.dnd-highlight {
		border-color: purple;
	}

	.dnd-upload label {
		left: 1%;
		display: block;
		top: 0;
		bottom: -3%;
		right: 1%;
		height: 200px;
		/*position: absolute;*/
		text-align: center;
		display: flex;
		justify-content: center; /* align horizontal */
		align-items: center;
		flex-direction: column;
		cursor: pointer;
		border: 2px dashed #ccc;
		border-radius: 20px;
	}

	.dnd-upload label:hover {
		background-color: rgba(255,255,255,1);
		border: 2px dashed purple;
		border-radius: 20px;
		color: rgba(255,255,255,1);
	}

	.dnd-upload label::before {
		padding-top: 10px;
		content: 'Drag a file here to upload';
		font-style: italic;
		color: #676a6c;
	}

	.dnd-upload label i {
		vertical-align: central;
	}


.dnd-gallery {
	margin-top: 10px;
}

	.dnd-gallery .img-preview {
		width: 150px;
		margin-bottom: 10px;
		margin-right: 10px;
		vertical-align: middle;
		position: relative;
	}

.img-preview::before {height: 20px;background: beige;z-index: 2;width: 150px;position: absolute;top: 0px;}

.dnd-input {
	display: none !important;
}

.progress-bar-wrapper {
	width: 80%;
	height: 20px;
	bottom: 0;
	position: absolute;
}

	.progress-bar-wrapper .progress-bar {
		border-radius: 5px;
		position: absolute;
		bottom: 25%;
	}