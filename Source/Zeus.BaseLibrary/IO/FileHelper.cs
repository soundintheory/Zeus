﻿using System;
using System.IO;

namespace Zeus.BaseLibrary.IO
{
	public static class FileHelper
	{
		public static string GetValidFileName(string title, string extension)
		{
			var validFileName = title.Trim();

			foreach (var invalChar in Path.GetInvalidFileNameChars())
			{
				validFileName = validFileName.Replace(invalChar.ToString(), string.Empty);
			}

			foreach (var invalChar in Path.GetInvalidPathChars())
			{
				validFileName = validFileName.Replace(invalChar.ToString(), string.Empty);
			}

			if (validFileName.Length > 250) //safe value threshold is 255
			{
				validFileName = validFileName.Remove(250);
			}

			return validFileName + "." + extension;
		}
	}
}