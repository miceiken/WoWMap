#version 330

uniform mat4 projModelView_matrix;
uniform mat4 normal_matrix;

in vec3 in_position;
in vec3 in_normal;

out vec3 normal;

void main(void)
{
	normal = normalize(normal_matrix * vec4(in_normal, 0)).xyz;

	gl_Position = projModelView_matrix * vec4(in_position, 1);
}