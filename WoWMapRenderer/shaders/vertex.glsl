#version 330

in vec3 position; /* Input vertex */
in vec3 vertex_shading; /* Vertex shading */
in vec2 texture_coordinates; /* texture coordinates */
in vec2 alpha_coordinates;

uniform mat4 projection_modelview; /* projection * modelview */

out vec2 texCoord;
out vec2 alphaCoordinates;
out vec3 vertexShading;

void main()
{
	vertexShading = vertex_shading;
	texCoord = texture_coordinates;
	alphaCoordinates = alpha_coordinates;

    gl_Position = projection_modelview * vec4(position, 1.0f);
}