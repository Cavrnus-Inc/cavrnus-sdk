using System.Collections.Generic;
using UnityEngine;

public static class InternalShaderAccess
{
	public static Shader LitShader { get; } = Shader.Find("Cavrnus/Lit");
	public static Shader GlassShader { get; } = Shader.Find("Cavrnus/Glass");
	public static Shader UnlitShader { get; } = Shader.Find("Cavrnus/Unlit");
	public static Shader LineShader { get; } = Shader.Find("Custom/LineShader");
	
	public static Shader UiBlur { get; } = Shader.Find("Cavrnus/UiBlur");
}
