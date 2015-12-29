#version 330

in vec2 texcoord;

uniform sampler2D texture_sampler0;
uniform sampler2D texture_sampler1;
uniform sampler2D texture_sampler2;
uniform sampler2D texture_sampler3;

in vec3 vshading;

float Triangular(float f)
{
	f = f / 2.0f;
	if (f < 0.0f)
		return f + 1.0f;
	return 1.0f - f;
}

vec4 BiCubic(sampler2D textureSampler, vec2 TexCoord, float widthMinClamp, float widthMaxClamp)
{
    float fWidth = 256.0;
    float fHeight = 64.0;

    float texelSizeX = 1.0 / fWidth;
    float texelSizeY = 1.0 / fHeight;
    vec4 nSum = vec4(0.0f, 0.0f, 0.0f, 0.0f);
    vec4 nDenom = vec4(0.0f, 0.0f, 0.0f, 0.0f);
    float a = fract(TexCoord.x * fWidth);
    float b = fract(TexCoord.y * fHeight);
    for(int m = -1; m <= 2; m++)
    {
        for(int n = -1; n <= 2; n++)
        {
            vec2 coords = TexCoord + vec2(texelSizeX * float(m), texelSizeY * float(n));
            float xCoord = coords.x;
            xCoord = max(widthMinClamp, xCoord);
            xCoord = min(widthMaxClamp, xCoord);
            coords.x = xCoord;

            vec4 vecData = texture(textureSampler, coords);
            float f  = Triangular(float(m) - a);
            vec4 vecCooef1 = vec4(f, f, f, f);
            float f1 = Triangular(-(float(n) - b));
            vec4 vecCoeef2 = vec4(f1, f1, f1, f1);
            nSum = nSum + (vecData * vecCoeef2 * vecCooef1);
            nDenom = nDenom + (vecCoeef2 * vecCooef1);
        }
    }
    return nSum / nDenom;
}

out vec4 outputColor;

void main()
{
	vec4 tex0 = texture(texture_sampler0, texcoord);
	vec4 tex1 = texture(texture_sampler1, texcoord);
	vec4 tex2 = texture(texture_sampler2, texcoord);
	vec4 tex3 = texture(texture_sampler3, texcoord);

	// vec4 oColor = tex0; // * vec4(1.0f, 0.0f, 0.0f, 1.0f); // mix(mix(mix(tex0, tex1, 0.5f), tex2, 0.5f), tex3, 0.5f) *  vec4(vshading, 1.0f);
	vec4 oColor = vec4(1.0f, 0.0f, 0.0f, 1.0f);
	oColor.a = 1.0f; // Force alpha back to 1
	outputColor = oColor;
}