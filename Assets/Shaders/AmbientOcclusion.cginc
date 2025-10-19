#include <HLSLSupport.cginc>

inline void GetAmbientOcclusion_float(fixed input, out fixed result)
{
	int t = clamp(input * 255, 0.0, 3.0);

	if (t == 0)
	{
		//result = float4(0,0,0,1);
		result = 0.1;
	}
	if (t == 1)
	{
		//result = float4(0.2,0,0,1);
		result = 0.6;
	}
	if (t == 2)
	{
		//result = float4(1,0,0,1);
		result = 0.8;
	}
	if (t == 3)
	{
		//result = float4(1,1,1,1);
		result = 1;
	}
}
