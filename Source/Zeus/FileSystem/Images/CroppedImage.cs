using System;
using System.Net;
using System.IO;
using SoundInTheory.DynamicImage;
using SoundInTheory.DynamicImage.Caching;
using SoundInTheory.DynamicImage.Filters;
using SoundInTheory.DynamicImage.Layers;
using SoundInTheory.DynamicImage.Sources;
using Zeus.ContentTypes;
using Zeus.Design.Editors;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Zeus.FileSystem.Images
{
    //the editor when this isn't hidden needs to understand that the crop values will be in the parent object

    [ContentType("User Cropped Image")]
    public class CroppedImage : Image
    {
        public const string DefaultCropId = "default";

        [CroppedImageUploadEditor("Image", 100)]
        public override byte[] Data
        {
            get { return base.Data; }
            set { base.Data = value; }
        }

        public override string IconUrl
        {
            get
            {
                return Utility.GetCooliteIconUrl(Ext.Net.Icon.PictureEdit);
            }
        }

        protected virtual Dictionary<string, CropData> Crops
        {
            get
            {
                var value = RawCropData;

                if (string.IsNullOrEmpty(value))
                {
                    return new Dictionary<string, CropData>();
                }

                try
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, CropData>>(value);
                }
                catch
                {
                    return new Dictionary<string, CropData>();
                }
            }
            set
            {
                RawCropData = JsonConvert.SerializeObject(value);
            }
        }

        public string RawCropData
        {
            get => GetDetail("Crops", default(string));
            set => SetDetail("Crops", value);
        }

        public virtual Dictionary<string, CropData> GetAllCropData() => Crops;

        public virtual CropData GetCrop(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return GetDefaultCrop();
            }

            return Crops.TryGetValue(key, out var cropData) ? cropData : new CropData();
        }

        public virtual void SetCrop(string key, int x, int y, int w, int h, float scale = 1)
            => SetCrop(key, new CropData { x = x, y = y, w = w, h = h, s = scale });

        public virtual void SetCrop(string key, CropData data)
        {
            if (data == null)
            {
                ResetCrop(key);
                return;
            }

            var crops = Crops;
            crops[key] = data;
            Crops = crops;
        }

        public virtual CropData GetDefaultCrop()
        {
            var crops = Crops;

            if (crops.TryGetValue(DefaultCropId, out var crop))
            {
                return crop;
            }

            return new CropData();
        }

        public virtual void SetDefaultCrop(string key, int x, int y, int w, int h, float scale = 1)
            => SetCrop(DefaultCropId, new CropData { x = x, y = y, w = w, h = h, s = scale });

        public virtual void SetDefaultCrop(string key, CropData data)
            => SetCrop(DefaultCropId, data);

        public virtual void ResetCrop(string key)
        {
            var crops = Crops;

            if (crops.Remove(key))
            {
                Crops = crops;
            }
        }

        public string GetCropUrl(int width, int height)
        {
            return GetCropUrl(null, width, height, true, DynamicImageFormat.Jpeg);
        }

        public string GetCropUrl(int width, int height, bool fill)
        {
            return GetCropUrl(null, width, height, fill, DynamicImageFormat.Jpeg);
        }

        public string GetCropUrl(int width, int height, bool fill, DynamicImageFormat format)
        {
            return GetCropUrl(null, width, height, fill, format);
        }

        public string GetCropUrl(string cropId, int width, int height)
        {
            return GetCropUrl(cropId, width, height, true, DynamicImageFormat.Jpeg);
        }

        public string GetCropUrl(string cropId, int width, int height, bool fill)
        {
            return GetCropUrl(cropId, width, height, fill, DynamicImageFormat.Jpeg);
        }

        public string GetCropUrl(string cropId, int width, int height, bool fill, DynamicImageFormat format)
        {
            return Context.Current.Cache.GetOrAdd(
                ID,
                $"CroppedImage.{ID}.{(!string.IsNullOrEmpty(cropId) ? cropId : DefaultCropId)}.{width}.{height}.{fill}.{format}",
                () => GetCropUrlInternal(cropId, width, height, fill, format)
            );
        }

        protected string GetCropUrlInternal(string cropId, int width, int height, bool fill, DynamicImageFormat format)
        {
            var crop = GetCrop(cropId);

            if (IsEmpty())
            {
                return "";
            }

            if (crop.IsEmpty)
            {
                return base.GetUrl(width, height, fill, format);
            }

            var imageSource = new ZeusImageSource(this);
            var dynamicImage = new Composition { ImageFormat = format };
            var imageLayer = new ImageLayer { Source = imageSource };

            dynamicImage.Layers.Add(imageLayer);

            // Add crop filter
            imageLayer.Filters.Add(new ZeusCropFilter
            {
                Enabled = true,
                X = crop.x,
                Y = crop.y,
                Width = crop.w,
                Height = crop.h
            });
            
            // Resize if necessary
            if (width > 0 || height > 0)
            {
                var resizeFilter = new ResizeFilter
                {
                    Mode = fill ? ResizeMode.UniformFill : ResizeMode.Uniform,
                    Width = width > 0 ? Unit.Pixel(width) : Unit.Empty,
                    Height = height > 0 ? Unit.Pixel(height) : Unit.Empty
                };

                imageLayer.Filters.Add(resizeFilter);
            }

            string imageUrl = ImageUrlGenerator.GetImageUrl(dynamicImage);

            return imageUrl;
        }
    }
}