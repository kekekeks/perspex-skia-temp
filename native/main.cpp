#define DEFINE_METHOD_TABLE
#include "common.h"
namespace libperspesk
{
	static SkGLContext* GlContext = NULL;
	extern GrContext* Context = NULL;
	static bool Initialized = false;;
	static void Init()
	{
		if (Initialized)
			return;
		Initialized = true;
		SkGraphics::Init();


#ifdef WIN32
		GlContext = SkANGLEGLContext::Create(GrGLStandard::kNone_GrGLStandard, false);
#else
		GlContext = SkCreatePlatformGLContext(GrGLStandard::kNone_GrGLStandard);
#endif
		if (GlContext == nullptr || !GlContext->isValid())
			return;
		GrBackendContext backend = reinterpret_cast<GrBackendContext>(GlContext->gl());
		Context = GrContext::Create(GrBackend::kOpenGL_GrBackend, backend);
	}
}

extern "C" __declspec(dllexport) void* GetPerspexMethodTable()
{
	Init();
	return &MethodTable;
}