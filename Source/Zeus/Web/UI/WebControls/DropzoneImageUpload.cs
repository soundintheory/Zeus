using SoundInTheory.DynamicImage.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zeus.Design.Editors;
using Zeus.FileSystem.Images;

namespace Zeus.Web.UI.WebControls
{
    public class DropzoneImageUpload : DropzoneUpload, IValidator
	{
        protected HiddenField _hiddenCropDataField;
        protected Dictionary<string, CropData> _currentCropData;

        public string FullSizeUrl { get; set; }

        public Dictionary<string, CropData> CurrentCropData
        {
			get
			{
				return _currentCropData;
			}
            set
            {
                _currentCropData = value;
                EnsureChildControls();
            }
        }

        public string UpdatedCropData
        {
            get
            {
                EnsureChildControls();
                return _hiddenCropDataField.Value;
            }
        }

        public override string TypeFilterDescription
		{
			get { return ViewState["TypeFilterDescription"] as string ?? "Images (*.jpg, *.jpeg, *.gif, *.png)"; }
			set { ViewState["TypeFilterDescription"] = value; }
		}

		public override string[] TypeFilter
		{
			get { return ViewState["TypeFilter"] as string[] ?? new[] { "image/jpg", "image/jpeg", "image/gif", "image/png" }; }
			set { ViewState["TypeFilter"] = value; }
		}

        public ImageCropDefinition[] Crops
        {
            get { return ViewState["Crops"] as ImageCropDefinition[] ?? Array.Empty<ImageCropDefinition>(); }
            set { ViewState["Crops"] = value; }
        }

        public bool AllowCropping
        {
            get { return ViewState["AllowCropping"] as bool? ?? false; }
            set { ViewState["AllowCropping"] = value; }
        }

        public int MinWidth
		{
			get { return ViewState["MinWidth"] as int? ?? 0; }
			set { ViewState["MinWidth"] = value; }
		}

		public int MinHeight
		{
			get { return ViewState["MinHeight"] as int? ?? 0; }
			set { ViewState["MinHeight"] = value; }
		}

        public int SourceWidth
        {
            get { return ViewState["SourceWidth"] as int? ?? 0; }
            set { ViewState["SourceWidth"] = value; }
        }

        public int SourceHeight
        {
            get { return ViewState["SourceHeight"] as int? ?? 0; }
            set { ViewState["SourceHeight"] = value; }
        }

        protected override void OnInit(EventArgs e)
		{
			Page.Validators.Add(this);
			base.OnInit(e);
		}

		public int CurrentID { get; set; }

        protected override void CreateChildControls()
        {
            _hiddenCropDataField = new HiddenField { ID = ID + "hdnCropData" };
            Controls.Add(_hiddenCropDataField);

            base.CreateChildControls();
        }

        protected override string GetExtraJS()
        {
			if (AllowCropping && Crops.Length > 0)
			{
                return "window.ImageEditor.initDropzone(dz);";
            }

			return "";
        }

		protected override Dictionary<string, object> GetConfig()
		{
			var config = base.GetConfig();
			config["allowCropping"] = AllowCropping;
			config["crops"] = Crops;
			config["minWidth"] = MinWidth;
			config["minHeight"] = MinHeight;
            config["cropDataFieldId"] = _hiddenCropDataField.ClientID;

            return config;
		}

        protected override Dictionary<string, object> GetCurrentFile()
        {
            var currentFile = base.GetCurrentFile();
			
			if (currentFile != null)
			{
				currentFile["crops"] = CurrentCropData;
				currentFile["fullSizeUrl"] = FullSizeUrl;
                currentFile["sourceWidth"] = SourceWidth;
                currentFile["sourceHeight"] = SourceHeight;
			}

			return currentFile;
        }

        #region IValidator Members

        /// <summary>Gets or sets the error message generated when the name editor contains invalid values.</summary>
        public string ErrorMessage
		{
			get { return (string)ViewState["ErrorMessage"] ?? string.Empty; }
			set { ViewState["ErrorMessage"] = value; }
		}

		/// <summary>Gets or sets whether the name editor's value passes validaton.</summary>
		public bool IsValid
		{
			get { return ViewState["IsValid"] != null ? (bool)ViewState["IsValid"] : true; }
			set { ViewState["IsValid"] = value; }
		}

		/// <summary>Validates the name editor's value checking uniqueness and lenght.</summary>
		public virtual void Validate()
		{
			if (!HasNewOrChangedFile)
			{
				return;
			}

			if (MinWidth == 0 && MinHeight == 0)
			{
				return;
			}

            using (var fileStream = new FileStream(FileUploadEditorAttribute.GetUploadedFilePath(this), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var image = System.Drawing.Image.FromStream(fileStream, false, false))
                {
                    if (MinWidth > 0 && image.Width < MinWidth)
                    {
                        ErrorMessage = $"Image width is less than the minimum width of {MinWidth} pixels.";
                        IsValid = false;
                        return;
                    }

                    if (MinHeight > 0 && image.Height < MinHeight)
                    {
                        ErrorMessage = $"Image height is less than the minimum height of {MinHeight} pixels.";
                        IsValid = false;
                        return;
                    }
                }
            }

			IsValid = true;
		}


		#endregion
	}
}
