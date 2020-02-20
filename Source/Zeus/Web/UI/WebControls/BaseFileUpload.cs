using Ext.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Zeus.Web.UI.WebControls
{
    public class BaseFileUpload : Control
    {
        public const int THUMBNAIL_WIDTH = 140;
        public const int THUMBNAIL_HEIGHT = 90;

        #region Fields

        protected HiddenField _hiddenFileNameField, _hiddenIdentifierField;
        protected string _currentFileName;

        #endregion

        #region Properties

        public string CurrentFileName
        {
            set
            {
                _currentFileName = value;
                EnsureChildControls();
            }
        }

        public string PreviewUrl { get; set; }

        public virtual string TypeFilterDescription
        {
            get { return ViewState["TypeFilterDescription"] as string; }
            set { ViewState["TypeFilterDescription"] = value; }
        }

        public virtual string[] TypeFilter
        {
            get { return ViewState["TypeFilter"] as string[] ?? new string[] { }; }
            set { ViewState["TypeFilter"] = value; }
        }

        public int MaximumFileSize
        {
            get { return (int)(ViewState["MaximumFileSize"] ?? 64); }
            set { ViewState["MaximumFileSize"] = value; }
        }

        public bool HasNewOrChangedFile
        {
            get { return !string.IsNullOrWhiteSpace(FileName) && FileName != "-1"; }
        }

        public bool HasDeletedFile
        {
            get { return FileName == "-1"; }
        }

        public string FileName
        {
            get
            {
                EnsureChildControls();
                return _hiddenFileNameField.Value;
            }
        }

        public string Identifier
        {
            get
            {
                EnsureChildControls();
                return _hiddenIdentifierField.Value;
            }
        }

        public bool Enabled
        {
            get { return (bool)(ViewState["Enabled"] ?? true); }
            set { ViewState["Enabled"] = value; }
        }

        protected string GetAnchorClientID()
        {
            return ClientID + "DemoAttach";
        }

        protected string GetListClientID()
        {
            return ClientID + "DemoList";
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            // TODO: Prevent edit page submission when uploads are in progress.

            base.OnLoad(e);
            if (Page.IsPostBack)
            {
                EnsureChildControls();
            }
        }
    }
}
