#version 330

uniform int layerCount;
uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform sampler2D alphaMap0;
uniform sampler2D alphaMap1;
uniform sampler2D alphaMap2;

in vec2 texCoord;
in vec3 vertexShading;

out vec4 outputColor;

void main()
{
	vec4 color = texture(texture0, texCoord);	
	vec4 layer;
	vec4 blend;
	
	if (layerCount > 1)
	{
		layer = texture(texture1, texCoord);
		blend = texture(alphaMap0, texCoord);
		color = mix(color, layer, blend);
	}
	
	if (layerCount > 2)
	{
		layer = texture(texture2, texCoord);
		blend = texture(alphaMap1, texCoord);
		color = mix(color, layer, blend);
	}

	if (layerCount > 3)
	{
		layer = texture(texture3, texCoord);
		blend = texture(alphaMap2, texCoord);
		color = mix(color, layer, blend);
	}
	
	outputColor = color * vec4(vertexShading, 1.0f);
}