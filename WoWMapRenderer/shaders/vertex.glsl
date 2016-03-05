#version 330

uniform mat4 projModelView_matrix;
uniform mat4 modelView_matrix;

in vec3 in_position;
in vec3 in_normal;

out vec3 normal;
out vec3 lighting;

void main(void)
{
	gl_Position = projModelView_matrix * vec4(in_position, 1.0f);

	// Apply lighting
	vec3 ambientLight = vec3(0.6f, 0.6f, 0.6f);
	vec3 directionalLightColor = vec3(0.5, 0.5, 0.75);
    vec3 directionalVector = vec3(0.85, 0.8, 0.75);

	vec4 transformedNormal = modelView_matrix * vec4(in_normal, 1.0f);
	float directional = max(dot(transformedNormal.xyz, directionalVector), 0.0f);
	lighting = ambientLight + (directionalLightColor * directional);
}
