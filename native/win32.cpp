#ifdef WIN32
#include "common.h"
#include "SkTHash.h"
namespace libperspesk
{

	class WindowRenderTarget : public RenderTarget
	{
	public:
		HWND hWnd;
		SkBitmap Bitmap;
		SkAutoTUnref<SkSurface> Surface;
		SkSurfaceProps SurfaceProps;
		bool isGpu;

		WindowRenderTarget(HWND hWnd, int width, int height)
			: SurfaceProps(SkSurfaceProps::kLegacyFontHost_InitType)
		{
			this->hWnd = hWnd;

			if (true)//Always create raster for now
			{
				isGpu = false;
				Resize(width, height);

			}

		}

		virtual void Resize(int width, int height)
		{
			if (!isGpu)
			{
				if (Bitmap.width() == width && Bitmap.height() == height)
					return;
				Bitmap.allocPixels(SkImageInfo::MakeN32(width, height, kPremul_SkAlphaType));
				SkSurface* pSurf = SkSurface::NewRasterDirect(Bitmap.info(), Bitmap.getPixels(), Bitmap.rowBytes());
				Surface.reset(pSurf);
			}
		}


		void Present()
		{
			if (!isGpu)
			{
				BITMAPINFO bmi;
				memset(&bmi, 0, sizeof(bmi));
				bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
				bmi.bmiHeader.biWidth = Bitmap.width();
				bmi.bmiHeader.biHeight = -Bitmap.height(); // top-down image
				bmi.bmiHeader.biPlanes = 1;
				bmi.bmiHeader.biBitCount = 32;
				bmi.bmiHeader.biCompression = BI_RGB;
				bmi.bmiHeader.biSizeImage = 0;

				HDC hdc = GetDC(hWnd);
				Bitmap.lockPixels();
				int ret = SetDIBitsToDevice(hdc,
					0, 0,
					Bitmap.width(), Bitmap.height(),
					0, 0,
					0, Bitmap.height(),
					Bitmap.getPixels(),
					&bmi,
					DIB_RGB_COLORS);
				Bitmap.unlockPixels();
				ReleaseDC(hWnd, hdc);
			}
		}

		class Context : public RenderingContext
		{
			WindowRenderTarget*Target;
		public:
			Context(WindowRenderTarget* target)
			{
				Target = target;
				Canvas = target->Surface->getCanvas();
			}

			~Context()
			{
				Target->Surface->getCanvas()->flush();
				Target->Present();
			}
		};

		virtual RenderingContext* CreateRenderingContext()
		{
			Surface->getCanvas()->restoreToCount(1);
			Surface->getCanvas()->save();
			Surface->getCanvas()->clear(SkColorSetARGB(0, 0, 0, 0));
			Surface->getCanvas()->resetMatrix();
			return new Context(this);
		}
	};

	extern RenderTarget* CreateRenderTarget(void* nativeHandle, int width, int height)
	{
		return new WindowRenderTarget((HWND)nativeHandle, width, height);
	}
}


#endif