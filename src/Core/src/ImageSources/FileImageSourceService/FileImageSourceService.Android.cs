﻿using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Microsoft.Maui.BumptechGlide;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFileImageSource fileImageSource)
				return GetDrawableAsync(fileImageSource, context, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}

		public async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IFileImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var filename = imageSource.File;

			var result = await Glide
				.With(context)
				.Load(filename, context)
				.SubmitAsync(context, cancellationToken)
				.ConfigureAwait(false);

			return result;
		}
	}
}