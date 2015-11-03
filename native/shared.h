#include "SkColor.h"
#include "SkPoint.h"
namespace libperspesk {
	enum PerspexGeometryElementType
	{
		pgLineTo,
		pgArcTo,
		pgBezierTo,
		pgBeginFigure,
		pgEndFigure
	};

	enum PerspexBrushType
	{
		brSolid,
		brLinearGradient,
		brRadialGradient,
		brImage
	};


	enum PerspexImageType
	{
		piPng, piGif, piJpeg
	};

	enum PerspexGradientSpreadMethod
	{
		grPad,
		grReflect,
		grRepeat,
	};

	struct PerspexGeometryElement
	{
		PerspexGeometryElementType Type;
		SkPoint Points[3];
		bool Flag;
	};

#define PERSPEX_MAX_GRADIENT_STOPS 1024
	struct PerspexBrush
	{
		PerspexBrushType Type;
		double Opacity;
		uint32_t Color;
		bool Stroke;
		float StrokeThickness;

		int GradientStopCount;
		PerspexGradientSpreadMethod GradientSpreadMethod;
		SkPoint GradientStartPoint, GradientEndPoint;
		float GradientRadius;
		SkColor GradientStopColors[PERSPEX_MAX_GRADIENT_STOPS];
		float GradientStops[PERSPEX_MAX_GRADIENT_STOPS];
	};

	struct PerspexBuffer
	{
		int Size;
		char* Data;
	};

	struct PerspexDrawingContextSettings
	{
		double Opacity;
	};
}