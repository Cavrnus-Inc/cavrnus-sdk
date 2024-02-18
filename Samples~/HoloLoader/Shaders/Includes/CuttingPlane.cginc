bool getWithinAxis(float3 pos, float3 center, float3 axis, float size) {
	return abs(dot((pos - center), axis)) < size;
}

bool getWithinVolume(float3 worldPos, float3 volumeCenter, float3 volumeXAxis, float3 volumeZAxis, float3 volSize) 
{
	float3 volumeYAxis = cross(volumeXAxis, volumeZAxis);
	return getWithinAxis(worldPos, volumeCenter, volumeXAxis, volSize.x) &&
		getWithinAxis(worldPos, volumeCenter, volumeYAxis, volSize.y) &&
		getWithinAxis(worldPos, volumeCenter, volumeZAxis, volSize.z);
}

bool handleCullingVolume(float3 worldPos, float4 volumeCenter, float3 volumeXAxis, float3 volumeZAxis, float3 cullSize, float borderSize) {
	if (volumeCenter.w != -1) {
		bool withinBoundingVolume = getWithinVolume(worldPos, volumeCenter.xyz, volumeXAxis, volumeZAxis, cullSize/2);
		clip(withinBoundingVolume ? -1 : 1);
		float3 withBorderSize = cullSize/2 + float3(borderSize, borderSize, borderSize);
		return getWithinVolume(worldPos, volumeCenter.xyz, volumeXAxis, volumeZAxis, withBorderSize);
	}
	return false;
}