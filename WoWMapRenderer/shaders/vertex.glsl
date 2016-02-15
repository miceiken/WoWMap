#version 330

in vec3 position; /* Input vertex */
in int type; /* vertex type */

uniform mat4 projection_modelview; /* projection * modelview */

flat out int vertex_type;

void main()
{
	vertex_type = type;

    gl_Position = projection_modelview * vec4(position, 1.0f);
}