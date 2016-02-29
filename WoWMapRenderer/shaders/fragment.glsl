#version 330

uniform vec4 meshColor = vec4(0.0, 1.0, 0.0, 1.0);

const vec3 lightDirection = vec3(0.6, 0.6, 0.6);

in vec3 normal;
out vec4 out_frag_color;

void main(void)
{
	out_frag_color = meshColor * clamp(dot(lightDirection, normal), 0.0, 1.0);
}