using Ext.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Zeus.FileSystem.Images;
using Zeus.Web.UI;
using Zeus.Web.UI.WebControls;

namespace Zeus.Design.Editors
{
    public class ImageEditorAttribute : ChildEditorAttribute
    {
        public int MinWidth { get; set; }

        public int MinHeight { get; set; }

        public bool IncludeDefaultCrop { get; set; }

        public double DefaultAspectRatio { get; set; }

        public bool AllowCropping { get; set; } = true;

        public ImageEditorAttribute(string title, int sortOrder)
            : base(title, sortOrder)
        {
        }

        protected override Control AddEditor(Control panel)
        {
            var fieldset = new FieldSet
            {
                ID = $"{Name}FieldSet",
                Title = Title,
                Collapsible = false,
                LabelAlign = LabelAlign.Top,
                Padding = 5,
                LabelSeparator = " "
            };

            var editor = new ImageEditView();
            editor.ID = Name;
            editor.Init += OnChildEditorInit;
            editor.AllowCropping = AllowCropping;
            editor.Crops = GetCrops();
            editor.MinWidth = MinWidth;
            editor.MinHeight = MinHeight;

            panel.Controls.Add(fieldset);
            fieldset.ContentContainer.Controls.Add(editor);

            return editor;
        }

        /// <summary>
        /// Get crop settings from attributes alongside the ImageEditor attribute
        /// </summary>
        protected virtual ImageCropDefinition[] GetCrops()
        {
            if (!AllowCropping)
            {
                return Array.Empty<ImageCropDefinition>();
            }

            var cropAttributes = UnderlyingProperty?.GetCustomAttributes<ImageCropAttribute>().Where(x => !string.IsNullOrWhiteSpace(x.Id)) ?? Array.Empty<ImageCropAttribute>();

            if (!cropAttributes.Any() || (IncludeDefaultCrop && !cropAttributes.Any(x => x.Id == CroppedImage.DefaultCropId)))
            {
                // Add the default crop at the beginning if needed
                cropAttributes = cropAttributes.Prepend(new ImageCropAttribute(DefaultAspectRatio));
            }
            else if (DefaultAspectRatio > 0)
            {
                var defaultCrop = cropAttributes.FirstOrDefault(x => x.Id == CroppedImage.DefaultCropId && x.AspectRatio <= 0);

                if (defaultCrop != null)
                {
                    defaultCrop.AspectRatio = DefaultAspectRatio;
                }
            }
            
            return cropAttributes
                .Select(x => new ImageCropDefinition(x))
                .ToArray();
        }
    }
}
