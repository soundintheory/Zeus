using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Zeus.FileSystem.Images
{
    public class CropData
    {
        public int x { get; set; }

        public int y { get; set; }

        public int w { get; set; }

        public int h { get; set; }

        public float s { get; set; }

        [JsonIgnore]
        public bool IsEmpty => w == 0 && h == 0;
    }
}
