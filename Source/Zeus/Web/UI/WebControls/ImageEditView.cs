using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeus.Design.Editors;

namespace Zeus.Web.UI.WebControls
{
    public class ImageEditView : ItemEditView
    {
        public string Title { get; set; }

        public bool UseFieldset { get; set; }

        public int MinWidth { get; set; }

        public int MinHeight { get; set; }

        public ImageCropDefinition[] Crops { get; set; }

        public bool AllowCropping { get; set; } = true;

        public string Description { get; set; }

        public virtual string TypeFilterDescription
        {
            get { return ViewState["TypeFilterDescription"] as string; }
            set { ViewState["TypeFilterDescription"] = value; }
        }
    }
}
