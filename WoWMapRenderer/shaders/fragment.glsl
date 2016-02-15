#version 330

in vec3 vertexShading;

out vec4 outputColor;

void main()
{
	vec4 color = texture(texture0, texCoord);	
	vec4 layer;
	vec4 blend;
	
	outputColor = color * vec4(vertexShading, 1.0f);
}